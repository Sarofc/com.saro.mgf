using System;

namespace Saro.Net
{
    internal sealed class DownloaderDriver : IService, IServiceAwake, IServiceUpdate, IDisposable
    {
        private void Main_onApplicationPause(bool value)
        {
            // unity退出后台，线程会挂起/退出，下载线程会gg
            if (value)
                Downloader.PauseAllDownloads();
            else
                Downloader.ResumeAllDownloads();
        }

        void IServiceAwake.Awake()
        {
            Main.onApplicationPause += Main_onApplicationPause;
        }

        void IServiceUpdate.Update()
        {
            Downloader.OnUpdate();
        }

        void IDisposable.Dispose()
        {
            Downloader.OnDispose();

            Main.onApplicationPause -= Main_onApplicationPause;
        }
    }
}