using UnityEditor.IMGUI.Controls;

namespace Saro.Pool
{
    public class SharedPoolItem : TreeViewItem
    {
        public SharedPoolInfo info;

        public SharedPoolItem(SharedPoolInfo info)
        {
            this.info = info;
        }
    }
}
