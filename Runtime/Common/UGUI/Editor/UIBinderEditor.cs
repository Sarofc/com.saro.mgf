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

            var newpath = AssetDatabase.GetAssetPath(m_UIScript.objectReferenceValue);

            if (!string.IsNullOrEmpty(newpath))
            {
                var oldCode = File.ReadAllText(newpath, Encoding.UTF8);

                var startIndex = oldCode.LastIndexOf(">>begin");
                var endIndex = oldCode.LastIndexOf("<<end");

                if (startIndex != -1 && endIndex != -1)
                {
                    startIndex += 7;
                    endIndex -= 2;

                    var len = endIndex - startIndex;
                    if (len > 0)
                    {
                        var newCode = oldCode.Remove(startIndex, len);
                        newCode = newCode.Insert(startIndex, "\t");
                        newCode = newCode.Insert(startIndex, code);

                        File.WriteAllText(newpath, newCode, Encoding.UTF8);
                        AssetDatabase.Refresh();
                    }
                    else
                    {
                        Debug.LogError("没有 写入成功，请检查 >>begin <<end 标识 是否是成对，且顺序正确，参考 Template_UIScript.txt ！");
                    }
                }
                else
                {
                    Debug.LogError("没有 写入成功，请检查 有没有 >>begin <<end 标识，参考 Template_UIScript.txt ！");
                }
            }
        }

        private void __CodeGen()
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
                sb.AppendFormat("\t\tpublic {0} {1};\n", data.obj.GetType(), data.key);
            }

            sb.AppendLine();

            sb.AppendLine("\t\tvoid GetComps()");
            sb.AppendLine("\t\t{");
            foreach (var data in Datas)
            {
                sb.AppendFormat("\t\t\t{0} = Binder.Get<{1}>(\"{0}\");\n", data.key, data.obj.GetType());
            }
            sb.AppendLine("\t\t}");

            var code = sb.ToString();

            var newpath = AssetDatabase.GetAssetPath(m_UIScript.objectReferenceValue);

            if (!string.IsNullOrEmpty(newpath))
            {
                var oldCode = File.ReadAllText(newpath);

                var startIndex = oldCode.LastIndexOf(">>begin");
                var endIndex = oldCode.LastIndexOf("<<end");

                if (startIndex != -1 && endIndex != -1)
                {
                    startIndex += 7;
                    endIndex -= 2;

                    var len = endIndex - startIndex;
                    if (len > 0)
                    {
                        var newCode = oldCode.Remove(startIndex, len);
                        newCode = newCode.Insert(startIndex, "\t");
                        newCode = newCode.Insert(startIndex, code);

                        File.WriteAllText(newpath, newCode);
                        AssetDatabase.Refresh();
                    }
                    else
                    {
                        Debug.LogError("没有 写入成功，请检查 >>begin <<end 标识 是否是成对，且顺序正确，参考 Template_UIScript.txt ！");
                    }
                }
                else
                {
                    Debug.LogError("没有 写入成功，请检查 有没有 >>begin <<end 标识，参考 Template_UIScript.txt ！");
                }
            }
        }
    }
}

#endif