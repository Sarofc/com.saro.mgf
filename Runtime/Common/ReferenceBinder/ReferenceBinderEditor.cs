#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Saro
{
    [CustomEditor(typeof(ReferenceBinder), true)]
    [CanEditMultipleObjects]
    public class ReferenceBinderEditor : Editor
    {
        protected ReferenceBinder m_ReferenceBinder => (ReferenceBinder)target;

        protected const string k_AutoReferencePatternValueKey = "mgf_AutoReferencePatternValueKey";
        protected const string k_DefaultAutoReferencePatternValue = "go_;btn_;";
        protected List<string> m_ReferencePatterns = new List<string>();

        protected virtual void OnEnable()
        {

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("添加"))
                {
                    m_ReferenceBinder.Add(Guid.NewGuid().GetHashCode().ToString(), null);
                }
                if (GUILayout.Button("全部删除"))
                {
                    m_ReferenceBinder.DelAll();
                }
                if (GUILayout.Button("删除空"))
                {
                    m_ReferenceBinder.DelNull();
                }
                if (GUILayout.Button("排序"))
                {
                    m_ReferenceBinder.Sort();
                }
                if (GUILayout.Button("生成代码"))
                {
                    CodeGen();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                var patternValue = OnPatternStringGUI();

                if (GUILayout.Button("规则添加"))
                {
                    m_ReferenceBinder.DelAll();
                    AutoReference(patternValue);
                    EditorUtility.SetDirty(target);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Datas"));

            var eventType = Event.current.type;
            //在Inspector 窗口上创建区域，向区域拖拽资源对象，获取到拖拽到区域的对象
            if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
            {
                // Show a copy icon on the drag
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (eventType == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (var o in DragAndDrop.objectReferences)
                    {
                        m_ReferenceBinder.Add(o.name, o);
                    }
                }

                Event.current.Use();
            }

            serializedObject.ApplyModifiedProperties();
            serializedObject.UpdateIfRequiredOrScript();
        }

        protected virtual string OnPatternStringGUI()
        {
            var patternValue = PlayerPrefs.GetString(k_AutoReferencePatternValueKey, k_DefaultAutoReferencePatternValue);

            EditorGUI.BeginChangeCheck();

            patternValue = EditorGUILayout.TextField(GUIContent.none, patternValue);
            if (EditorGUI.EndChangeCheck())
            {
                PlayerPrefs.SetString(k_AutoReferencePatternValueKey, patternValue);
            }

            return patternValue;
        }

        protected virtual void CodeGen()
        {
            var Datas = m_ReferenceBinder.Datas;
            var sb = new StringBuilder(1024);

            sb.AppendLine("private ReferenceCollector Binder;");
            foreach (var data in Datas)
            {
                sb.AppendFormat("private {0} {1};\n", data.obj.GetType(), data.key);
            }

            sb.AppendLine("void GetComps()");
            sb.AppendLine("{");
            foreach (var data in Datas)
            {
                sb.AppendFormat("\t{0} = Binder.Get<{1}>(\"{0}\");\n", data.key, data.obj.GetType());
            }
            sb.AppendLine("}");

            var code = sb.ToString();
            GUIUtility.systemCopyBuffer = code;

            Debug.LogError(code);
        }

        protected virtual void AutoReference(string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return;

            m_ReferencePatterns.Clear();
            var patterns = pattern.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in patterns)
            {
                m_ReferencePatterns.Add(item);
            }

            PostIteratorTree(m_ReferencePatterns, m_ReferenceBinder.gameObject.transform, (_pattern, _child) =>
            {
                m_ReferenceBinder.Add(_child.name, _child.gameObject);
            });
        }

        protected void PostIteratorTree(IList<string> patterns, Transform root, Action<string, Transform> callback, int depth = -1)
        {
            if (root == null) return;
            depth++;

            foreach (var pattern in patterns)
            {
                if (root.name.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    if (depth == 0 && root.GetComponent<ReferenceBinder>()) continue;
                    callback(pattern, root);
                }
            }
            foreach (Transform child in root)
            {
                if (child == root) continue;

                if (depth > 0 && root.GetComponent<ReferenceBinder>()) continue;

                PostIteratorTree(patterns, child, callback, depth);
            }
        }
    }

    [CustomPropertyDrawer(typeof(ReferenceBinder.ReferenceBinderData))]
    internal class ReferenceBinderDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var keyProp = property.FindPropertyRelative("key");
            var objProp = property.FindPropertyRelative("obj");

            var keyRect = position;
            keyRect.width = position.width * 0.5f;

            var objRect = position;
            var width = keyRect.width + 5f;
            objRect.x += width;
            objRect.width -= width;

            EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none);
            EditorGUI.PropertyField(objRect, objProp, GUIContent.none);
        }
    }

}

#endif