using System;
using System.Collections.Generic;

namespace Saro.Net
{
    public enum EDownloadStatus
    {
        Wait,
        Progressing,
        Success,
        Failed
    }

    public sealed class DownloadInfo
    {
        /// <summary>
        /// 下载链接
        /// </summary>
        public string DownloadUrl { get; set; }
        /// <summary>
        /// 保存地址
        /// </summary>
        public string SavePath { get; set; }
        /// <summary>
        /// 文件大小，字节
        /// </summary>
        public long Size { get; set; }
        /// <summary>
        /// 文件下载偏移，用于切片下载
        /// </summary>
        public long Offset { get; set; }
        /// <summary>
        /// 文件hash
        /// </summary>
        public string Hash { get; set; }
        /// <summary>
        /// 断点续传，默认开启 TODO 待测试
        /// </summary>
        public bool UseRESUME { get; set; } = true;
        /// <summary>
        /// 用户自定义数据
        /// </summary>
        public object UserData { get; set; }
        /// <summary>
        /// 下载限时，ms，0为不限时
        /// <code>默认值为0</code>
        /// </summary>
        public int Timeout { get; set; }
        /// <summary>
        /// 是否合法
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool IsValid(long position)
        {
            var min = Offset + position;
            var max = Offset + Size;

            if (min < 0) return false;

            return min < max;
        }

        public override string ToString()
        {
            return $"{DownloadUrl}|{Size}|{Hash}";
        }
    }

    // TODO
    // 1. 下载限速
    // 2. 搞个debug窗口，显示所有下载进度
    public static class Downloader
    {
        public static long s_SpeedLimit = 0;
        public static uint s_MaxDownloads = 2;

        private static readonly List<IDownloadAgent> s_Prepared = new List<IDownloadAgent>();
        private static readonly List<IDownloadAgent> s_Progressing = new List<IDownloadAgent>();
        private static readonly Dictionary<string, IDownloadAgent> s_Cache = new Dictionary<string, IDownloadAgent>();
        private static long s_LastSampleTime;
        private static long s_LastTotalDownloadedBytes;

        /// <summary>
        /// url to IDownloadAgent
        /// </summary>
        public static IReadOnlyDictionary<string, IDownloadAgent> Cache => s_Cache;
        public static bool Working => s_Progressing.Count > 0;

        /// <summary>
        /// 下载器实例 创建委托，自定义下载器，需要实现这个，默认使用 <see cref="HttpDownload"/>
        /// </summary>
        public static Func<IDownloadAgent> s_OnDownloadAgentFactory = () =>
        {
            return new HttpDownload();
        };

        /// <summary>
        /// 下载器 全局开始事件，可以用于统一处理下载回调
        /// </summary>
        public static event Action<IDownloadAgent> s_GlobalStarted;

        /// <summary>
        /// 下载器 全局完成事件，可以用于统一处理下载回调
        /// </summary>
        public static event Action<IDownloadAgent> s_GlobalCompleted;

        public static float TotalProgress
        {
            get
            {
                var cur = 0f;
                var all = 0f;
                foreach (var item in s_Cache)
                {
                    cur += item.Value.Position * 0.01f;
                    all += item.Value.Info.Size * 0.01f;
                }

                if (all <= 0.0001f) return 0f;

                return cur / all;
            }
        }

        public static long TotalDownloadedBytes
        {
            get
            {
                var value = 0L;
                foreach (var item in s_Cache) value += item.Value.Position;

                return value;
            }
        }

        public static long TotalSize
        {
            get
            {
                var value = 0L;
                foreach (var item in s_Cache) value += item.Value.Info.Size;

                return value;
            }
        }

        /// <summary>
        /// download speed per second  (byte/s)
        /// </summary>
        public static long TotalDownloadSpeed { get; private set; }

        private static readonly double[] k_ByteUnits =
        {
            1073741824.0, 1048576.0, 1024.0, 1
        };

        private static readonly string[] k_ByteUnitsNames =
        {
            "GB", "MB", "KB", "B"
        };

        public static string FormatBytes(long bytes)
        {
            var size = "0 B";
            if (bytes == 0) return size;

            for (var index = 0; index < k_ByteUnits.Length; index++)
            {
                var unit = k_ByteUnits[index];
                if (bytes >= unit)
                {
                    size = $"{bytes / unit:##.##} {k_ByteUnitsNames[index]}";
                    break;
                }
            }

            return size;
        }

        public static void Initialize()
        {
            DownloaderDriver.Create();
        }

        public static void ClearAllDownloads()
        {
            foreach (var download in s_Progressing) download.Cancel();

            s_Prepared.Clear();
            s_Progressing.Clear();
            s_Cache.Clear();
        }

        public static void PauseAllDownloads()
        {
            foreach (var download in s_Progressing) download.Pause();
        }

        public static void ResumeAllDownloads()
        {
            foreach (var download in s_Progressing) download.Resume();
        }

        public static IDownloadAgent DownloadAsync(DownloadInfo info)
        {
            if (!s_Cache.TryGetValue(info.DownloadUrl, out var download))
            {
                download = s_OnDownloadAgentFactory();
                download.Info = info;

                // 限速
                if (s_SpeedLimit != 0)
                {
                    download.SpeedLimit = s_SpeedLimit;
                }

                s_Prepared.Add(download);
                s_Cache.Add(info.DownloadUrl, download);
            }
            else
            {
                WARN($"Download url {info.DownloadUrl} already exist.");
            }

            return download;
        }

        public static void Update()
        {
            if (s_Prepared.Count > 0)
            {
                for (var index = 0; index < Math.Min(s_Prepared.Count, s_MaxDownloads - s_Progressing.Count); index++)
                {
                    var download = s_Prepared[index];
                    s_Prepared.RemoveAt(index--);
                    s_Progressing.Add(download);
                    download.Start();
                    s_GlobalStarted?.Invoke(download);
                }
            }

            if (s_Progressing.Count > 0)
            {
                for (var index = 0; index < s_Progressing.Count; index++)
                {
                    var download = s_Progressing[index];

                    download.Update();

                    if (download.Status == EDownloadStatus.Failed || download.Status == EDownloadStatus.Success)
                    {
                        s_Progressing.RemoveAt(index--);
                        download.Complete();
                        s_GlobalCompleted?.Invoke(download);

                        // 这时候，download才是真正的结束！
                    }
                }
            }
            else
            {
                // s_Cache不清空，有需求，使用 ClearAllDownloads 即可。
                //if (s_Cache.Count <= 0) return;
                //s_Cache.Clear();

                //s_LastTotalDownloadedBytes = 0;
                //s_LastSampleTime = DateTime.Now.Ticks;
            }

            SampleTest();
        }


        public static void Destroy()
        {
            ClearAllDownloads();

            s_GlobalStarted = null;
            s_GlobalCompleted = null;
        }

        private static void SampleTest()
        {
            if (DateTime.Now.Ticks - s_LastSampleTime >= 10000000) // 每 1s sample 一次
            {
                TotalDownloadSpeed = TotalDownloadedBytes - s_LastTotalDownloadedBytes;
                s_LastTotalDownloadedBytes = TotalDownloadedBytes;
                s_LastSampleTime = DateTime.Now.Ticks;
            }
        }

        #region Log

        [System.Diagnostics.Conditional(Log.k_ScriptDefineSymbol)]
        public static void INFO(string msg)
        {
            Log.INFO("Download", msg);
        }

        [System.Diagnostics.Conditional(Log.k_ScriptDefineSymbol)]
        public static void WARN(string msg)
        {
            Log.WARN("Download", msg);
        }

        #endregion
    }
}