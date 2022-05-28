using System;
using System.Collections.Generic;

namespace Saro.Collections
{
    /// <summary>
    /// Least Recently Used Cache
    /// <code>thread unsafe</code>
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public sealed class TLRUCache<TKey, TValue>
    {
        public class LRUNode
        {
            public TKey key;
            public TValue value;

            public LRUNode(TKey key, TValue value)
            {
                this.key = key;
                this.value = value;
            }
        }

        public event Action<TValue> OnValueRemoved;

        private readonly Dictionary<TKey, LinkedListNode<LRUNode>> m_CacheLut;
        private readonly LinkedList<LRUNode> m_Values;

        public int Capacity => m_Capacity;

        public LinkedList<LRUNode> Values => m_Values;

        private readonly int m_Capacity;

        public TLRUCache() : this(8)
        { }

        public TLRUCache(int capacity) : this(capacity, null)
        { }

        public TLRUCache(int capacity, Action<TValue> onValueRemoved)
        {
            m_Capacity = capacity;
            OnValueRemoved = onValueRemoved;

            m_CacheLut = new Dictionary<TKey, LinkedListNode<LRUNode>>(m_Capacity);
            m_Values = new LinkedList<LRUNode>();
        }

        public TValue Get(TKey key)
        {
            if (m_CacheLut.TryGetValue(key, out LinkedListNode<LRUNode> node))
            {
                m_Values.Remove(node);
                m_Values.AddFirst(node);
                return node.Value.value;
            }

            return default;
        }

        public void Put(TKey key, TValue value)
        {
            if (m_CacheLut.TryGetValue(key, out LinkedListNode<LRUNode> node))
            {
                m_Values.Remove(node);
                m_Values.AddFirst(node);
                node.Value.value = value;
                return;
            }

            if (m_CacheLut.Count >= m_Capacity)
            {
                var last = m_Values.Last;
                m_CacheLut.Remove(last.Value.key);
                m_Values.RemoveLast();

                OnValueRemoved?.Invoke(last.Value.value);

                last.Value.key = key;
                last.Value.value = value;

                m_CacheLut.Add(key, last);
                m_Values.AddFirst(last);
            }
            else
            {
                var newNode = new LinkedListNode<LRUNode>(new LRUNode(key, value));
                m_CacheLut.Add(key, newNode);
                m_Values.AddFirst(newNode);
            }
        }

        public void Clear(bool raiseOnValueRemoved = false)
        {
            if (raiseOnValueRemoved)
            {
                if (OnValueRemoved != null)
                {
                    foreach (var item in m_Values)
                    {
                        OnValueRemoved(item.value);
                    }
                }
            }

            m_CacheLut.Clear();
            m_Values.Clear();
        }
    }
}
