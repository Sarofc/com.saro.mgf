using System;

namespace Saro.UI
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class UIWindowAttribute : Attribute
    {
        public int Index { get; private set; }
        public Type Type { get; set; }
        public string AssetPath { get; private set; }

        public UIWindowAttribute(int uiIndex, string assetPath)
        {
            Index = uiIndex;
            AssetPath = assetPath;
        }
    }
}
