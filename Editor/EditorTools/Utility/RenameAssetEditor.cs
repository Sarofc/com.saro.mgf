using UnityEditor;
using UnityEngine;

namespace Saro.SEditor
{
    public class RenameAssetEditor : EditorWindow
    {
        public string toReplace = "";
        public string replaceTo = "";
        private Vector2 m_Scroll;

        [MenuItem("MGF Tools/Utilities/RenameAsset")]
        private static void Init()
        {
            GetWindow<RenameAssetEditor>().Show();
        }

        private void OnGUI()
        {
            toReplace = EditorGUILayout.TextField("toReplace: ", toReplace);
            replaceTo = EditorGUILayout.TextField("replaceTo: ", replaceTo);
            var selecteds = Selection.objects;

            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
            foreach (var item in selecteds)
            {
                if (item.name.Contains(toReplace))
                    EditorGUILayout.ObjectField(item, typeof(UnityEngine.Object), false);
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Replace", GUILayout.Width(80)))
            {
                try
                {
                    AssetDatabase.StartAssetEditing();
                    foreach (var item in selecteds)
                    {
                        var path = AssetDatabase.GetAssetPath(item);
                        var newName = item.name.Replace(toReplace, replaceTo);
                        AssetDatabase.RenameAsset(path, newName);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                }
            }
        }
    }
}