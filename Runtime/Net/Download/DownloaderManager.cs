
namespace Saro.Net
{
    public sealed class DownloaderManager : IService
    {
        // TODO 封装 Downloader 里面的常用方法

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
            Downloader.Update();
        }

        void IService.Dispose()
        {
            Downloader.Destroy();

            Main.onApplicationPause -= Main_onApplicationPause;
        }
    }
}