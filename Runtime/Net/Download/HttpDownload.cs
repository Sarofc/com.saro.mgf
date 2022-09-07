using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Saro.Net
{
    /*
     * TODO
     * 
     * 1.检测超时?
     * 2.切片下载测试
     * 3.限速
     * 4.先下载到临时文件，然后再move到目标文件
     * 
     */

    public sealed class HttpDownload : IDownloadAgent
    {
        // 通过buffer size 貌似可以限速？
        public static long s_ReadBufferSize = 1024 * 64;
        //public static long s_ReadBufferSize = 128;

        #region Instance

        private readonly byte[] m_ReadBuffer = new byte[s_ReadBufferSize];
        private FileStream m_Writer;

        public HttpDownload()
        {
            Status = EDownloadStatus.Wait;
            Position = 0;
        }

        public DownloadInfo Info { get; set; }
        public EDownloadStatus Status { get; private set; }
        public string Error { get; private set; }
        public event Action<IDownloadAgent> Completed;
        public long SpeedLimit { get => s_ReadBufferSize; set => s_ReadBufferSize = value; }

        // 也保证了回调调用完成
        public bool IsDone { get; private set; }
        public float Progress => Position * 1f / Info.Size;
        public long Position { get; private set; }

        private void Retry()
        {
            Status = EDownloadStatus.Wait;
            Start();
        }

        public void Resume()
        {
            Retry();
        }

        public void Pause()
        {
            Status = EDownloadStatus.Wait;
        }

        public void Cancel()
        {
            Error = "User Cancel.";
            Status = EDownloadStatus.Failed;
        }

        void IDownloadAgent.Complete()
        {
            if (Completed != null)
            {
                Completed(this);
                Completed = null;
            }

            IsDone = true;
        }

        public void Update() { }

        private void Run()
        {
            try
            {
                Downloading();
                CloseWrite();

                if (Status == EDownloadStatus.Wait) return;

                if (Status == EDownloadStatus.Failed) return;

                if (Position != Info.Size)
                {
                    Error = $"Download length {Position} mismatch to {Info.Size}";
                    Status = EDownloadStatus.Failed;

                    return;
                }

                MoveFile();

                Status = EDownloadStatus.Success;
            }
            catch (Exception e)
            {
                Error = "HttpDownload.Run failed\n" + e.Message;
                Status = EDownloadStatus.Failed;
            }
            finally
            {
                CloseWrite();
            }
        }

        private void CloseWrite()
        {
            if (m_Writer != null)
            {
                m_Writer.Flush();
                m_Writer.Close();
                m_Writer = null;
            }
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors spe)
        {
            return true;
        }

        private void MoveFile()
        {
            try
            {
                File.Move(Info.TempPath, Info.SavePath);
            }
            catch (Exception e)
            {
                Error = "MoveFile failed\n" + e.ToString();
                Status = EDownloadStatus.Failed;
            }
        }

        private void Downloading()
        {
            // TODO
            // 下载中，长时间没速度，也应该进行处理

            var request = CreateWebRequest();
            using (var response = request.GetResponse())
            {
                if (response.ContentLength > 0)
                {
                    if (Info.Size == 0) Info.Size = response.ContentLength + Position;

                    using (var reader = response.GetResponseStream())
                    {
                        if (Position < Info.Size)
                        {
                            while (Status == EDownloadStatus.Progressing)
                            {
                                if (ReadToEnd(reader))
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    Status = EDownloadStatus.Success;
                }
            }
        }

        private WebRequest CreateWebRequest()
        {
            if (Info.DownloadUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;
            }

            var request = GetHttpWebRequest();

            return request;
        }

        private WebRequest GetHttpWebRequest()
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(Info.DownloadUrl);
            httpWebRequest.ProtocolVersion = HttpVersion.Version11;

            var from = Info.Offset + Position;
            var to = Info.Offset + Info.Size;

            httpWebRequest.AddRange(from, to);

            httpWebRequest.Timeout = Info.Timeout == 0 ? System.Threading.Timeout.Infinite : Info.Timeout;

            return httpWebRequest;
        }

        private bool ReadToEnd(Stream reader)
        {
            var len = reader.Read(m_ReadBuffer, 0, m_ReadBuffer.Length);
            if (len > 0)
            {
                m_Writer.Write(m_ReadBuffer, 0, len);
                Position += len;
                return false;
            }

            return true;
        }

        public void Start()
        {
            if (Status != EDownloadStatus.Wait) return;

            IsDone = false;

            Status = EDownloadStatus.Progressing;

            // 检测正式文件
            var fileInfo = new FileInfo(Info.SavePath);
            if (fileInfo.Exists && fileInfo.Length > 0)
            {
                if (Info.Size > 0 && fileInfo.Length == Info.Size) // 大小一致姑且认为是对的，在Compelete回调里，再自己校验
                {
                    Position = Info.Size;
                    Status = EDownloadStatus.Success;
                    return;
                }
            }

            // 检测temp下载文件
            var tempFileInfo = new FileInfo(Info.TempPath);
            if (tempFileInfo.Exists && tempFileInfo.Length > 0)
            {
                if (Info.Size > 0 && tempFileInfo.Length == Info.Size) // 大小一致姑且认为是对的，在Compelete回调里，再自己校验
                {
                    // 这种情况，应该是较少的
                    Position = Info.Size;
                    MoveFile();
                    Status = EDownloadStatus.Success;
                    return;
                }

                // TODO
                // issue：
                // IOException: Sharing violation on path
                if (m_Writer == null)
                {
                    m_Writer = tempFileInfo.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                }

                bool newFile = false;
                if (Info.Size < tempFileInfo.Length) // 现有文件比远端文件大
                {
                    newFile = true;
                }
                else
                {
                    if (Info.UseRESUME)
                    {
                        Position = m_Writer.Length - 1;
                    }
                    else
                    {
                        newFile = true;
                    }
                }

                if (newFile)
                {
                    m_Writer.SetLength(0);
                    Position = 0;
                }

                if (!Info.IsValid(Position))
                {
                    Error = $"Invalid Range [{Info.Offset + Position}-{Info.Offset + Position + Info.Size}]";
                    Status = EDownloadStatus.Failed;
                    return;
                }

                if (Position > 0) m_Writer.Seek(-1, SeekOrigin.End);
            }
            else
            {
                if (!Info.IsValid(Position))
                {
                    Error = $"Invalid Range [{Info.Offset + Position}-{Info.Offset + Position + Info.Size}]";
                    Status = EDownloadStatus.Failed;
                    return;
                }

                var dir = Path.GetDirectoryName(Info.TempPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

                m_Writer = File.Create(Info.TempPath);
                Position = 0;
            }

            Task.Run(Run);
        }

        #endregion

        #region IEnumerator Impl

        bool IEnumerator.MoveNext() => !IsDone;

        void IEnumerator.Reset()
        { }

        object IEnumerator.Current => null;

        #endregion
    }
}