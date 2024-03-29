﻿namespace Saro.Pool
{
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// 需注意，[编辑器下] 被注册的对象池无法被gc
    /// </summary>
    public static class ObjectPoolChecker
    {
#if UNITY_EDITOR
        public static List<IObjectPool> s_ObjectPools = new();
#endif

        [Conditional("UNITY_EDITOR")]
        public static void Register(IObjectPool pool)
        {
#if UNITY_EDITOR
            s_ObjectPools.Add(pool);
#endif
        }
    }
}
