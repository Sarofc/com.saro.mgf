using Saro.SEditor;
using UnityEditor;
using UnityEngine;

namespace Saro.Pool
{
    internal class PoolDebugWindow : TabWindowContainer
    {
        [MenuItem("MGF Tools/Debug/Pool Debugger")]
        private static void ShowWindow()
        {
            var window = GetWindow<PoolDebugWindow>();
            window.titleContent = new GUIContent("Pool Debugger");
            window.Show();
        }

        protected override void AddTabs()
        {
            AddTab(new SharedPoolDebugTab());
            AddTab(new ObjectPoolDebugTab());
        }
    }
}
