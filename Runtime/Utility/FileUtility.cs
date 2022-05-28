//#define UNITY_ANDROID

using System.IO;

#if UNITY_ANDROID && !UNITY_EDITOR // 不加宏，代码清理的时候，会砍掉，打包会报错
using System;
using UnityEngine.Networking;
#endif

namespace Saro.Utility
{
    // TODO
    // 待安卓真机测试
    // 与 直接使用 android jni 文件系统对比
    // webgl  可能也需要

    /// <summary>
    /// 跨平台文件处理，针对安卓StreammingAssests目录
    /// </summary>
    public static class FileUtility
    {
#if UNITY_ANDROID
        private const string k_AndroidFileSystemPrefixString = "jar:";

        private const string k_IndexesName = "Indexes";
        private static HashSet<string> s_Indexes;
#endif

        [System.Diagnostics.Conditional("UNITY_ANDROID")]
        public static void LoadIndexes()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var file = Application.streamingAssetsPath + "/" + k_IndexesName;
            var request = UnityWebRequest.Get(file);
            request.SendWebRequest();
            while (!request.isDone) { }
            var content = request.downloadHandler.text;
            var lines = content.Split('\n');

            s_Indexes = new HashSet<string>();
            foreach (var line in lines)
            {
                s_Indexes.Add(line);
            }
#endif
        }

        [System.Diagnostics.Conditional("UNITY_ANDROID")]
        public static void BuildIndexes()
        {
#if UNITY_ANDROID
            var sb = new StringBuilder(2048);
            var files = Directory.GetFiles(Application.streamingAssetsPath, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (file.EndsWith(".meta")) continue;
                var rePath = file.Replace(Application.streamingAssetsPath, "");
                sb.AppendLine(rePath);
            }

            File.WriteAllText(Application.streamingAssetsPath + "/" + k_IndexesName, sb.ToString());
#endif
        }

        public static string ReadAllText(string file)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (IsAndroidStreammingAssetPath(file))
            {
                var request = UnityWebRequest.Get(file);
                request.SendWebRequest();
                while (!request.isDone) { }
                return request.downloadHandler.text;
            }
#endif

            return File.ReadAllText(file);
        }

        public static byte[] ReadAllBytes(string file)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (IsAndroidStreammingAssetPath(file))
            {
                var request = UnityWebRequest.Get(file);
                request.SendWebRequest();
                while (!request.isDone) { }
                return request.downloadHandler.data;
            }
#endif

            return File.ReadAllBytes(file);
        }

        public static bool Exists(string path)
        {
            bool exists = File.Exists(path);

#if UNITY_ANDROID && !UNITY_EDITOR
            if (!exists)
            {
                if (s_Indexes == null)
                    return false;

                var rePath = path.Remove(0, Application.streamingAssetsPath.Length);
                if (s_Indexes.Contains(rePath))
                {
                    return true;
                }
            }
#endif

            return exists;
        }

        public static bool IsAndroidStreammingAssetPath(string file)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (file.StartsWith(k_AndroidFileSystemPrefixString, StringComparison.Ordinal))
            {
                return true;
            }
#endif

            return false;
        }
    }
}
