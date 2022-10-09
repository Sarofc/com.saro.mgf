
using Saro.Core;
using Saro.SEditor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Saro.Pool
{
    public class ObjectPoolDebugTab : TabWindow
    {
        private TreeViewState m_TreeState;
        private MultiColumnHeaderState m_TreeMCHState;
        private ObjectPoolTree m_Tree;
        private SearchField m_SearchField;
        private string m_SearchText;

        public override string TabName => nameof(IObjectPool);

        public override void OnEnable()
        {
            if (m_TreeState == null)
                m_TreeState = new TreeViewState();

            var headerState = ObjectPoolTree.CreateDefaultMultiColumnHeaderState();// multiColumnTreeViewRect.width);
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_TreeMCHState, headerState))
                MultiColumnHeaderState.OverwriteSerializedFields(m_TreeMCHState, headerState);
            m_TreeMCHState = headerState;

            m_Tree = new ObjectPoolTree(m_TreeState, m_TreeMCHState);

            m_SearchField = new SearchField();
        }

        public override void OnGUI(Rect rect)
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("运行时查看", MessageType.Warning);
                return;
            }

            var searchRect = EditorGUILayout.GetControlRect();
            var treeRect = rect;
            treeRect.y += searchRect.height;
            treeRect.height -= searchRect.height;

            var searchText = m_SearchField.OnToolbarGUI(searchRect, m_SearchText);
            if (searchText != m_SearchText)
            {
                m_Tree.searchString = m_SearchText = searchText;
            }
            m_Tree.Reload();
            m_Tree.OnGUI(treeRect);
        }
    }
}