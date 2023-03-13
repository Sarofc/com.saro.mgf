namespace Saro.Net
{
    internal sealed class DownloaderDriver : IService
    {
        private void Main_onApplicationPause(bool value)
        {
            // unity退出后台，线程会挂起/退出，下载线程会gg
            if (value)
                Downloader.PauseAllDownloads();
            else
                Downloader.ResumeAllDownloads();
        }

        void IService.Awake()
        {
            Main.onApplicationPause += Main_onApplicationPause;
        }

        void IService.Update()
        {
            Downloader.OnUpdate();
        }

        void IService.Dispose()
        {
            Downloader.OnDispose();

            Main.onApplicationPause -= Main_onApplicationPause;
        }
    }
}