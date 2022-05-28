//#define DEBUG_SHARED_POOL

using System;
using System.Collections.Generic;

/// <summary>
/// 通用型对象池
/// <code>非容器C#类，用这个，其他剩余情况使用 <see cref="Saro.Pool.IObjectPool{T}"/></code>
/// <code>Thread safe</code>
/// </summary>
public static partial class SharedPool
{
    public const string k_DEBUG_REFERENCE_POOL = "DEBUG_SHARED_POOL";

    private static readonly Dictionary<Type, ReferenceCollection> s_ReferenceCollections = new Dictionary<Type, ReferenceCollection>();

    /// <summary>
    /// 获取对象池的数量。
    /// </summary>
    public static int Count => s_ReferenceCollections.Count;

    /// <summary>
    /// 仅测试用!!!获取所有对象池的信息。
    /// </summary>
    /// <returns>所有对象的信息。</returns>
    public static void GetAllReferencePoolInfos(ref List<SharedPoolInfo> results)
    {
        results.Clear();

        lock (s_ReferenceCollections)
        {
            foreach (KeyValuePair<Type, ReferenceCollection> referenceCollection in s_ReferenceCollections)
            {
                var info = new SharedPoolInfo(referenceCollection.Key, referenceCollection.Value.UnusedReferenceCount, referenceCollection.Value.UsingReferenceCount, referenceCollection.Value.AcquireReferenceCount, referenceCollection.Value.ReleaseReferenceCount, referenceCollection.Value.AddReferenceCount, referenceCollection.Value.RemoveReferenceCount);
                results.Add(info);
            }
        }
    }

    /// <summary>
    /// 仅测试用!!!获取对象池的信息。
    /// </summary>
    /// <param name="key">对象类型</param>
    /// <returns>对象池的信息。</returns>
    public static SharedPoolInfo GetReferencePoolInfo(Type key)
    {
        SharedPoolInfo info = default;
        lock (s_ReferenceCollections)
        {
            if (s_ReferenceCollections.TryGetValue(key, out ReferenceCollection collection))
            {
                info = new SharedPoolInfo(key, collection.UnusedReferenceCount, collection.UsingReferenceCount, collection.AcquireReferenceCount, collection.ReleaseReferenceCount, collection.AddReferenceCount, collection.RemoveReferenceCount);
                return info;
            }
        }

        return info;
    }

    /// <summary>
    /// 清除所有对象池。
    /// </summary>
    public static void ClearAll()
    {
        lock (s_ReferenceCollections)
        {
            foreach (KeyValuePair<Type, ReferenceCollection> referenceCollection in s_ReferenceCollections)
            {
                referenceCollection.Value.RemoveAll();
            }

            s_ReferenceCollections.Clear();
        }
    }

    /// <summary>
    /// 从对象池获取对象。
    /// </summary>
    /// <typeparam name="T">对象类型。</typeparam>
    /// <returns>对象。</returns>
    public static T Rent<T>() where T : class, IReference, new()
    {
        return GetReferenceCollection(typeof(T)).Rent<T>();
    }

    /// <summary>
    /// 从对象池获取对象。
    /// </summary>
    /// <param name="referenceType">对象类型。</param>
    /// <returns>引用。</returns>
    public static IReference Rent(Type referenceType)
    {
        __internal_check_reference_type_valid(referenceType);
        return GetReferenceCollection(referenceType).Rent();
    }

    /// <summary>
    /// 将对象归还对象池。
    /// </summary>
    /// <param name="reference">对象。</param>
    public static void Return(IReference reference)
    {
        if (reference == null)
        {
            __throw_exception_nullreference(reference);
            return;
        }

        Type referenceType = reference.GetType();

        __internal_check_reference_type_valid(referenceType);

        GetReferenceCollection(referenceType).Return(reference);
    }

    /// <summary>
    /// 向对象池中追加指定数量的对象。
    /// </summary>
    /// <typeparam name="T">对象类型。</typeparam>
    /// <param name="count">追加数量。</param>
    public static void Add<T>(int count) where T : class, IReference, new()
    {
        GetReferenceCollection(typeof(T)).Add<T>(count);
    }

    /// <summary>
    /// 向对象池中追加指定数量的对象。
    /// </summary>
    /// <param name="referenceType">引用类型。</param>
    /// <param name="count">追加数量。</param>
    public static void Add(Type referenceType, int count)
    {
        __internal_check_reference_type_valid(referenceType);
        GetReferenceCollection(referenceType).Add(count);
    }

    /// <summary>
    /// 从对象池中移除指定数量的对象。
    /// </summary>
    /// <typeparam name="T">对象类型。</typeparam>
    /// <param name="count">移除数量。</param>
    public static void Remove<T>(int count) where T : class, IReference
    {
        GetReferenceCollection(typeof(T)).Remove(count);
    }

    /// <summary>
    /// 从对象池中移除指定数量的对象。
    /// </summary>
    /// <param name="referenceType">对象类型。</param>
    /// <param name="count">移除数量。</param>
    public static void Remove(Type referenceType, int count)
    {
        __internal_check_reference_type_valid(referenceType);
        GetReferenceCollection(referenceType).Remove(count);
    }

    /// <summary>
    /// 从对象池中移除这种类型的所有对象。
    /// </summary>
    /// <typeparam name="T">对象类型。</typeparam>
    public static void RemoveAll<T>() where T : class, IReference
    {
        GetReferenceCollection(typeof(T)).RemoveAll();
    }

