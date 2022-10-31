using System;
using System.Diagnostics;

namespace Saro.SEditor
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class SeparatorAttribute : Attribute
    {
    }
}