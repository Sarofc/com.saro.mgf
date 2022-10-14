#if true

namespace Saro.XConsole
{
    /*
     *  ring buffer
     * 
     *  Warning : 
     *  
     *  1. Can't expand capacity
     *  2. Set a value
     *      - Will override the 'first' value when array is full
     *      - If the array contains the value, move it to the 'last'
     *  
     */
    public class LiteRingBuffer<T>
    {
        public int Capacity => m_Arr.Length;
        public int Length => m_Size;

        public T this[int idx] => m_Arr[(m_Tail + idx) % m_Arr.Length];

        private int m_Tail;
        private int m_Size;

        private T[] m_Arr;

        public LiteRingBuffer(int capacity = 8)
        {
            m_Arr = new T[capacity];

            m_Tail = 0;
            m_Size = 0;
        }


        public void AddTail(T value)
        {
            if (m_Size == m_Arr.Length)
            {
                var idx = FindIndex(value);
                if (idx != -1)
                {
                    int lastIdx = m_Tail == 0 ? m_Size - 1 : (m_Tail - 1) % m_Arr.Length;

                    if (idx == lastIdx)
                    {
                        return;
                    }
                    else if (idx < lastIdx)
                    {
                        T cur = m_Arr[idx];
                        for (int i = idx + 1; i <= lastIdx; i++)
                        {
                            m_Arr[i - 1] = m_Arr[i];
                        }
                        m_Arr[lastIdx] = cur;
                    }
                    else
                    {
                        T cur = m_Arr[idx];
                        for (int i = idx + 1; i < m_Size; i++)
                        {
                            m_Arr[i - 1] = m_Arr[i];
                        }

                        m_Arr[m_Size - 1] = m_Arr[0];

                        for (int i = 1; i <= lastIdx; i++)
                        {
                            m_Arr[i - 1] = m_Arr[i];
                        }

                        m_Arr[lastIdx] = cur;
                    }
                }
                else
                {
                    m_Arr[m_Tail] = value;
                    m_Tail = (m_Tail + 1) % m_Arr.Length;
                }
            }
            else
            {
                var idx = FindIndex(value);
                if (idx != -1)
                {
                    T cur = m_Arr[idx];
                    for (int i = idx + 1; i < m_Size; i++)
                    {
                        m_Arr[i - 1] = m_Arr[i];
                    }
                    m_Arr[m_Size - 1] = cur;
                }
                else
                    m_Arr[m_Size++] = value;
            }
        }

        public void FastClear()
        {
            m_Size = 0;
            m_Tail = 0;
        }

        private int FindIndex(T value)
        {
            for (int i = 0; i < m_Size; i++)
            {
                if (m_Arr[i].Equals(value))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}

#endif