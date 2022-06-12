#if UNITY_ANDROID && !UNITY_EDITOR
#define USE_INDEXES
#endif

#if UNITY_ANDROID
#define BUILD_INDEXES
#endif

using System.IO;
using System.Text;
using UnityEngine;

#if USE_INDEXES
using System;
using System.Collections.Generic;
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
        private const string k_IndexesName = "Indexes";

#if USE_INDEXES
        private const string k_AndroidFileSystemPrefixString = "jar:";
        private static HashSet<string> s_Indexes;
#endif

        static FileUtility()
        {
#if USE_INDEXES
            var file = Application.streamingAssetsPath + "/" + k_IndexesName;
            var request = UnityWebRequest.Get(file);
            request.SendWebRequest();
            while (!request.isDone) { }
            var content = request.downloadHandler.text;

            request.Dispose();

            var lines = content.Split('\n');

            s_Indexes = new HashSet<string>();
            foreach (var line in lines)
            {
                s_Indexes.Add(line);
            }

            Log.INFO($"[USE_INDEXES]. LoadIndexes.");
#endif
        }

        public static void BuildIndexes()
        {
#if BUILD_INDEXES
            var sb = new StringBuilder(2048);
            var files = Directory.GetFiles(Application.streamingAssetsPath, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (file.EndsWith(".meta")) continue;
                var res = file.Replace(Application.streamingAssetsPath, "").Replace("\\", "/");
                sb.Append(res).Append("\n");

                // win android 下， appendline 貌似是 \r\n 
            }

            File.WriteAllText(Application.streamingAssetsPath + "/" + k_IndexesName, sb.ToString());
#endif
        }

        public static string ReadAllText(string file)
        {
#if USE_INDEXES
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
#if USE_INDEXES
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

#if USE_INDEXES
            if (!exists)
            {
                if (s_Indexes == null)
                    return false;

                if (!IsAndroidStreammingAssetPath(path))
                    return false;

                var resPath = path.Remove(0, Application.streamingAssetsPath.Length);

                exists = s_Indexes.Contains(resPath);

                Log.INFO($"[USE_INDEXES] exists: {exists} index: {resPath} path: {path}");
            }
#endif

            return exists;
        }

        public static bool IsAndroidStreammingAssetPath(string file)
        {
#if USE_INDEXES
            if (file.StartsWith(k_AndroidFileSystemPrefixString, StringComparison.Ordinal))
            {
                return true;
            }
#endif

            return false;
        }
    }
}
