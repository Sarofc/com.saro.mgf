using System;

namespace Saro
{
    public interface IDestroySystem : ISystemType
    {
        void Run(object o);
    }

    [FObjectSystem]
    public abstract class DestroySystem<T> : IDestroySystem
    {
        public void Run(object o)
        {
            Destroy((T)o);
        }

        public Type SystemType()
        {
            return typeof(IDestroySystem);
        }

        public Type Type()
        {
            return typeof(T);
        }

        public abstract void Destroy(T self);
    }
}
