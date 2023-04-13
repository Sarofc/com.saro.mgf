using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Saro
{
    /*
     * TODO 尽量保证和 delegate 行为一致
     * 
     * 不保证的
     * 1.  线程安全
     * 
     */

    /// <summary>
    /// 大致同 delegate，但+/-操作，分配内存要少一些
    /// <code>thread unsafe</code>
    /// </summary>
    public sealed class FDelegates
    {
    
        public List<Action> RuntimeCalls => m_RuntimeCalls ??= new List<Action>(m_DefaultCapacity);

        private readonly int m_DefaultCapacity = 8;
        private List<Action> m_RuntimeCalls;

        private List<int> m_DelayedRemoved;
        private List<Action> m_DelayedAdd;

        private int m_Version;
        private int m_LastVersion;

        public FDelegates()
        {
        }

        public FDelegates(int capacity)
        {
            m_DefaultCapacity = capacity;
        }

        public void Invoke()
        {
            if (m_RuntimeCalls != null && m_RuntimeCalls.Count > 0)
            {
                m_Version++;

                foreach (var call in CollectionsMarshal.AsSpan(m_RuntimeCalls))
                {
                    if (call != null)
                    {
                        try
                        {
                            call();
                        }
                        catch (Exception e)
                        {
                            Log.ERROR(e);
                        }
                    }
                }
                m_LastVersion = m_Version;

                if (m_DelayedRemoved != null && m_DelayedRemoved.Count > 0)
                {
                    for (int i = m_DelayedRemoved.Count - 1; i >= 0; i--)
                    {
                        var index = m_DelayedRemoved[i];
                        m_RuntimeCalls.RemoveAt(index);
                    }
                    m_DelayedRemoved.Clear();
                }

                if (m_DelayedAdd != null && m_DelayedAdd.Count > 0)
                {
                    for (int i = 0; i < m_DelayedAdd.Count; i++)
                    {
                        var call = m_DelayedAdd[i];
                        m_RuntimeCalls.Add(call);
                    }
                    m_DelayedAdd.Clear();
                }
            }
        }

        public static FDelegates operator +(FDelegates delegates, Action @delegate)
        {
            if (delegates.m_LastVersion != delegates.m_Version)
            {
                if (delegates.m_DelayedAdd == null)
                {
                    delegates.m_DelayedAdd = new List<Action>(delegates.m_DefaultCapacity / 2 + 1);
                }
                delegates.m_DelayedAdd.Add(@delegate);
            }
            else
            {
                delegates.RuntimeCalls.Add(@delegate);
            }

            return delegates;
        }

        public static FDelegates operator -(FDelegates delegates, Action @delegate)
        {
            if (delegates == null) return delegates; // incase, null reference

            if (delegates.m_RuntimeCalls == null) return delegates;

            var index = delegates.m_RuntimeCalls.IndexOf(@delegate);

            if (index < 0) return delegates;

            if (delegates.m_LastVersion != delegates.m_Version)
            {
                if (delegates.m_DelayedRemoved == null)
                {
                    delegates.m_DelayedRemoved = new List<int>(delegates.m_DefaultCapacity / 2 + 1);
                }
                delegates.m_DelayedRemoved.Add(index);
            }
            else
            {
                delegates.m_RuntimeCalls.RemoveAt(index);
            }

            return delegates;
        }
    }


    public sealed class FDelegates<T>
    {
        public List<Action<T>> RuntimeCalls => m_RuntimeCalls ??= new List<Action<T>>(m_DefaultCapacity);

        private readonly int m_DefaultCapacity = 8;
        private List<Action<T>> m_RuntimeCalls;

        private List<int> m_DelayedRemoved;
        private List<Action<T>> m_DelayedAdd;

        private int m_Version;
        private int m_LastVersion;

        public FDelegates()
        {
        }

        public FDelegates(int capacity)
        {
            m_DefaultCapacity = capacity;
        }

        public void Invoke(T arg)
        {
            if (m_RuntimeCalls != null && m_RuntimeCalls.Count > 0)
            {
                m_Version++;
                for (int i = 0; i < m_RuntimeCalls.Count; i++)
                {
                    Action<T> call = m_RuntimeCalls[i];
                    if (call != null)
                    {
                        try
                        {
                            call(arg);
                        }
                        catch (Exception e)
                        {
                            Log.ERROR(e);
                        }
                    }
                }
                m_LastVersion = m_Version;

                if (m_DelayedRemoved != null && m_DelayedRemoved.Count > 0)
                {
                    for (int i = m_DelayedRemoved.Count - 1; i >= 0; i--)
                    {
                        var index = m_DelayedRemoved[i];
                        m_RuntimeCalls.RemoveAt(index);
                    }
                    m_DelayedRemoved.Clear();
                }

                if (m_DelayedAdd != null && m_DelayedAdd.Count > 0)
                {
                    for (int i = 0; i < m_DelayedAdd.Count; i++)
                    {
                        var call = m_DelayedAdd[i];
                        m_RuntimeCalls.Add(call);
                    }
                    m_DelayedAdd.Clear();
                }
            }
        }

        public static FDelegates<T> operator +(FDelegates<T> delegates, Action<T> @delegate)
        {
            if (delegates.m_LastVersion != delegates.m_Version)
            {
                if (delegates.m_DelayedAdd == null)
                {
                    delegates.m_DelayedAdd = new List<Action<T>>(delegates.m_DefaultCapacity / 2 + 1);
                }
                delegates.m_DelayedAdd.Add(@delegate);
            }
            else
            {
                delegates.RuntimeCalls.Add(@delegate);
            }

            return delegates;
        }

        public static FDelegates<T> operator -(FDelegates<T> delegates, Action<T> @delegate)
        {
            if (delegates.m_RuntimeCalls == null) return delegates;

            var index = delegates.m_RuntimeCalls.IndexOf(@delegate);

            if (index < 0) return delegates;

            if (delegates.m_LastVersion != delegates.m_Version)
            {
                if (delegates.m_DelayedRemoved == null)
                {
                    delegates.m_DelayedRemoved = new List<int>(delegates.m_DefaultCapacity / 2 + 1);
                }
                delegates.m_DelayedRemoved.Add(index);
            }
            else
            {
                delegates.m_RuntimeCalls.RemoveAt(index);
            }

            return delegates;
        }
    }
}