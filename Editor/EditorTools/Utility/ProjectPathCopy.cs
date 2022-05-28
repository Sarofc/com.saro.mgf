using System.Text;
using UnityEditor;
using UnityEngine;

namespace Saro.Utility
{
    public class ProjectPathCopy
    {
        [MenuItem("Assets/Copy Selection Path", false, 10)]
        private static void CopyPath()
        {
            var objs = Selection.objects;
            if (objs != null && objs.Length > 0)
            {
                var sb = new StringBuilder(1024);
                foreach (var item in objs)
                {
                    if (item == null) continue;

                    var path = AssetDatabase.GetAssetPath(item);
                    sb.AppendLine(path);
                }

                GUIUtility.systemCopyBuffer = sb.ToString();
            }
        }
    }
}