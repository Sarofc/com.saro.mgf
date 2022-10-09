

using System;
using System.Diagnostics;
using UnityEngine;

namespace Saro.SEditor
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class ReadOnlyAttribute : PropertyAttribute { }
}