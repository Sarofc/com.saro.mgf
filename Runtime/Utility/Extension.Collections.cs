using System.Collections.Generic;

namespace Saro.Utility
{
    public static partial class Extension
    {
        public static void ForEach<T>(this IEnumerable<T> elements, System.Action<T> action)
        {
            foreach (T item in elements)
            {
                action(item);
            }
        }

        public static void Fill<T>(this T[] elements, T element)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                elements[i] = element;
            }
        }

        public static bool AddUnique<T>(this List<T> list, T element)
        {
            if (!list.Contains(element))
            {
                list.Add(element);
                return true;
            }

            //                UnityEngine.Debug.LogError($"{element} has already exsits.");

            return false;
        }

        #region Dictionary

        /// <summary>
        /// key存在，则返回value，否则，添加key，并返回默认值
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="map"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> map, TKey key)
        {
            if (!map.TryGetValue(key, out TValue value))
            {
                map.Add(key, value);
            }

            return value;
        }

        /// <summary>
        /// 将 src 合并到 dst。跳过 src 里重复元素，即保留 dst 里的元素
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dst"></param>
        /// <param name="src"></param>
        public static void Merge<TKey, TValue>(this Dictionary<TKey, TValue> dst, IDictionary<TKey, TValue> src)
        {
            foreach (var kv in src)
            {
                if (dst.ContainsKey(kv.Key)) continue;

                dst.Add(kv.Key, kv.Value);
            }
        }

        #endregion
    }
}