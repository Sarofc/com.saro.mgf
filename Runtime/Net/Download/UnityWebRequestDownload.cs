using System;
using System.Collections;
using System.IO;
using UnityEngine.Networking;

namespace Saro.Net
{
    /*
     * TODO
     * 
     * 1.检测超时?
     * 2.切片下载测试
     * 3.限速
     * 4.先下载到临时文件，然后再move到目标文件
     * （待测试）5.且后台，然后回来，报错卡住，可能时 handler 脚本的问题  
     * （待测试）6.需要确保CloseWrite一定要释放掉
     * 
     */

    /// <summary>
    /// 断点续传下载器
    /// </summary>
    public sealed class UnityWebRequestDownload : IDownloadAgent
    {
        public sealed class MyDownloadScript : DownloadHandlerScript
        {
            private UnityWebRequestDownload m_Download;

            public MyDownloadScript(UnityWebRequestDownload download, byte[] bytes) : base(bytes)
            {
                m_Download = download;
            }

            protected override float GetProgress()
            {
                return m_Download.Progress;
            }

            protected override bool ReceiveData(byte[] buffer, int dataLength)
            {
                return m_Download.ReceiveData(buffer, dataLength);
            }

            protected override void CompleteContent()
            {
                m_Download.CompleteContent();
            }
        }

        public long Position { get; private set; }

        public DownloadInfo Info { get; set; }
        public bool IsDone { get; private set; }
        public EDownloadStatus Status { get; private set; }

        public event Action<IDownloadAgent> Completed;
        public long SpeedLimit { get; set; }

        public float Progress => Position * 1f / Info.Size;


        private UnityWebRequest m_Request;
        private FileStream m_Writer;

        public static long s_ReadBufferSize = 1024 * 4;
        private readonly byte[] m_ReadBuffer = new byte[s_ReadBufferSize];

        public string Error { get; internal set; }

        public override string ToString()
        {
            return Info.ToString();
        }

        public void Start()
        {
            if (Status != EDownloadStatus.Wait) return;

            IsDone = false;
            Status = EDownloadStatus.Progressing;

            Error = null;

            var fileInfo = new FileInfo(Info.SavePath);

            if (fileInfo.Exists && fileInfo.Length > 0)
            {
                if (Info.Size > 0 && fileInfo.Length == Info.Size)
                {
                    Status = EDownloadStatus.Success;
                    Position = Info.Size;
                    return;
                }

                if (m_Writer == null)
                {
                    m_Writer = fileInfo.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                }

                bool newFile = false;
                if (Info.Size < fileInfo.Length) // 现有文件比远端文件大
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
                    Error = $"Invalid Range [{Info.Offset + Position }-{ Info.Offset + Position + Info.Size}]";
                    Status = EDownloadStatus.Failed;
                    return;
                }

                if (Position > 0) m_Writer.Seek(-1, SeekOrigin.End);
            }
            else
            {
                if (!Info.IsValid(Position))
                {
                    Error = $"Invalid Range [{Info.Offset + Position }-{ Info.Offset + Position + Info.Size}]";
                    Status = EDownloadStatus.Failed;
                    return;
                }

                Position = 0;

                if (!Info.IsValid(Position))
                {
                    Error = $"Invalid Range [{Info.Offset + Position }-{ Info.Offset + Position + Info.Size}]";
                    Status = EDownloadStatus.Failed;
                    return;
                }

                var dir = Path.GetDirectoryName(Info.SavePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

                m_Writer = File.Create(Info.SavePath);
            }

            m_Request = CreateHttpWebRequest();
        }

        public void Update()
        {
            if (Status == EDownloadStatus.Progressing)
            {
                if (m_Request.isDone && m_Request.downloadedBytes < (ulong)Info.Size)
                {
                    Error = "unknown error: downloadedBytes < len";
                }
                if (!string.IsNullOrEmpty(m_Request.error))
                {
                    Error = m_Request.error;
                }
            }
        }

        public void Pause()
        {
            Status = EDownloadStatus.Wait;
        }

        public void Resume()
        {
            Retry();
        }

        public void Cancel()
        {
            Error = "User Cancel.";
            Status = EDownloadStatus.Failed;
            CloseWrite();
        }

        void IDownloadAgent.Complete()
        {
            if (Completed != null)
            {
                Completed(this);
                Completed = null;
            }

            CloseWrite();

            IsDone = true;
        }

        private UnityWebRequest CreateHttpWebRequest()
        {
            var request = UnityWebRequest.Get(Info.DownloadUrl);
            request.downloadHandler = new MyDownloadScript(this, m_ReadBuffer);

            var from = Info.Offset + Position;
            var to = Info.Offset + Info.Size;

            request.SetRequestHeader("Range", $"bytes={from}-{to}");

            request.SendWebRequest();

            m_Request.timeout = Info.Timeout;

            return request;
        }

        private void CloseWrite()
        {
            if (m_Writer != null)
            {
                m_Writer.Close();
                m_Writer.Dispose();
                m_Writer = null;
            }
            if (m_Request != null)
            {
                m_Request.Abort();
                m_Request.Dispose();
                m_Request = null;
            }
        }

        private void Retry()
        {
            CloseWrite();
            Status = EDownloadStatus.Wait;
            Start();
        }

        #region Handler

        internal bool ReceiveData(byte[] buffer, int dataLength)
        {
            if (!string.IsNullOrEmpty(m_Request.error))
            {
                Error = m_Request.error;
                return false;
            }

            m_Writer.Write(buffer, 0, dataLength);
            Position += dataLength;

            return Status == EDownloadStatus.Progressing;
        }

        internal void CompleteContent()
        {
            Status = EDownloadStatus.Success;

            CloseWrite();
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