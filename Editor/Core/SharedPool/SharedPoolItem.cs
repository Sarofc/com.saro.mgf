using UnityEditor.IMGUI.Controls;

namespace Saro.MoonAsset
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
