using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Saro.Utility
{
    public static class UnmanagedTypeExtensions
    {
        static readonly Dictionary<Type, bool> s_CachedTypes =
            new Dictionary<Type, bool>();

        public static bool IsUnmanagedEx(this Type t)
        {
            var result = false;

            if (s_CachedTypes.ContainsKey(t))
                return s_CachedTypes[t];

            if (t.IsPrimitive || t.IsPointer || t.IsEnum)
                result = true;
            else
            if (t.IsValueType && t.IsGenericType)
            {
                var areGenericTypesAllBlittable = t.GenericTypeArguments.All(x => IsUnmanagedEx(x));
                if (areGenericTypesAllBlittable)
                    result = t.GetFields(BindingFlags.Public |
                                         BindingFlags.NonPublic | BindingFlags.Instance)
                              .All(x => IsUnmanagedEx(x.FieldType));
                else
                    return false;
            }
            else
            if (t.IsValueType)
                result = t.GetFields(BindingFlags.Public |
                                     BindingFlags.NonPublic | BindingFlags.Instance)
                          .All(x => IsUnmanagedEx(x.FieldType));

            s_CachedTypes.Add(t, result);
            return result;
        }
    }
}