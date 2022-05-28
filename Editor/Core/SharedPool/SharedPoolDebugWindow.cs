
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Saro.MoonAsset
{
    public class SharedPoolDebugWindow : EditorWindow
    {
        private TreeViewState m_SharedPoolTreeState;
        private MultiColumnHeaderState m_SharedPoolTreeMCHState;
        private SharedPoolTree m_SharedPoolTree;

        [MenuItem("MGF Tools/Debug/SharedPool Debugger")]
        private static void ShowWindow()
        {
            var window = GetWindow<SharedPoolDebugWindow>();
            window.titleContent = new GUIContent("SharedPoolDebugger");
            window.Show();
        }

        private void OnEnable()
        {
            if (m_SharedPoolTreeState == null)
                m_SharedPoolTreeState = new TreeViewState();

            var headerState = SharedPoolTree.CreateDefaultMultiColumnHeaderState();// multiColumnTreeViewRect.width);
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_SharedPoolTreeMCHState, headerState))
                MultiColumnHeaderState.OverwriteSerializedFields(m_SharedPoolTreeMCHState, headerState);
            m_SharedPoolTreeMCHState = headerState;

            m_SharedPoolTree = new SharedPoolTree(m_SharedPoolTreeState, m_SharedPoolTreeMCHState);
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("运行时查看", MessageType.Warning);
                return;
            }

            m_SharedPoolTree.Reload();
            m_SharedPoolTree.OnGUI(new Rect(0, 0, position.width, position.height));
        }
    }
}