using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Saro.SEditor
{
    public abstract class TabWindowContainer : EditorWindow
    {
        private List<TabWindow> m_TabWindows;
        private int m_Mode;
        private string[] m_ToolbarLabels;

        private void OnEnable()
        {
            m_TabWindows = new List<TabWindow>();
            AddTabs();

            m_ToolbarLabels = new string[m_TabWindows.Count];

            for (int i = 0; i < m_TabWindows.Count; i++)
            {
                TabWindow item = m_TabWindows[i];

                item.OnEnable();

                m_ToolbarLabels[i] = item.TabName;
            }
        }

        protected virtual void AddTabs() { }

        protected void AddTab(TabWindow tab)
        {
            m_TabWindows.Add(tab);
        }

        private void OnDisable()
        {
            for (int i = 0; i < m_TabWindows.Count; i++)
            {
                TabWindow item = m_TabWindows[i];

                item.OnDisable();
            }
        }

        private void OnGUI()
        {
            m_Mode = GUILayout.Toolbar(m_Mode, m_ToolbarLabels, "LargeButton");

            m_TabWindows[m_Mode].OnGUI(GetSubWindowArea());
        }


        private Rect GetSubWindowArea()
        {
            float padding = 32;
            Rect subPos = new Rect(0, padding, position.width, position.height - padding);
            return subPos;
        }
    }
}