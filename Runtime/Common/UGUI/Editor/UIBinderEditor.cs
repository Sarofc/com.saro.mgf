#if UNITY_EDITOR
using Saro.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Saro.UI
{
    [CustomEditor(typeof(UIBinder))]
    public sealed class UIBinderEditor : ReferenceBinderEditor
    {
        private Dictionary<string, Type> m_BindTypes = new Dictionary<string, Type>();

        private SerializedProperty m_UIScript;

        protected override void OnEnable()
        {
            base.OnEnable();

            var allTypes = TypeUtility.GetSubClassTypesAllAssemblies(typeof(IUIBindProcessor));

            m_BindTypes.Clear();

            foreach (var type in allTypes)
            {
                var processor = Activator.CreateInstance(type) as IUIBindProcessor;
                if (processor != null)
                {
                    m_BindTypes.Merge(processor.Binds);
                }
            }

            m_ReferencePatterns.Clear();
            foreach (var bind in m_BindTypes)
            {
                m_ReferencePatterns.Add(bind.Key);
            }

            m_UIScript = serializedObject.FindProperty("m_UIScript");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_UIScript);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            base.OnInspectorGUI();

            EditorGUILayout.HelpBox("0.点击[规则添加]自动绑定组件\n1. 通过右键菜单Create/MGF/Scripts/UI Script创建UI简本\n2.拖拽UI脚本到UIScript\n3.点击[生成代码]", MessageType.Info);
        }

        protected override string OnPatternStringGUI()
        {
            var pattern = string.Join(";", m_ReferencePatterns);

            var tmpEnable = GUI.enabled;
            GUI.enabled = false;
            EditorGUILayout.TextField(GUIContent.none, pattern);
            GUI.enabled = tmpEnable;

            return pattern;
        }

        protected override void AutoReference(string _)
        {
            PostIteratorTree(m_ReferencePatterns, m_ReferenceBinder.gameObject.transform, (pattern, child) =>
            {
                if (m_BindTypes.TryGetValue(pattern, out var type))
                {
                    if (type == typeof(GameObject))
                    {
                        if (child.gameObject.TryGetComponent<UIBinder>(out var binder))
                            m_ReferenceBinder.Add(child.name, binder);
                        else
                            m_ReferenceBinder.Add(child.name, child.gameObject);
                    }
                    else
                    {
                        var component = child.gameObject.GetComponent(type);
                        if (component == null)
                        {
                            m_ReferenceBinder.Add(child.name, child.gameObject);
                        }
                        else
                        {
                            m_ReferenceBinder.Add(child.name, component);
                        }
                    }
                }
            });
        }


        protected override void CodeGen()
        {
            if (m_UIScript == null || m_UIScript.objectReferenceValue == null)
            {
                Debug.LogError("MonoScript is null, please assign UIScript!");
                return;
            }

            var Datas = m_ReferenceBinder.Datas;
            var sb = new StringBuilder(1024);

            sb.Append("\n");

            foreach (var data in Datas)
            {
                sb.AppendFormat("\t\tpublic {0} {1} => Binder.GetRef<{0}>(\"{1}\");\n", data.obj.GetType(), data.key);
            }

            sb.AppendLine();

            var code = sb.ToString();
            sb.Clear();

            var monoScript = m_UIScript.objectReferenceValue as MonoScript;
            var classType = monoScript.GetClass();

            if (!string.IsNullOrEmpty(classType.Namespace))
            {
                sb.AppendLine($@"
namespace {classType.Namespace}
{{
    partial class {classType.Name}
    {{
        {code}
    }}
}}"
                );
            }
            else
            {
                sb.AppendLine($@"
    partial class {classType.Name}
    {{
        {code}
    }}"
                );
            }

            var scriptPath = AssetDatabase.GetAssetPath(m_UIScript.objectReferenceValue);
            var newpath = $"{Path.GetDirectoryName(scriptPath)}/{classType.Name}.binding.g.cs";

            if (EditorUtility.DisplayDialog("生成绑定代码", $"生成\n{newpath}", "生成", "取消"))
            {
                var content = sb.ToString();

                File.WriteAllText(newpath, content);

                Log.ERROR($"uibinder generate cs: {newpath}");
            }
        }
    }
}

#endif