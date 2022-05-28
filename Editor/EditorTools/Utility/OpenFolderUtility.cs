using System.IO;
using UnityEditor;
using UnityEngine;

namespace Saro.Utility
{
    public class OpenFolderUtility
    {
        [MenuItem("MGF Tools/OpenFolder/PersistentDataPath")]
        private static void OpenPersistentDataPath()
        {
            OpenDirectory(Application.persistentDataPath);
        }

        [MenuItem("MGF Tools/OpenFolder/ExtraAssets")]
        private static void OpenExtraAssetsPath()
        {
            OpenDirectory("./ExtraAssets");
        }

        public static void OpenDirectory(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            path = path.Replace("/", "\\");
            if (!Directory.Exists(path))
            {
                Debug.LogError("No Directory: " + path);
                return;
            }

            System.Diagnostics.Process.Start("explorer.exe", path);
        }
    }
}