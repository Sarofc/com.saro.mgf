using System;

namespace Saro.SEditor
{
    ///Derive this to create custom attributes to be drawn with an ObjectAttributeDrawer<T>.
    [AttributeUsage(AttributeTargets.Field)]
    abstract public class CustomDrawerAttribute : Attribute { }
}
