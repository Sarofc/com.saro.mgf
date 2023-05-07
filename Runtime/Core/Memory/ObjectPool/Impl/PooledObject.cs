using System;

namespace Saro.Pool
{
    public readonly struct PooledObject<T> : System.IDisposable where T : class
    {
        internal PooledObject(T value, IObjectPool<T> pool)
        {
            m_ToReturn = value;
            m_Pool = pool;
        }

        public void Dispose()
        {
            m_Pool.Return(m_ToReturn);
        }

        private readonly T m_ToReturn;

        private readonly IObjectPool<T> m_Pool;
    }
}
