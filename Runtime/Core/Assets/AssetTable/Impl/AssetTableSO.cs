using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Saro.Attributes;
using Saro.Utility;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Saro.Core
{
    [CreateAssetMenu]
    public sealed partial class AssetTableSO : ScriptableObject, IAssetTable
    {
        [System.Serializable]
        public struct AssetInfo
        {
            public int assetID;
            public string assetPath;
        }

        public const string k_Path = "Assets/Res/AssetTable.asset";

        public List<AssetInfo> assetInfos = new();

        private Dictionary<int, string> m_AssetMap;

        public string GetAssetPath(int assetID)
        {
            EnsureAssetMap();

            m_AssetMap.TryGetValue(assetID, out var assetPath);

            return assetPath;
        }

        private void EnsureAssetMap()
        {
            if (m_AssetMap != null) return;

            m_AssetMap = new(assetInfos.Count);

            foreach (var info in assetInfos)
            {
                m_AssetMap.Add(info.assetID, info.assetPath);
            }

#if !UNITY_EDITOR
            // 打包后，加载完成后，释放掉数组内存
            assetInfos = null;
#endif
        }
    }

#if UNITY_EDITOR
    public partial class AssetTableSO
    {
        [AssetPath(typeof(UnityEngine.Object), true)]
        public string[] collectDirectors = new string[0];

        // public List<string> missing = new();
        public List<string> ignores = new();

        public static IAssetTable GetTable()
        {
            return AssetDatabase.LoadAssetAtPath<AssetTableSO>(k_Path);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void AutoCollect()
        {
            if (!EditorAssetTableGetter.AutoCollect()) return;

            var table = GetTable() as AssetTableSO;
            if (table == null)
            {
                table = CreateInstance<AssetTableSO>();
                AssetDatabase.CreateAsset(table, k_Path);
                Log.ERROR($"create AssetTableSO in {k_Path}");
                return;
            }
            table.Collect();
            Log.ERROR("AssetTableSO.AutoCollect");
        }

        [ContextMenu("Collect")]
        private void Collect()
        {
            var sw = new Stopwatch();
            sw.Start();

            if (assetInfos == null) assetInfos = new List<AssetInfo>();
            else assetInfos.Clear();

            // missing.Clear();
            ignores.Clear();

            for (int i = 0; i < collectDirectors.Length; i++)
            {
                var dir = collectDirectors[i];
                var files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    if (file.EndsWith(".meta")) continue;

                    var fileName = Path.GetFileName(file);
                    var fileNameSpan = fileName.AsSpan();
                    var strs = fileNameSpan.Split('_');
                    foreach (var str in strs)
                    {
                        if (int.TryParse(fileNameSpan[str], out var assetID))
                        {
                            assetInfos.Add(new AssetInfo
                            {
                                assetID = assetID,
                                assetPath = file.Replace(Application.dataPath, "").ReplaceFast('\\', '/')
                            });
                        }
                        else
                        {
                            ignores.Add(file.Replace(Application.dataPath, "").ReplaceFast('\\', '/'));
                        }

                        break;
                    }
                }
            }

            sw.Stop();
            Log.ERROR($"Collect AssetIDs. Cost: {sw.ElapsedMilliseconds} ms");

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }
    }
#endif
}