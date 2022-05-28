#if UNITY_EDITOR

using Newtonsoft.Json;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Saro.IO
{
    public class VfsDumper : EditorWindow
    {
        [MenuItem("MGF Tools/Debug/Vfs Dumper")]
        private static void Init()
        {
            GetWindow<VfsDumper>();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Dump Vfs info"))
            {
                var path = string.Empty;
                path = EditorUtility.OpenFilePanel("Select Vfs file", path, "*");
                if (!string.IsNullOrEmpty(path))
                {
                    using (var vfs = VFileSystem.Open(path, FileMode.Open, FileAccess.Read))
                    {
                        var infos = vfs.GetAllFileInfos();
                        var json = JsonConvert.SerializeObject(infos);
                        var dstpath = path + ".dump.json";
                        File.WriteAllText(dstpath, json);
                    }
                }
                else
                {
                    Debug.LogError("please select a valid file. path: " + path);
                }
            }
        }
    }
}


#endif