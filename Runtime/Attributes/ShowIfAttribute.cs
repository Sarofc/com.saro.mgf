using System;
using System.Diagnostics;

namespace Saro.SEditor
{
    [Conditional("UNITY_EDITOR")]
    public sealed class ShowIfAttribute : Attribute
    {
        public readonly string requiredPropertyName;
        public readonly object checkObject;

        public ShowIfAttribute(string requiredPropertyName, object checkObject)
        {
            this.requiredPropertyName = requiredPropertyName;
            this.checkObject = checkObject;
        }
    }
}
