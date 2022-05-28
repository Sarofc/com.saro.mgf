using System;

namespace Saro
{
    public interface ILoadSystem : ISystemType
    {
        void Run(object o);
    }

    [FObjectSystem]
    public abstract class LoadSystem<T> : ILoadSystem
    {
        public void Run(object o)
        {
            Load((T)o);
        }

        public Type Type()
        {
            return typeof(T);
        }

        public Type SystemType()
        {
            return typeof(ILoadSystem);
        }

        public abstract void Load(T self);
    }
}
