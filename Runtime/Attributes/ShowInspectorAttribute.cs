using System;
using System.Diagnostics;

namespace Saro.SEditor
{
    /// <summary>
    /// only for <see cref="SEditorUtility"/>
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    public sealed class ShowInInspectorAttribute : Attribute
    {
    }
}
