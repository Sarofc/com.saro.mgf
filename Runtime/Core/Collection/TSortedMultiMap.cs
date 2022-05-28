using System.Collections.Generic;

namespace Saro.Collections
{
    public sealed class TSortedMultiMap<TKey, TValue> : SortedDictionary<TKey, List<TValue>>
    {
        private readonly List<TValue> m_Empty = new List<TValue>();

        public void Add(TKey t, TValue k)
        {
            TryGetValue(t, out List<TValue> list);
            if (list == null)
            {
                list = new List<TValue>();
                Add(t, list);
            }
            list.Add(k);
        }

        public bool Remove(TKey t, TValue k)
        {
            TryGetValue(t, out List<TValue> list);
            if (list == null)
            {
                return false;
            }
            if (!list.Remove(k))
            {
                return false;
            }
            if (list.Count == 0)
            {
                Remove(t);
            }
            return true;
        }

        /// <summary>
        /// 不返回内部的list,copy一份出来
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public TValue[] GetAll(TKey t)
        {
            TryGetValue(t, out List<TValue> list);
            if (list == null)
            {
                return new TValue[0];
            }
            return list.ToArray();
        }

        /// <summary>
        /// 返回内部的list
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public new List<TValue> this[TKey t]
        {
            get
            {
                TryGetValue(t, out List<TValue> list);
                return list ?? m_Empty;
            }
        }

        public TValue GetOne(TKey t)
        {
            TryGetValue(t, out List<TValue> list);
            if (list != null && list.Count > 0)
            {
                return list[0];
            }
            return default;
        }

        public bool Contains(TKey t, TValue k)
        {
            TryGetValue(t, out List<TValue> list);
            if (list == null)
            {
                return false;
            }
            return list.Contains(k);
        }
    }
}
