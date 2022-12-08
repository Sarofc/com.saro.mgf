using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Saro.EditorTools.Animation
{
    public class AnimationClipTool : EditorWindow
    {
        [MenuItem("MGF Tools/Animation/AnimationClip")]
        private static void ShowWindow()
        {
            var window = GetWindow<AnimationClipTool>();
            window.titleContent = new GUIContent("AnimationClipTool");
            window.Show();
        }

        public List<string> paths = new List<string>();

        private Vector2 m_ScrollPos;
        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Usage : Copy animation clip from FBX file.", MessageType.Info);

            EditorGUILayout.HelpBox("1. Drag folder(s) to paths label.\n2. Click process button.", MessageType.Info);

            var dragArea = GUILayoutUtility.GetRect(0f, 35f, GUILayout.ExpandWidth(true));
            EditorGUI.LabelField(dragArea, "Paths : drag folder(s) here.");

            m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);
            for (int i = 0; i < paths.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{i.ToString()}. ", GUILayout.Width(18));
                paths[i] = EditorGUILayout.TextField(paths[i]);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            if (Event.current.type == EventType.DragUpdated && dragArea.Contains(Event.current.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            }

            else if (Event.current.type == EventType.DragExited && dragArea.Contains(Event.current.mousePosition))
            {
                var dragPaths = DragAndDrop.paths;
                if (DragAndDrop.paths != null && dragPaths.Length > 0)
                {
                    for (int i = 0; i < dragPaths.Length; i++)
                    {
                        if (!paths.Contains(dragPaths[i]))
                            paths.Add(dragPaths[i]);
                    }
                }
            }

            if (GUILayout.Button("Clear all paths"))
            {
                paths.Clear();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Process"))
            {
                for (int i = 0; i < paths.Count; i++)
                {
                    var fbxDirs = Directory.GetFiles(paths[i], "*.fbx", SearchOption.AllDirectories);

                    for (int k = 0; k < fbxDirs.Length; k++)
                    {
                        var fbxClip = AnimationHelper.LoadAllAnimationClipsAtPath(fbxDirs[k]);

                        for (int j = 0; j < fbxClip.Count; j++)
                        {
                            if (fbxClip[j])
                            {
                                Debug.Log(paths[i] + " : " + fbxClip[j].name);

                                var clip = new AnimationClip();
                                EditorUtility.CopySerialized(fbxClip[j], clip);
                                AssetDatabase.CreateAsset(clip, paths[i] + "/" + fbxClip[j].name + ".anim");
                            }
                        }
                    }
                }
            }
        }
    }
}