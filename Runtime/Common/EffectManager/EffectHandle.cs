using System;

namespace Saro.Gameplay.Effect
{
    public struct EffectHandle : IEquatable<EffectHandle>
    {
        public EffectScriptBase EffectScript { get; private set; }

        public int Handle => m_CachedObjectID;
        private readonly int m_CachedObjectID;

        public EffectHandle(EffectScriptBase effect)
        {
            EffectScript = effect;
            m_CachedObjectID = effect.ObjectID;
        }

        public void Dispose()
        {
            EffectScript = null;
        }

        private bool IsValid()
        {
            return EffectScript != null && EffectScript.ObjectID == m_CachedObjectID;
        }

        public static explicit operator EffectScriptBase(EffectHandle handle)
        {
            return handle ? handle.EffectScript : null;
        }

        public static implicit operator bool(EffectHandle handle)
        {
            return handle.IsValid();
        }

        public bool Equals(EffectHandle other)
        {
            return CompareObjects(this, other);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is EffectHandle)) return false;

            return Equals((EffectHandle)obj);
        }

        public static bool operator ==(EffectHandle lhs, EffectHandle rhs)
        {
            return CompareObjects(lhs, rhs);
        }

        public static bool operator !=(EffectHandle lhs, EffectHandle rhs)
        {
            return !CompareObjects(lhs, rhs);
        }

        private static bool CompareObjects(EffectHandle lhs, EffectHandle rhs)
        {
            var validA = lhs.IsValid();
            var validB = rhs.IsValid();

            if (validA && !validB) return false;
            if (!validA && validB) return false;

            return lhs.m_CachedObjectID == rhs.m_CachedObjectID;
        }

        public override int GetHashCode()
        {
            return m_CachedObjectID;
        }
    }
}
