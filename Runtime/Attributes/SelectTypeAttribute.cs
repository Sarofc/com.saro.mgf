using System;
using System.Diagnostics;
using UnityEngine;

namespace Saro.SEditor
{
    /// <summary>
    /// Type类型下拉框，在string字段上使用
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
