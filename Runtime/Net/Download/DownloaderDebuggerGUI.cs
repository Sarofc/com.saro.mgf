
using System.IO;
using UnityEngine;

namespace Saro.Net
{
    public sealed class DownloaderDebuggerGUI : MonoBehaviour
    {
        public Rect windowRect = new Rect(20, 20, 450, 1000); // TODO 这个可能要自适应一下

        private Vector2 m_Scroll;

        private void OnGUI()
        {
            windowRect = GUI.Window(0, windowRect, DoMyWindow, "Downloader");
        }

        private void DoMyWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            GUILayout.Label($"Total Download Count: {Downloader.Cache.Count}");
            GUILayout.Label($"Total Download Speed: {NetUtility.FormatBytes(Downloader.TotalDownloadSpeed)}/s");

            m_Scroll = GUILayout.BeginScrollView(m_Scroll);

            var caches = Downloader.Cache;
            foreach (var pair in caches)
            {
                var agent = pair.Value;
                GUILayout.Label(Path.GetFileName(agent.Info.DownloadUrl));

                GUILayout.HorizontalSlider(agent.Progress, 0f, 1f);
            }

            GUILayout.EndScrollView();
        }
    }
}