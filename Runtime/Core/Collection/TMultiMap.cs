using System.Collections;
using System.Collections.Generic;

namespace Saro.Collections
{
    /// <summary>
    /// 多值字典。
    /// 一个key可以对应多个元素，且元素可重复
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public sealed class TMultiMap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TLinkedListRange<TValue>>>, IEnumerable
    {
        private readonly TLinkedList<TValue> m_LinkedList;
        private readonly Dictionary<TKey, TLinkedListRange<TValue>> m_Map;

        public TMultiMap() : this(EqualityComparer<TKey>.Default)
        { }

        public TMultiMap(IEqualityComparer<TKey> comparer)
        {
            m_LinkedList = new TLinkedList<TValue>();
            m_Map = new Dictionary<TKey, TLinkedListRange<TValue>>(comparer);
        }

        public int Count => m_Map.Count;

        /// <summary>
        /// 获取链表区间
        /// </summary>
        public TLinkedListRange<TValue> this[TKey key]
        {
            get
            {
                m_Map.TryGetValue(key, out TLinkedListRange<TValue> range);
                return range;
            }
        }

        /// <summary>
        /// 清除集合
        /// </summary>
        public void Clear()
        {
            m_Map.Clear();
            m_LinkedList.Clear();
        }

        /// <summary>
        /// 是否包含此区间
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(TKey key)
        {
            return m_Map.ContainsKey(key);
        }

        /// <summary>
        /// 是否包含此元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(TKey key, TValue value)
        {
            if (m_Map.TryGetValue(key, out TLinkedListRange<TValue> range))
            {
                return range.Contains(value);
            }

            return false;
        }

        /// <summary>
        /// 尝试获取链表区间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TLinkedListRange<TValue> range)
        {
            return m_Map.TryGetValue(key, out range);
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TKey key, TValue value)
        {
            if (m_Map.TryGetValue(key, out TLinkedListRange<TValue> range))
            {
                m_LinkedList.AddBefore(range.Tail, value);
            }
            else
            {
                LinkedListNode<TValue> first = m_LinkedList.AddLast(value);
                LinkedListNode<TValue> last = m_LinkedList.AddLast(default(TValue));
                m_Map.Add(key, new TLinkedListRange<TValue>(first, last));
            }
        }

        /// <summary>
        /// 添加元素,相同TValue只会被添加一次
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddUniqe(TKey key, TValue value)
        {
            if (m_Map.TryGetValue(key, out TLinkedListRange<TValue> range))
            {
                if (!m_LinkedList.Contains(value))
                {
                    m_LinkedList.AddBefore(range.Tail, value);
                }
            }
            else
            {
                LinkedListNode<TValue> first = m_LinkedList.AddLast(value);
                LinkedListNode<TValue> last = m_LinkedList.AddLast(default(TValue));
                m_Map.Add(key, new TLinkedListRange<TValue>(first, last));
            }
        }

        /// <summary>
        /// 根据key value移除元素，只会移除第一个相同的元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Remove(TKey key, TValue value)
        {
            if (m_Map.TryGetValue(key, out TLinkedListRange<TValue> range))
            {
                for (LinkedListNode<TValue> current = range.Head; current != null && current != range.Tail; current = current.Next)
                {
                    if (current.Value.Equals(value))
                    {
                        if (current == range.Head)
                        {
                            LinkedListNode<TValue> next = current.Next;
                            if (next == range.Tail)
                            {
                                m_LinkedList.Remove(next);
                                m_Map.Remove(key);
                            }
                            else
                            {
                                m_Map[key] = new TLinkedListRange<TValue>(next, range.Tail);
                            }
                        }
                        m_LinkedList.Remove(current);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 移除key对应的链表区间
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool RemoveAll(TKey key)
        {
            if (m_Map.TryGetValue(key, out TLinkedListRange<TValue> range))
            {
                m_Map.Remove(key);

                LinkedListNode<TValue> current = range.Head;
                while (current != null)
                {
                    LinkedListNode<TValue> next = current != range.Tail ? current.Next : null;
                    m_LinkedList.Remove(current);
                    current = next;
                }

                return true;
            }

            return false;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(m_Map);
        }

        IEnumerator<KeyValuePair<TKey, TLinkedListRange<TValue>>> IEnumerable<KeyValuePair<TKey, TLinkedListRange<TValue>>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TLinkedListRange<TValue>>>, IEnumerator
        {
            object IEnumerator.Current => m_Enumerator.Current;

            public KeyValuePair<TKey, TLinkedListRange<TValue>> Current => m_Enumerator.Current;

            private Dictionary<TKey, TLinkedListRange<TValue>>.Enumerator m_Enumerator;

            internal Enumerator(Dictionary<TKey, TLinkedListRange<TValue>> dic)
            {
                if (dic == null) throw new System.Exception("Dictionary is invalid.");

                m_Enumerator = dic.GetEnumerator();
            }

            public void Dispose()
            {
                m_Enumerator.Dispose();
            }

            public bool MoveNext()
            {
                return m_Enumerator.MoveNext();
            }

            void IEnumerator.Reset()
            {
                ((IEnumerator<KeyValuePair<TKey, TLinkedListRange<TValue>>>)m_Enumerator).Reset();
            }
        }
    }
}