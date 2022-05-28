using System;
using System.Collections;

namespace Saro.Net
{
    public interface IDownloadAgent : IEnumerator
    {
        /// <summary>
        /// 当前文件流位置
        /// </summary>
        long Position { get; }

        /// <summary>
        /// 下载信息
        /// </summary>
        DownloadInfo Info { get; set; }

        /// <summary>
        /// 限速 TODO
        /// </summary>
        long SpeedLimit { get; set; }

        /// <summary>
        /// 下载完成回调（包括下载成功，下载失败，以及其他情况）
        /// </summary>
        event Action<IDownloadAgent> Completed;

        /// <summary>
        /// 完成
        /// </summary>
        bool IsDone { get; }

        /// <summary>
        /// 下载状态
        /// </summary>
        EDownloadStatus Status { get; }

        /// <summary>
        /// 错误信息，null or empty 时，表示成功
        /// </summary>
        string Error { get; }

        /// <summary>
        /// 下载进度
        /// </summary>
        float Progress { get; }

        /// <summary>
        /// 启动下载
        /// </summary>
        void Start();

        /// <summary>
        /// 下载完成，用户不要调用
        /// </summary>
        void Complete();

        /// <summary>
        /// 暂停下载
        /// </summary>
        void Pause();

        /// <summary>
        /// 恢复下载
        /// </summary>
        void Resume();

        /// <summary>
        /// 取消下载
        /// </summary>
        void Cancel();

        /// <summary>
        /// 轮询
        /// </summary>
        void Update();
    }
}