using System;

namespace Saro
{
    public interface ILateUpdateSystem : ISystemType
    {
        void Run(object o);
    }

    [FObjectSystem]
    public abstract class LateUpdateSystem<T> : ILateUpdateSystem
    {
        public void Run(object o)
        {
            LateUpdate((T)o);
        }

        public Type Type()
        {
            return typeof(T);
        }

        public Type SystemType()
        {
            return typeof(ILateUpdateSystem);
        }

        public abstract void LateUpdate(T self);
    }
}
