using System;
using System.Text;

namespace Saro.Utility
{
    /// <summary>
    /// StringBuilder缓存，来自.net
    /// <code>thread safe</code>
    /// </summary>
    public static class StringBuilderCache
    {
        // The value 360 was chosen in discussion with performance experts as a compromise between using
        // as litle memory (per thread) as possible and still covering a large part of short-lived
        // StringBuilder creations on the startup path of VS designers.
        private const int k_MAX_BUILDER_SIZE = 360;

        [ThreadStatic]
        private static StringBuilder s_CachedInstance;

        public static StringBuilder Get(int capacity = 16)
        {
            if (capacity <= k_MAX_BUILDER_SIZE)
            {
                StringBuilder sb = StringBuilderCache.s_CachedInstance;
                if (sb != null)
                {
                    // Avoid stringbuilder block fragmentation by getting a new StringBuilder
                    // when the requested size is larger than the current capacity
                    if (capacity <= sb.Capacity)
                    {
                        StringBuilderCache.s_CachedInstance = null;
                        sb.Clear();
                        return sb;
                    }
                }
            }
            return new StringBuilder(capacity);
        }

        public static void Release(StringBuilder sb)
        {
            if (sb.Capacity <= k_MAX_BUILDER_SIZE)
            {
                StringBuilderCache.s_CachedInstance = sb;
            }
        }

        public static string GetStringAndRelease(StringBuilder sb)
        {
            string result = sb.ToString();
            Release(sb);
            return result;
        }
    }
}