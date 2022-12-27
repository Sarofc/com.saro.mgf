﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Saro.Utility
{
    /*
     * TODO
     * 
     * 优化下性能
     * 
     */
    public static class TypeUtility
    {
        public static IReadOnlyDictionary<string, Assembly> AssemblyMap => s_AssemblyMap;
        private static readonly Dictionary<string, Assembly> s_AssemblyMap = new Dictionary<string, Assembly>(StringComparer.Ordinal);

        static TypeUtility()
        {
            CacheAssemblies();
        }

        public static void CacheAssemblies()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                var assemblyName = assemblies[i].GetName().Name;
                if (!s_AssemblyMap.ContainsKey(assemblyName))
                {
                    s_AssemblyMap.Add(assemblyName, assemblies[i]);
                }
            }
        }

        public static void ClearCacheAssemblies()
        {
            s_AssemblyMap.Clear();
        }

        public static Type GetType(string assemblyName, string typeName)
        {
            if (!s_AssemblyMap.TryGetValue(assemblyName, out var assembly))
            {
                assembly = Assembly.Load(assemblyName);
                s_AssemblyMap.Add(assemblyName, assembly);
            }

            return assembly.GetType(typeName);
        }

        public static string GetTypeInfo(Type type)
        {
            return $"{type.FullName}, {type.Assembly.GetName().Name}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeInfo"><see cref="GetTypeInfo"/></param>
        /// <returns></returns>
        public static Type GetTypeByTypeInfo(string typeInfo, bool throwOnError = true)
        {
            try
            {
                var array = typeInfo.Split(", ", StringSplitOptions.RemoveEmptyEntries);
                return TypeUtility.GetType(array[1], array[0]);
            }
            catch (Exception e)
            {
                if (throwOnError)
                    throw e;
            }
            return null;
        }

        public static List<Type> GetSubClassTypesAllAssemblies(Type supperClassType, bool includedAbstract = false)
        {
            var ret = new List<Type>();
            foreach (var item in s_AssemblyMap)
            {
                var types = GetSubClassTypes(item.Key, supperClassType, includedAbstract);
                if (types.Count > 0)
                    ret.AddRange(types);
            }

            return ret;
        }

        public static List<string> GetSubClassTypeNames(string assemblyName, Type supperClassType, bool includedAbstract = false)
        {
            if (!s_AssemblyMap.TryGetValue(assemblyName, out var assembly))
            {
                assembly = Assembly.Load(assemblyName);
                s_AssemblyMap.Add(assemblyName, assembly);
            }

            var res = new List<string>();

            if (assembly != null)
            {
                var types = assembly.GetTypes();

                // UnityEngine.Debug.Log(types.Length);

                for (int i = 0; i < types.Length; i++)
                {
                    //if (types[i].IsClass)
                    {
                        if (supperClassType.IsAssignableFrom(types[i]))
                        //if (types[i].IsSubclassOf(supperClassType))
                        {
                            if (includedAbstract)
                            {
                                res.Add(types[i].FullName);
                            }
                            else
                            {
                                if (!types[i].IsAbstract)
                                {
                                    res.Add(types[i].FullName);
                                }
                            }
                        }
                    }
                }
            }

            return res;
        }

        public static List<Type> GetSubClassTypes(string assemblyName, Type supperClassType, bool includedAbstract = false)
        {
            if (!s_AssemblyMap.TryGetValue(assemblyName, out var assembly))
            {
                assembly = Assembly.Load(assemblyName);
                s_AssemblyMap.Add(assemblyName, assembly);
            }

            List<Type> res = new();

            if (assembly != null)
            {
                Type[] types = assembly.GetTypes();

                // UnityEngine.Debug.Log(types.Length);

                for (int i = 0; i < types.Length; i++)
                {
                    //if (types[i].IsClass)
                    {
                        if (supperClassType.IsAssignableFrom(types[i]))
                        //if (types[i].IsSubclassOf(supperClassType))
                        {
                            if (includedAbstract)
                            {
                                res.Add(types[i]);
                            }
                            else
                            {
                                if (!types[i].IsAbstract)
                                {
                                    res.Add(types[i]);
                                }
                            }
                        }
                    }
                }
            }

            return res;
        }

        /// This is a way to get a field name string in such a manner that the compiler will
        /// generate errors for invalid fields.  Much better than directly using strings.
        /// Usage: instead of
        /// <example>
        /// "m_MyField";
        /// </example>
        /// do this:
        /// <example>
        /// MyClass myclass = null;
        /// SerializedPropertyUtility.PropertyName( () => myClass.m_MyField);
        /// </example>
        public static string PropertyName<TValue>(Expression<Func<TValue>> expr)
        {
            var body = expr.Body as MemberExpression;
            if (body == null)
            {
                var ubody = (UnaryExpression)expr.Body;
                body = ubody.Operand as MemberExpression;
            }

            return body.Member.Name;
        }

        public static string PropertyName<TType, TValue>(Expression<Func<TType, TValue>> expr)
        {
            MemberExpression me;
            switch (expr.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    me = expr.Body as MemberExpression;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            var members = new List<string>();
            while (me != null)
            {
                members.Add(me.Member.Name);
                me = me.Expression as MemberExpression;
            }

            var sb = StringBuilderCache.Get();
            for (int i = members.Count - 1; i >= 0; i--)
            {
                sb.Append(members[i]);
                if (i > 0) sb.Append('.');
            }

            return StringBuilderCache.GetStringAndRelease(sb);
        }
    }
}