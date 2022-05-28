using System;
using System.Collections;
using System.Collections.Generic;

namespace Saro.Collections
{
    public struct TLinkedListRange<T> : IEnumerable<T>, IEnumerable
    {
        public static TLinkedListRange<T> s_Empty = new TLinkedListRange<T>();

        private readonly LinkedListNode<T> m_Head;
        private readonly LinkedListNode<T> m_Tail;

        public TLinkedListRange(LinkedListNode<T> first, LinkedListNode<T> last)
        {
            if (first == null || last == null || first == last)
            {
                throw new Exception("Range is invalid");
            }
            m_Head = first;
            m_Tail = last;
        }

        public bool IsValid
        {
            get
            {
                return m_Head != null && m_Tail != null && m_Head != m_Tail;
            }
        }

        public LinkedListNode<T> Head => m_Head;
        public LinkedListNode<T> Tail => m_Tail;

        public int Count
        {
            get
            {
                if (!IsValid) return 0;
                int count = 0;
                for (LinkedListNode<T> cur = m_Head; cur != null && cur != m_Tail; cur = cur.Next)
                {
                    count++;
                }
                return count;
            }
        }

        public bool Contains(T value)
        {
            for (LinkedListNode<T> cur = m_Head; cur != null && cur != m_Tail; cur = cur.Next)
            {
                if (cur.Value.Equals(value))
                    return true;
            }
            return false;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private readonly TLinkedListRange<T> m_linkedListRange;
            private LinkedListNode<T> m_current;
            private T m_currentValue;

            public Enumerator(TLinkedListRange<T> linkedListRange)
            {
                if (!linkedListRange.IsValid)
                {
                    throw new Exception("Range is invalid");
                }
                m_linkedListRange = linkedListRange;
                m_current = m_linkedListRange.m_Head;
                m_currentValue = default(T);
            }

            public T Current => m_currentValue;

            object IEnumerator.Current => m_currentValue;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (m_current == null || m_current == m_linkedListRange.m_Tail)
                {
                    return false;
                }

                m_currentValue = m_current.Value;
                m_current = m_current.Next;
                return true;
            }

            public void Reset()
            {
                m_current = m_linkedListRange.m_Head;
                m_currentValue = default(T);
            }
        }
    }
}