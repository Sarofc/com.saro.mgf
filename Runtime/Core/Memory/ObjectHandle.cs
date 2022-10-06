using System;

namespace Saro.Pool
{
    public interface IHandledObject
    {
        /// <summary>
        /// 实例ID，从1开始自增，0代表无效
        /// </summary>
        public int ObjectID { get; }
    }

    /// <summary>
    /// 针对 池对象 的引用问题
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct ObjectHandle<T> : IEquatable<ObjectHandle<T>> where T : class, IHandledObject
    {
        public readonly T Object
        {
            get
            {
                if (!IsValid())
                {
                    Log.ERROR($"{nameof(ObjectHandle<T>)} is invalid");
                    return null;
                }
                return m_Object;
            }
        }

        public readonly int Handle
        {
            get
            {
                if (!IsValid())
                {
                    Log.ERROR($"{nameof(ObjectHandle<T>)} is invalid");
                    return 0;
                }
                return m_CachedObjectID;
            }
        }

        private readonly T m_Object;
        private readonly int m_CachedObjectID;

        public ObjectHandle(T obj)
        {
            m_Object = obj;
            m_CachedObjectID = obj.ObjectID;
        }

        private bool IsValid() => m_CachedObjectID != 0 && m_Object != null && m_Object.ObjectID == m_CachedObjectID;

        public static explicit operator T(ObjectHandle<T> handle) => handle ? handle.m_Object : null;

        public static implicit operator bool(ObjectHandle<T> handle) => handle.IsValid();

        public bool Equals(ObjectHandle<T> other) => CompareObjects(this, other);

        public override bool Equals(object obj)
        {
            if (!(obj is ObjectHandle<T>)) return false;
            return Equals((ObjectHandle<T>)obj);
        }

        public static bool operator ==(ObjectHandle<T> lhs, ObjectHandle<T> rhs) => CompareObjects(lhs, rhs);

        public static bool operator !=(ObjectHandle<T> lhs, ObjectHandle<T> rhs) => !CompareObjects(lhs, rhs);

        public override int GetHashCode() => m_CachedObjectID;

        private static bool CompareObjects(ObjectHandle<T> lhs, ObjectHandle<T> rhs)
        {
            var validA = lhs.IsValid();
            var validB = rhs.IsValid();

            if (validA && !validB) return false;
            if (!validA && validB) return false;

            return lhs.m_CachedObjectID == rhs.m_CachedObjectID;
        }
    }
}
