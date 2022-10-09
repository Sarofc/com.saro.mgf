//using Cysharp.Threading.Tasks;
//using System;

//namespace Saro.Pool
//{
//    /// <summary>
//    ///   <para>A linked list version of Pool.IObjectPool_1.</para>
//    /// </summary>
//    public class LinkedPool<T> : IDisposable, IObjectPool<T> where T : class
//    {
//        public LinkedPool(Func<T> createFunc, Action<T> actionOnGet = null, Action<T> actionOnRelease = null, Action<T> actionOnDestroy = null, bool collectionCheck = true, int maxSize = 10000)
//        {
//            if (createFunc == null)
//            {
//                throw new ArgumentNullException("createFunc");
//            }
//            bool flag2 = maxSize <= 0;
//            if (flag2)
//            {
//                throw new ArgumentException("maxSize", "Max size must be greater than 0");
//            }
//            m_CreateFunc = createFunc;
//            m_ActionOnGet = actionOnGet;
//            m_ActionOnRelease = actionOnRelease;
//            m_ActionOnDestroy = actionOnDestroy;
//            m_Limit = maxSize;
//            m_CollectionCheck = collectionCheck;
//        }

//        public LinkedPool(Func<UniTask<T>> createFuncAsync, Action<T> actionOnGet = null, Action<T> actionOnRelease = null, Action<T> actionOnDestroy = null, bool collectionCheck = true, int maxSize = 10000)
//        {
//            if (createFuncAsync == null)
//            {
//                throw new ArgumentNullException("createFuncAsync");
//            }
//            bool flag2 = maxSize <= 0;
//            if (flag2)
//            {
//                throw new ArgumentException("maxSize", "Max size must be greater than 0");
//            }
//            m_CreateFuncAsync = createFuncAsync;
//            m_ActionOnGet = actionOnGet;
//            m_ActionOnRelease = actionOnRelease;
//            m_ActionOnDestroy = actionOnDestroy;
//            m_Limit = maxSize;
//            m_CollectionCheck = collectionCheck;
//        }

//        public int CountInactive { get; private set; }

//        public T Rent()
//        {
//            T t = default(T);
//            if (m_PoolFirst == null)
//            {
//                t = m_CreateFunc();
//            }
//            else
//            {
//                LinkedPool<T>.LinkedPoolItem poolFirst = m_PoolFirst;
//                t = poolFirst.value;
//                m_PoolFirst = poolFirst.poolNext;
//                poolFirst.poolNext = m_NextAvailableListItem;
//                m_NextAvailableListItem = poolFirst;
//                m_NextAvailableListItem.value = default(T);
//                CountInactive--;
//            }
//            Action<T> actionOnGet = m_ActionOnGet;
//            if (actionOnGet != null)
//            {
//                actionOnGet(t);
//            }
//            return t;
//        }

//        public async UniTask<T> RentAsync()
//        {
//            T t = default(T);
//            if (m_PoolFirst == null)
//            {
//                t = await m_CreateFuncAsync();
//            }
//            else
//            {
//                LinkedPool<T>.LinkedPoolItem poolFirst = m_PoolFirst;
//                t = poolFirst.value;
//                m_PoolFirst = poolFirst.poolNext;
//                poolFirst.poolNext = m_NextAvailableListItem;
//                m_NextAvailableListItem = poolFirst;
//                m_NextAvailableListItem.value = default(T);
//                CountInactive--;
//            }
//            Action<T> actionOnGet = m_ActionOnGet;
//            if (actionOnGet != null)
//            {
//                actionOnGet(t);
//            }
//            return t;
//        }

//        public PooledObject<T> Rent(out T v)
//        {
//            return new PooledObject<T>(v = Rent(), this);
//        }

//        public void Return(T item)
//        {
//            bool collectionCheck = m_CollectionCheck;
//            if (collectionCheck)
//            {
//                for (LinkedPool<T>.LinkedPoolItem linkedPoolItem = m_PoolFirst; linkedPoolItem != null; linkedPoolItem = linkedPoolItem.poolNext)
//                {
//                    bool flag = linkedPoolItem.value == item;
//                    if (flag)
//                    {
//                        throw new InvalidOperationException("Trying to release an object that has already been released to the pool.");
//                    }
//                }
//            }
//            m_ActionOnRelease?.Invoke(item);
//            if (CountInactive < m_Limit)
//            {
//                LinkedPool<T>.LinkedPoolItem linkedPoolItem2 = m_NextAvailableListItem;
//                if (linkedPoolItem2 == null)
//                {
//                    linkedPoolItem2 = new LinkedPool<T>.LinkedPoolItem();
//                }
//                else
//                {
//                    m_NextAvailableListItem = linkedPoolItem2.poolNext;
//                }
//                linkedPoolItem2.value = item;
//                linkedPoolItem2.poolNext = m_PoolFirst;
//                m_PoolFirst = linkedPoolItem2;
//                CountInactive++;
//            }
//            else
//            {
//                m_ActionOnDestroy?.Invoke(item);
//            }
//        }

//        public void Clear()
//        {
//            if (m_ActionOnDestroy != null)
//            {
//                for (LinkedPool<T>.LinkedPoolItem linkedPoolItem = m_PoolFirst; linkedPoolItem != null; linkedPoolItem = linkedPoolItem.poolNext)
//                {
//                    m_ActionOnDestroy(linkedPoolItem.value);
//                }
//            }
//            m_PoolFirst = null;
//            m_NextAvailableListItem = null;
//            CountInactive = 0;
//        }

//        public void Dispose()
//        {
//            Clear();
//        }

//        private readonly Func<T> m_CreateFunc;

//        private readonly Func<UniTask<T>> m_CreateFuncAsync;

//        private readonly Action<T> m_ActionOnGet;

//        private readonly Action<T> m_ActionOnRelease;

//        private readonly Action<T> m_ActionOnDestroy;

//        private readonly int m_Limit;

//        internal LinkedPool<T>.LinkedPoolItem m_PoolFirst;

//        internal LinkedPool<T>.LinkedPoolItem m_NextAvailableListItem;

//        private bool m_CollectionCheck;

//        internal class LinkedPoolItem
//        {
//            internal LinkedPool<T>.LinkedPoolItem poolNext;

//            internal T value;
//        }
//    }
//}
