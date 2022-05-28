using System;
using System.Collections.Generic;

namespace Saro.Collections
{
    public sealed class TBinaryHeap<T> where T : IComparable<T>
    {
        public int Count => m_Count;
        public int Capacity => m_Values.Length;

        public T this[int index]
        {
            get
            {
                return m_Values[index];
            }
        }

        private T[] m_Values;
        private Comparer<T> m_Comparer;

        private int m_Count;

        public TBinaryHeap() : this(0, Comparer<T>.Default)
        { }

        public TBinaryHeap(int capacity) : this(capacity, Comparer<T>.Default)
        { }

        public TBinaryHeap(Comparer<T> comparer) : this(0, comparer)
        {
        }

        public TBinaryHeap(int capacity, Comparer<T> comparer)
        {
            if (capacity > 0) m_Values = new T[capacity];
            m_Comparer = comparer ?? Comparer<T>.Default;
            m_Count = 0;
        }

        public TBinaryHeap(IList<T> list) : this(list, Comparer<T>.Default)
        { }

        public TBinaryHeap(IList<T> list, Comparer<T> comparer)
        {
            m_Values = new T[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                m_Values[i] = list[i];
            }
            m_Comparer = comparer ?? Comparer<T>.Default;
            m_Count = m_Values.Length;

            ReBuild();
        }

        public void Push(T val)
        {
            EnsureCapacity();

            m_Values[m_Count++] = val;

            UpHeap(m_Count - 1);
        }

        public T Pop()
        {
            if (m_Count == 0) throw new InvalidOperationException("no element to pop");

            Swap(0, --m_Count);
            DownHeap(1);

            return m_Values[m_Count];
        }

        public T Top()
        {
            return m_Values[0];
        }

        public void Clear()
        {
            Array.Clear(m_Values, 0, m_Count);
            m_Count = 0;
        }

        public void FastClear()
        {
            m_Count = 0;
        }

        public T[] ToArray()
        {
            var copy = new T[m_Count];
            Array.Copy(m_Values, copy, m_Count);
            return copy;
        }


        public void ReBuild()
        {
            for (int i = m_Count / 2; i >= 1; i--)
            {
                DownHeap(i);
            }
        }

        private void EnsureCapacity()
        {
            if (m_Count <= 0 && m_Values == null)
            {
                m_Values = new T[4];
            }
            else if (m_Values.Length <= m_Count)
            {
                var newValues = new T[Capacity * 2];
                Array.Copy(m_Values, newValues, m_Count);
                m_Values = newValues;
            }
        }

        private void UpHeap(int i)
        {
            int p = (i - 1) / 2;
            while (p >= 0)
            {
                if (m_Comparer.Compare(m_Values[p], m_Values[i]) <= 0) break;

                Swap(p, i);

                i = p;
                p = (i - 1) / 2;
            }
        }

        private void DownHeap(int i)
        {
            T d = m_Values[i - 1];
            int child;
            while (i <= m_Count / 2)
            {
                child = i * 2;
                if (child < m_Count && m_Comparer.Compare(m_Values[child - 1], m_Values[child]) > 0)
                    child++;

                if (m_Comparer.Compare(d, m_Values[child - 1]) <= 0)
                    break;

                m_Values[i - 1] = m_Values[child - 1];
                i = child;
            }
            m_Values[i - 1] = d;
        }

        private void Swap(int x, int y)
        {
            if (x == y) return;

            var tmp = m_Values[x];
            m_Values[x] = m_Values[y];
            m_Values[y] = tmp;
        }
    }
}