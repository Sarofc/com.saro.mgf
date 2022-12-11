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

        public ObjectPool(Func<T> onCreate, Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000)
        {
            if (onCreate == null)
            {
                throw new ArgumentNullException("createFunc");
            }
            bool flag2 = maxSize <= 0;
            if (flag2)
            {
                throw new ArgumentException("Max Size must be greater than 0", "maxSize");
            }
            m_Stack = new Stack<T>(defaultCapacity);
            m_OnCreate = onCreate;
            m_OnGet = onGet;
            m_OnRelease = onRelease;
            m_OnDestroy = onDestroy;
            m_MaxSize = maxSize;
            m_CollectionCheck = collectionCheck;

            ObjectPoolChecker.Register(this);
        }

        public ObjectPool(Func<UniTask<T>> onCreateAsync, Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000)
        {
            if (onCreateAsync == null)
            {
                throw new ArgumentNullException("createFuncAsync");
            }
            bool flag2 = maxSize <= 0;
            if (flag2)
            {
                throw new ArgumentException("Max Size must be greater than 0", "maxSize");
            }
            m_Stack = new Stack<T>(defaultCapacity);
            m_CreateAsync = onCreateAsync;
            m_MaxSize = maxSize;
            m_OnGet = onGet;
            m_OnRelease = onRelease;
            m_OnDestroy = onDestroy;
            m_CollectionCheck = collectionCheck;

            ObjectPoolChecker.Register(this);
        }

        public T Rent()
        {
            T t;
            if (m_Stack.Count == 0)
            {
                t = m_OnCreate();
                CountAll += 1;
            }
            else
            {
                t = m_Stack.Pop();
            }
            Action<T> onGet = m_OnGet;
            if (onGet != null)
            {
                onGet(t);
            }
            RentCount++;
            return t;
        }

        public async UniTask<T> RentAsync()
        {
            T t;
            if (m_Stack.Count == 0)
            {
                t = await m_CreateAsync();
                CountAll += 1;
            }
            else
            {
                t = m_Stack.Pop();
            }
            Action<T> onGet = m_OnGet;
            if (onGet != null)
            {
                onGet(t);
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
            m_OnRelease?.Invoke(element);
            if (CountInactive < m_MaxSize)
            {
                m_Stack.Push(element);
            }
            else
            {
                m_OnDestroy?.Invoke(element);
            }
            ReturnCount++;
        }

        public void Clear()
        {
            if (m_OnDestroy != null)
            {
                foreach (T obj in m_Stack)
                {
                    m_OnDestroy(obj);
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

        private readonly Func<T> m_OnCreate;

        private readonly Func<UniTask<T>> m_CreateAsync;

        private readonly Action<T> m_OnGet;

        private readonly Action<T> m_OnRelease;

        private readonly Action<T> m_OnDestroy;

        private readonly int m_MaxSize;

        internal bool m_CollectionCheck;
    }
}
