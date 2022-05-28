#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Saro.Core
{
    [CustomEditor(typeof(ProcedureSettings))]
    public class ProcedureSettingsInspector : Editor
    {
        private SerializedProperty m_Start;
        private SerializedProperty m_SelectedIndex;

        private List<string> m_ProcedureList
        {
            get
            {
                if (m_ProcedureSettings == null) m_ProcedureSettings = target as ProcedureSettings;
                return m_ProcedureSettings.procedureList;
            }
        }
        private List<string> m_AllProcedures;

        private ProcedureSettings m_ProcedureSettings;
        private bool m_Debug;
        private Vector2 m_ScrollPos;
        private Vector2 m_ScrollPos1;

        private void OnEnable()
        {
            m_ProcedureSettings = target as ProcedureSettings;

            m_Start = serializedObject.FindProperty("start");
            m_SelectedIndex = serializedObject.FindProperty("selectedIndex");

            SearchProcedure();

            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", target, typeof(MonoScript), false);
            GUI.enabled = true;

            EditorGUILayout.HelpBox("1.�̳�AProcedureBase��.\n2.��ѡ��Ҫʵ����������.\n3.ѡ��ʼ����.", MessageType.Info);

            serializedObject.Update();

            // check valid
            if (string.IsNullOrEmpty(m_Start.stringValue) || Utility.RefelctionUtility.GetType(ProcedureComponent.k_Assembly, m_Start.stringValue) == null)
            {
                EditorGUILayout.HelpBox("Start Procedure is invalid!", MessageType.Error);
            }
            else if (EditorApplication.isPlaying)
            {
                var procedureMgr = ProcedureComponent.Get();
                if (procedureMgr != null)
                {
                    EditorGUILayout.LabelField("Current Procedure: ",
                        procedureMgr.CurrentProcedure == null ?
                        "None" :
                        procedureMgr.CurrentProcedure.ToString()
                    );
                    Repaint();
                }
                else
                {
                    EditorGUILayout.HelpBox("Null ProcedureMgr service!", MessageType.Error);
                }
            }

            if (m_ProcedureList != null && m_ProcedureList.Count > 0)
            {
                if (m_ProcedureList.Count > m_AllProcedures.Count)
                {
                    EditorGUILayout.HelpBox("procedure count mismatching", MessageType.Error);
                }

                for (int i = 0; i < m_ProcedureList.Count; i++)
                {
                    string typeName = m_ProcedureList[i];
                    if (string.IsNullOrEmpty(typeName))
                    {
                        EditorGUILayout.HelpBox($"null or empty string: [{typeName}]", MessageType.Error);
                    }

                    Type type = Utility.RefelctionUtility.GetType(ProcedureComponent.k_Assembly, typeName);
                    if (type == null)
                    {
                        EditorGUILayout.HelpBox($"null type: [{typeName}]", MessageType.Error);
                    }
                    else if (type.IsAbstract)
                    {
                        EditorGUILayout.HelpBox($"abstract class: [{typeName}]", MessageType.Error);
                    }
                }
            }

            var isPlaying = EditorApplication.isPlayingOrWillChangePlaymode;
            // edit
            EditorGUI.BeginDisabledGroup(isPlaying);
            {
                // update
                if (m_AllProcedures != null && m_AllProcedures.Count > 0)
                {
                    EditorGUILayout.LabelField("Select Procedure: ", EditorStyles.boldLabel);
                    m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos, GUILayout.ExpandHeight(false));
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < m_AllProcedures.Count; i++)
                    {
                        string name = m_AllProcedures[i];
                        bool selected = m_ProcedureList.Contains(name);

                        bool isStart = string.Compare(name, m_Start.stringValue) == 0;
                        if (!isPlaying && isStart) GUI.enabled = false;

                        if (selected != EditorGUILayout.ToggleLeft(name, selected))
                        {
                            if (!selected)
                            {
                                m_ProcedureList.Add(name);
                            }
                            else //if (name != m_start.stringValue)
                            {
                                m_ProcedureList.Remove(name);
                            }
                        }

                        if (!isPlaying && isStart) GUI.enabled = true;
                    }
                    ApplyTo();
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndScrollView();
                }
                else
                {
                    EditorGUILayout.HelpBox("There is no valid procedure.", MessageType.Warning);
                }

                EditorGUILayout.Separator();
                EditorGUILayout.BeginHorizontal();

                if (m_ProcedureList != null && m_ProcedureList.Count > 0)
                {
                    int selectedIndex = EditorGUILayout.Popup("Start Procedure: ", m_SelectedIndex.intValue, m_ProcedureList.ToArray());
                    if (selectedIndex != m_SelectedIndex.intValue)
                    {
                        m_SelectedIndex.intValue = selectedIndex;
                        m_Start.stringValue = m_ProcedureList[selectedIndex];
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Selected procedure first.", MessageType.Info);
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            EditorGUI.indentLevel++;
            m_Debug = EditorGUILayout.Foldout(m_Debug, "Debug Selected List", true);
            if (m_Debug)
            {
                m_ScrollPos1 = EditorGUILayout.BeginScrollView(m_ScrollPos1);
                EditorGUI.indentLevel++;
                for (int i = 0; i < m_ProcedureList.Count; i++)
                {
                    EditorGUILayout.LabelField(m_ProcedureList[i]);
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndScrollView();
            }
            EditorGUI.indentLevel--;

            if (GUILayout.Button("Reload"))
            {
                m_ProcedureList.Clear();
                SearchProcedure();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ApplyTo()
        {
            serializedObject.Update();

            m_ProcedureList.Sort();

            if (!string.IsNullOrEmpty(m_Start.stringValue))
            {
                m_SelectedIndex.intValue = m_ProcedureList.IndexOf(m_Start.stringValue);
                if (m_SelectedIndex.intValue < 0)
                {
                    m_Start.stringValue = null;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void SearchProcedure()
        {
            try
            {
                m_AllProcedures = Utility.RefelctionUtility.GetSubClassTypeNames(ProcedureComponent.k_Assembly, typeof(AProcedureBase));
                m_AllProcedures.Sort();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }

}
#endif