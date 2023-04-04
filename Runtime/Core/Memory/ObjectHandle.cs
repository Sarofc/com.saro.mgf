using System;

namespace Saro.Pool
{
    public interface IHandledObject
    {
        /// <summary>
        /// 实例ID，从1开始自增，0代表无效
        /// <code>回收后需要置为0，才能保证if(handle)正确</code>
        /// </summary>
        int ObjectId { get; }
    }

    /// <summary>
    /// 针对 池对象 的引用问题
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct ObjectHandle<T> : IEquatable<ObjectHandle<T>> where T : class, IHandledObject
    {
        public readonly T Value => IsValid() ? m_Object : null;

        public readonly int Handle => IsValid() ? m_CachedObjectID : 0;

        private readonly T m_Object;
        private readonly int m_CachedObjectID;

        public ObjectHandle(T obj)
        {
            m_Object = obj;
            m_CachedObjectID = obj == null ? 0 : obj.ObjectId;
        }

        private bool IsValid() => m_CachedObjectID != 0 && m_Object != null && m_Object.ObjectId == m_CachedObjectID;

        public static implicit operator T(in ObjectHandle<T> handle) => handle.Value;

        public static implicit operator bool(in ObjectHandle<T> handle) => handle.IsValid();

        public bool Equals(ObjectHandle<T> other) => CompareObjects(this, other);

        public override bool Equals(object obj)
        {
            if (obj is not ObjectHandle<T>) return false;
            return Equals((ObjectHandle<T>)obj);
        }

        public static bool operator ==(in ObjectHandle<T> lhs, in ObjectHandle<T> rhs) => CompareObjects(lhs, rhs);

        public static bool operator !=(in ObjectHandle<T> lhs, in ObjectHandle<T> rhs) => !CompareObjects(lhs, rhs);

        public override int GetHashCode() => m_CachedObjectID;

        private static bool CompareObjects(in ObjectHandle<T> lhs, in ObjectHandle<T> rhs)
        {
            var validA = lhs.IsValid();
            var validB = rhs.IsValid();

            if (validA && !validB) return false;
            if (!validA && validB) return false;

            return lhs.m_CachedObjectID == rhs.m_CachedObjectID;
        }

        public override string ToString()
        {
            return $"{m_CachedObjectID}:{m_Object}";
        }
    }
}
