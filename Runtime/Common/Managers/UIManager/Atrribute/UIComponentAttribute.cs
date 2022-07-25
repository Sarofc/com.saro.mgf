using System;

namespace Saro.UI
{
    /// <summary>
    /// 组件属性
    /// </summary>
    public class UIComponentAttribute : Attribute
    {
        /// <summary>
        /// 路径
        /// </summary>
        public string Path { get; private set; }

        public UIComponentAttribute(string path)
        {
            this.Path = path;
        }
    }
}