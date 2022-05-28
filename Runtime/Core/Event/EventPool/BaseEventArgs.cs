using System;

namespace Saro.Events
{
    public abstract class BaseEventArgs : EventArgs, IReference
    {
        public abstract int ID { get; }

        public abstract void IReferenceClear();
    }
}