    /// <summary>
    /// 从对象池中移除这种类型的所有对象。
    /// </summary>
    /// <param name="referenceType">对象类型。</param>
    public static void RemoveAll(Type referenceType)
    {
        __internal_check_reference_type_valid(referenceType);
        GetReferenceCollection(referenceType).RemoveAll();
    }

    public static ReferenceCollection GetReferenceCollection(Type referenceType)
    {
        if (referenceType == null)
        {
            __throw_exception_reference_type_invalid();
        }

        ReferenceCollection referenceCollection = null;
        lock (s_ReferenceCollections)
        {
            if (!s_ReferenceCollections.TryGetValue(referenceType, out referenceCollection))
            {
                referenceCollection = new ReferenceCollection(referenceType);
                s_ReferenceCollections.Add(referenceType, referenceCollection);
            }
        }

        return referenceCollection;
    }

    private static void __throw_exception_reference_type_invalid()
    {
        throw new Exception("[SharedPool] ReferenceType is invalid.");
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private static void __throw_exception_nullreference(IReference reference)
    {
        throw new Exception("[SharedPool] Reference is null.");
    }

    [System.Diagnostics.Conditional(k_DEBUG_REFERENCE_POOL)]
    private static void __internal_check_reference_type_valid(Type referenceType)
    {
        if (referenceType == null)
        {
            __throw_exception_reference_type_invalid();
        }

        if (!referenceType.IsClass || referenceType.IsAbstract)
        {
            throw new Exception("[SharedPool] Reference type is not a non-abstract class type.");
        }

        if (!typeof(IReference).IsAssignableFrom(referenceType))
        {
            throw new Exception(string.Format("[SharedPool] Reference type '{0}' is invalid. Must implement IReference interface.", referenceType.FullName));
        }
    }

    [System.Diagnostics.Conditional(k_DEBUG_REFERENCE_POOL)]
    private static void __internal_check_reference_released(Queue<IReference> references, IReference reference)
    {
        if (references.Contains(reference))
        {
            throw new Exception("[SharedPool] The reference has been released.");
        }
    }


    public sealed class ReferenceCollection
    {
        private readonly Queue<IReference> m_References;
        private readonly Type m_ReferenceType;
        private int m_UsingReferenceCount;
        private int m_AcquireReferenceCount;
        private int m_ReleaseReferenceCount;
        private int m_AddReferenceCount;
        private int m_RemoveReferenceCount;

        public ReferenceCollection(Type referenceType)
        {
            m_References = new Queue<IReference>();
            m_ReferenceType = referenceType;
            m_UsingReferenceCount = 0;
            m_AcquireReferenceCount = 0;
            m_ReleaseReferenceCount = 0;
            m_AddReferenceCount = 0;
            m_RemoveReferenceCount = 0;
        }

        public Type ReferenceType => m_ReferenceType;

        public int UnusedReferenceCount => m_References.Count;

        public int UsingReferenceCount => m_UsingReferenceCount;

        public int AcquireReferenceCount => m_AcquireReferenceCount;

        public int ReleaseReferenceCount => m_ReleaseReferenceCount;

        public int AddReferenceCount => m_AddReferenceCount;

        public int RemoveReferenceCount => m_RemoveReferenceCount;

        public T Rent<T>() where T : class, IReference, new()
        {
            if (typeof(T) != m_ReferenceType)
            {
                throw new Exception("Type is invalid.");
            }

            m_UsingReferenceCount++;
            m_AcquireReferenceCount++;
            lock (m_References)
            {
                if (m_References.Count > 0)
                {
                    return (T)m_References.Dequeue();
                }
            }

            m_AddReferenceCount++;
            return new T();
        }

        public IReference Rent()
        {
            m_UsingReferenceCount++;
            m_AcquireReferenceCount++;
            lock (m_References)
            {
                if (m_References.Count > 0)
                {
                    return m_References.Dequeue();
                }
            }

            m_AddReferenceCount++;
            return (IReference)Activator.CreateInstance(m_ReferenceType);
        }

        public void Return(IReference reference)
        {
            reference.IReferenceClear();
            lock (m_References)
            {
                __internal_check_reference_released(m_References, reference);

                m_References.Enqueue(reference);
            }

            m_ReleaseReferenceCount++;
            m_UsingReferenceCount--;
        }


        public void Add<T>(int count) where T : class, IReference, new()
        {
            if (typeof(T) != m_ReferenceType)
            {
                throw new Exception("Type is invalid.");
            }

            lock (m_References)
            {
                m_AddReferenceCount += count;
                while (count-- > 0)
                {
                    m_References.Enqueue(new T());
                }
            }
        }

        public void Add(int count)
        {
            lock (m_References)
            {
                m_AddReferenceCount += count;
                while (count-- > 0)
                {
                    m_References.Enqueue((IReference)Activator.CreateInstance(m_ReferenceType));
                }
            }
        }

        public void Remove(int count)
        {
            lock (m_References)
            {
                if (count > m_References.Count)
                {
                    count = m_References.Count;
                }

                m_RemoveReferenceCount += count;
                while (count-- > 0)
                {
                    m_References.Dequeue();
                }
            }
        }

        public void RemoveAll()
        {
            lock (m_References)
            {
                m_RemoveReferenceCount += m_References.Count;
                m_References.Clear();
            }
        }
    }
}
