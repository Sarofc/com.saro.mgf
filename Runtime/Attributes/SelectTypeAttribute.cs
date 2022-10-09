// ---------------------------------------------------------------------------- 
// Author: Ryan Hipple
// Date:   05/01/2018
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using UnityEngine;

namespace Saro.SEditor
{
    /// <summary>
    /// Put this attribute on a public (or SerialzeField) enum in a
    /// MonoBehaviour or ScriptableObject to get an improved enum selector
    /// popup. The enum list is scrollable and can be filtered by typing.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SelectTypeAttribute : PropertyAttribute
    {
        public Type SupperType { get; private set; }
        public bool FullName { get; set; } = true;

        public SelectTypeAttribute(Type supperType)
        {
            this.SupperType = supperType;
        }
    }
}
