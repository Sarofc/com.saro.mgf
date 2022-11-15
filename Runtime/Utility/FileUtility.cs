#if (UNITY_ANDROID || UNITY_WEBGL) && !UNITY_EDITOR
#define USE_INDEXES
#endif

#if UNITY_ANDROID || UNITY_WEBGL
#define BUILD_INDEXES
#endif

using System.IO;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace Saro.Utility
{
    // TODO
    // 待安卓真机测试
    // 与 直接使用 android jni 文件系统对比
    // webgl  可能也需要

    /// <summary>
    /// 跨平台文件处理，针对StreammingAssests目录
    /// <code>webgl只能使用async方法</code>
    /// </summary>
    public static class FileUtility
    {
        private const string k_IndexesName = "Indexes";

#if USE_INDEXES
        private const string k_AndroidFileSystemPrefixString = "jar:";
        private const string k_HttpPrefixString = "http";
        private static HashSet<string> s_Indexes;
#endif

        public static async UniTask LoadIndexesAsync()
        {
#if USE_INDEXES
            Log.INFO($"[USE_INDEXES]. LoadIndexes.");

            var indexPath = Application.streamingAssetsPath + "/" + k_IndexesName;

            var request = UnityWebRequest.Get(indexPath);
            await request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                var content = request.downloadHandler.text;
                var lines = content.Split('\n');

                s_Indexes = new HashSet<string>();
                foreach (var line in lines)
                    s_Indexes.Add(line);
            }
            else
            {
                Log.ERROR($"[USE_INDEXES]. LoadIndexes Error: {request.error}");
            }
#endif
        }

        public static void BuildIndexes()
        {
            var indexPath = Application.streamingAssetsPath + "/" + k_IndexesName;
            if (File.Exists(indexPath))
                File.Delete(indexPath);

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

            File.WriteAllText(indexPath, sb.ToString());
#endif
        }

        public static string ReadAllText(string file)
        {
#if USE_INDEXES
            if (ShouldUseUnityWebRequest(file))
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
            if (ShouldUseUnityWebRequest(file))
            {
                var request = UnityWebRequest.Get(file);
                request.SendWebRequest();
                while (!request.isDone) { }
                return request.downloadHandler.data;
            }
#endif
            return File.ReadAllBytes(file);
        }

        public static async UniTask<string> ReadAllTextAsync(string file)
        {
#if USE_INDEXES
            if (ShouldUseUnityWebRequest(file))
            {
                var request = UnityWebRequest.Get(file);
                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    return request.downloadHandler.text;
                }
                else
                {
                    Log.ERROR($"FileUtility:ReadAllBytesAsync failed. file: {file} error: {request.error}");
                    return null;
                }
            }
#endif
            return await File.ReadAllTextAsync(file);
        }

        public static async UniTask<byte[]> ReadAllBytesAsync(string file)
        {
#if USE_INDEXES
            if (ShouldUseUnityWebRequest(file))
            {
                var request = UnityWebRequest.Get(file);
                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    return request.downloadHandler.data;
                }
                else
                {
                    Log.ERROR($"FileUtility:ReadAllBytesAsync failed. file: {file} error: {request.error}");
                    return null;
                }
            }
#endif
            return await File.ReadAllBytesAsync(file);
        }

        public static bool Exists(string path)
        {
            bool exists = File.Exists(path);

#if USE_INDEXES
            if (!exists)
            {
                if (s_Indexes == null)
                    return false;

                if (!ShouldUseUnityWebRequest(path))
                    return false;

                var resPath = path.Remove(0, Application.streamingAssetsPath.Length);

                exists = s_Indexes.Contains(resPath);

                // Log.INFO($"[USE_INDEXES] exists: {exists} index: {resPath} path: {path}");
            }
#endif

            return exists;
        }

        public static bool ShouldUseUnityWebRequest(string file)
        {
#if USE_INDEXES && UNITY_WEBGL
            if (IsHttpFile(file))
                return true;
#endif

#if USE_INDEXES && UNITY_ANDROID
            if (IsAndroidStreamingAssetFile(file))
                return true;
#endif

            return false;
        }


        public static bool IsHttpFile(string file)
        {
#if USE_INDEXES && UNITY_WEBGL
            if (file.StartsWith(k_HttpPrefixString, StringComparison.Ordinal))
                return true;
#endif
            return false;
        }

        public static bool IsAndroidStreamingAssetFile(string file)
        {
#if USE_INDEXES && UNITY_ANDROID
            if (file.StartsWith(k_AndroidFileSystemPrefixString, StringComparison.Ordinal))
                return true;
#endif
            return false;
        }
    }
}
