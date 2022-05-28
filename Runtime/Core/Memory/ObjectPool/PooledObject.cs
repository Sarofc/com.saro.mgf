using System;

namespace Saro.Pool
{
    public struct PooledObject<T> : IDisposable where T : class
    {
        internal PooledObject(T value, IObjectPool<T> pool)
        {
            m_ToReturn = value;
            m_Pool = pool;
        }

        void IDisposable.Dispose()
        {
            m_Pool.Return(m_ToReturn);
        }

        private readonly T m_ToReturn;

        private readonly IObjectPool<T> m_Pool;
    }
}
