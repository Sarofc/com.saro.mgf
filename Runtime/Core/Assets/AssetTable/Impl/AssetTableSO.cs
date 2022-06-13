using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Saro.Attributes;
using Saro.Utility;
using UnityEngine;
using System.Text;

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

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Searchable]
        [Sirenix.OdinInspector.ListDrawerSettings(NumberOfItemsPerPage = 10)]
#endif
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

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Searchable]
        [Sirenix.OdinInspector.ListDrawerSettings(NumberOfItemsPerPage = 10)]
#endif
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
            //Log.ERROR("AssetTableSO.AutoCollect");
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.PropertySpace(25f)]
        [Sirenix.OdinInspector.Button("Gen AssetIDs Script")]
#else
        [ContextMenu("Gen AssetIDs Script")]
#endif
        private void GenAssetIDScript()
        {
            try
            {
                var path = EditorAssetTableGetter.GenAssetIDsPath;

                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var sb = new StringBuilder(1024);

                var ns = string.IsNullOrEmpty(EditorSettings.projectGenerationRootNamespace) ?
                    "Saro"
                    : EditorSettings.projectGenerationRootNamespace;

                sb.AppendLine("// auto gen. don't modify.");
                sb.AppendLine($"namespace {ns}");
                sb.AppendLine("{");
                sb.AppendLine("\tpublic static class AssetIDs");
                sb.AppendLine("\t{");
                foreach (var item in assetInfos)
                {
                    foreach (var collectDir in collectDirectors)
                    {
                        if (item.assetPath.StartsWith(collectDir))
                        {
                            var subStartIndex = collectDir.Length + 1;
                            var fieldName = Path.GetFileName(item.assetPath).ReplaceFast('-', '_').ReplaceFast('/', '_').ReplaceFast('.', '_');
                            sb.Append($"\t\tpublic const int _{fieldName} = {item.assetID};")
                            .AppendLine();

                            break;
                        }
                    }
                }
                sb.AppendLine("\t}");
                sb.AppendLine("}");

                File.WriteAllText(path, sb.ToString());

                AssetDatabase.SaveAssets();
            }
            catch (Exception e)
            {
                Log.ERROR(e);
            }
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button("Collect Assets")]
#else
        [ContextMenu("Collect Assets")]
#endif
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