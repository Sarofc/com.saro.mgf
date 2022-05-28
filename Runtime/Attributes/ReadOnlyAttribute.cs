

using System;
using UnityEngine;

namespace Saro.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class ReadOnlyAttribute : PropertyAttribute { }
}