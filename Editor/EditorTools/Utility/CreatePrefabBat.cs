using System.IO;
using UnityEditor;
using UnityEngine;

namespace Saro.Utility
{

    public class CreatePrefabBat : EditorWindow
    {
        [MenuItem("MGF Tools/Utilities/CreatePrefab")]
        private static void ShowWindow()
        {
            var window = GetWindow<CreatePrefabBat>();
            window.Show();
        }

        private void OnGUI()
        {
            var selects = Selection.gameObjects;
            EditorGUILayout.LabelField($"选中 {selects.Length} 个物体.");

            EditorGUI.indentLevel++;
            foreach (var item in Selection.gameObjects)
            {
                EditorGUILayout.LabelField(item.ToString());
            }
            EditorGUI.indentLevel--;

            if (GUILayout.Button("Create Prefab"))
            {
                var path = EditorUtility.OpenFolderPanel("Save Path", "", "");
                if (path.Length != 0)
                {
                    var index = path.IndexOf("Assets");
                    path = path.Substring(index);
                }
                else
                {
                    return;
                }
                foreach (var item in Selection.gameObjects)
                {
                    PrefabUtility.SaveAsPrefabAssetAndConnect(item, Path.Combine(path, item.name + ".prefab"), InteractionMode.AutomatedAction, out bool success);
                    if (!success)
                    {
                        Debug.LogError("Save Prefab fail! " + item.name);
                    }
                }
            }
        }
    }
}