using System;
using System.Collections;
using System.Collections.Generic;

namespace Saro.Collections
{
    /*
     * 对LinkedList的一层封装
     *
     * 缓存节点，减少GC
     */
    public sealed class TLinkedList<T> : ICollection<T>, IEnumerable<T>, ICollection, IEnumerable
    {
        private readonly LinkedList<T> m_LinkedList;
        private readonly Queue<LinkedListNode<T>> m_CachedNodes;

        public TLinkedList()
        {
            m_LinkedList = new LinkedList<T>();
            m_CachedNodes = new Queue<LinkedListNode<T>>();
        }

        public int Count => m_LinkedList.Count;

        public int CachedNodeCount => m_CachedNodes.Count;

        public LinkedListNode<T> First => m_LinkedList.First;

        public LinkedListNode<T> Last => m_LinkedList.Last;

        public bool IsReadOnly => ((ICollection<T>)m_LinkedList).IsReadOnly;

        public bool IsSynchronized => ((ICollection)m_LinkedList).IsSynchronized;

        public object SyncRoot => ((ICollection)m_LinkedList).SyncRoot;

        public void Add(T item)
        {
            AddLast(item);
        }

        public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T item)
        {
            LinkedListNode<T> newNode = AcquireNode(item);
            m_LinkedList.AddAfter(node, newNode);
            return newNode;
        }

        public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode)
        {
            m_LinkedList.AddAfter(node, newNode);
        }

        public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T item)
        {
            LinkedListNode<T> newNode = AcquireNode(item);
            m_LinkedList.AddBefore(node, newNode);
            return newNode;
        }

        public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)
        {
            m_LinkedList.AddBefore(node, newNode);
        }

        public LinkedListNode<T> AddFirst(T item)
        {
            LinkedListNode<T> newNode = AcquireNode(item);
            m_LinkedList.AddFirst(newNode);
            return newNode;
        }

        public void AddFirst(LinkedListNode<T> node)
        {
            m_LinkedList.AddFirst(node);
        }

        public LinkedListNode<T> AddLast(T item)
        {
            LinkedListNode<T> newNode = AcquireNode(item);
            m_LinkedList.AddLast(newNode);
            return newNode;
        }

        public void AddLast(LinkedListNode<T> node)
        {
            m_LinkedList.AddLast(node);
        }

        public void Clear()
        {
            LinkedListNode<T> current = m_LinkedList.First;
            while (current != null)
            {
                ReleaseNode(current);
                current = current.Next;
            }
            m_LinkedList.Clear();
        }

        public void ClearCachedNodes()
        {
            m_CachedNodes.Clear();
        }

        public bool Contains(T item)
        {
            return m_LinkedList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            m_LinkedList.CopyTo(array, arrayIndex);
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)m_LinkedList).CopyTo(array, index);
        }


        public LinkedListNode<T> Find(T item)
        {
            return m_LinkedList.Find(item);
        }

        public LinkedListNode<T> FindLast(T item)
        {
            return m_LinkedList.FindLast(item);
        }

        public bool Remove(T item)
        {
            LinkedListNode<T> node = m_LinkedList.Find(item);
            if (node != null)
            {
                m_LinkedList.Remove(node);
                ReleaseNode(node);
                return true;
            }
            return false;
        }

        public void Remove(LinkedListNode<T> node)
        {
            m_LinkedList.Remove(node);
            ReleaseNode(node);
        }

        public void RemoveFirst()
        {
            LinkedListNode<T> first = m_LinkedList.First;
            if (first == null)
            {
                throw new NullReferenceException("First node is null");
            }

            m_LinkedList.RemoveFirst();
            ReleaseNode(first);
        }

        public void RemoveLast()
        {
            LinkedListNode<T> last = m_LinkedList.Last;
            if (last == null)
            {
                throw new NullReferenceException("Last node is null");
            }

            m_LinkedList.RemoveLast();
            ReleaseNode(last);
        }

        private void ReleaseNode(LinkedListNode<T> node)
        {
            node.Value = default(T);
            m_CachedNodes.Enqueue(node);
        }

        private LinkedListNode<T> AcquireNode(T item)
        {
            LinkedListNode<T> node = null;
            if (m_CachedNodes.Count > 0)
            {
                node = m_CachedNodes.Dequeue();
                node.Value = item;
            }
            else
            {
                node = new LinkedListNode<T>(item);
            }

            return node;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(m_LinkedList);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            public T Current => m_Enumerator.Current;

            object IEnumerator.Current => m_Enumerator.Current;

            private LinkedList<T>.Enumerator m_Enumerator;

            internal Enumerator(LinkedList<T> linkedList)
            {
                if (linkedList == null) throw new Exception("LinkedList is invalid.");
                m_Enumerator = linkedList.GetEnumerator();
            }

            void IDisposable.Dispose()
            {
                m_Enumerator.Dispose();
            }

            public bool MoveNext()
            {
                return m_Enumerator.MoveNext();
            }

            void IEnumerator.Reset()
            {
                ((IEnumerator<T>)m_Enumerator).Reset();
            }
        }
    }
}