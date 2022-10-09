using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Saro.Pool
{
    /// <summary>
    ///   <para>A stack based Pool.IObjectPool_1.</para>
    /// </summary>
    public class ObjectPool<T> : IDisposable, IObjectPool<T> where T : class
    {
        public int CountAll { get; private set; }

        public int CountActive => CountAll - CountInactive;

        public int CountInactive => m_Stack.Count;

        public int RentCount { get; private set; }
        public int ReturnCount { get; private set; }

        public ObjectPool(Func<T> createFunc, Action<T> actionOnGet = null, Action<T> actionOnRelease = null, Action<T> actionOnDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000)
        {
            if (createFunc == null)
            {
                throw new ArgumentNullException("createFunc");
            }
            bool flag2 = maxSize <= 0;
            if (flag2)
            {
                throw new ArgumentException("Max Size must be greater than 0", "maxSize");
            }
            m_Stack = new Stack<T>(defaultCapacity);
            m_CreateFunc = createFunc;
            m_MaxSize = maxSize;
            m_ActionOnGet = actionOnGet;
            m_ActionOnRelease = actionOnRelease;
            m_ActionOnDestroy = actionOnDestroy;
            m_CollectionCheck = collectionCheck;

            ObjectPoolChecker.Register(this);
        }

        public ObjectPool(Func<UniTask<T>> createFuncAsync, Action<T> actionOnGet = null, Action<T> actionOnRelease = null, Action<T> actionOnDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000)
        {
            if (createFuncAsync == null)
            {
                throw new ArgumentNullException("createFuncAsync");
            }
            bool flag2 = maxSize <= 0;
            if (flag2)
            {
                throw new ArgumentException("Max Size must be greater than 0", "maxSize");
            }
            m_Stack = new Stack<T>(defaultCapacity);
            m_CreateFuncAsync = createFuncAsync;
            m_MaxSize = maxSize;
            m_ActionOnGet = actionOnGet;
            m_ActionOnRelease = actionOnRelease;
            m_ActionOnDestroy = actionOnDestroy;
            m_CollectionCheck = collectionCheck;

            ObjectPoolChecker.Register(this);
        }

        public T Rent()
        {
            T t;
            if (m_Stack.Count == 0)
            {
                t = m_CreateFunc();
                CountAll += 1;
            }
            else
            {
                t = m_Stack.Pop();
            }
            Action<T> actionOnGet = m_ActionOnGet;
            if (actionOnGet != null)
            {
                actionOnGet(t);
            }
            RentCount++;
            return t;
        }

        public async UniTask<T> RentAsync()
        {
            T t;
            if (m_Stack.Count == 0)
            {
                t = await m_CreateFuncAsync();
                CountAll += 1;
            }
            else
            {
                t = m_Stack.Pop();
            }
            Action<T> actionOnGet = m_ActionOnGet;
            if (actionOnGet != null)
            {
                actionOnGet(t);
            }
            RentCount++;
            return t;
        }

        public PooledObject<T> Rent(out T v)
        {
            return new PooledObject<T>(v = Rent(), this);
        }

        public void Return(T element)
        {
            if (m_CollectionCheck && m_Stack.Count > 0)
            {
                if (m_Stack.Contains(element))
                {
                    throw new InvalidOperationException("Trying to release an object that has already been released to the pool.");
                }
            }
            m_ActionOnRelease?.Invoke(element);
            if (CountInactive < m_MaxSize)
            {
                m_Stack.Push(element);
            }
            else
            {
                m_ActionOnDestroy?.Invoke(element);
            }
            ReturnCount++;
        }

        public void Clear()
        {
            if (m_ActionOnDestroy != null)
            {
                foreach (T obj in m_Stack)
                {
                    m_ActionOnDestroy(obj);
                }
            }
            m_Stack.Clear();
            CountAll = 0;
            RentCount = 0;
            ReturnCount = 0;
        }

        public void Dispose()
        {
            Clear();
        }

        internal readonly Stack<T> m_Stack;

        private readonly Func<T> m_CreateFunc;

        private readonly Func<UniTask<T>> m_CreateFuncAsync;

        private readonly Action<T> m_ActionOnGet;

        private readonly Action<T> m_ActionOnRelease;

        private readonly Action<T> m_ActionOnDestroy;

        private readonly int m_MaxSize;

        internal bool m_CollectionCheck;
    }
}
