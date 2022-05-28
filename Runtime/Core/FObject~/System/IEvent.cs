using Cysharp.Threading.Tasks;
using System;

namespace Saro
{
    public interface IEvent
    {
        Type GetEventType();
    }

    [FEvent]
    public abstract class FEvent<A> : IEvent where A : struct
    {
        public Type GetEventType()
        {
            return typeof(A);
        }

        protected abstract UniTask Run(A a);

        public async UniTask Handle(A a)
        {
            try
            {
                await Run(a);
            }
            catch (Exception e)
            {
                Log.ERROR(e);
            }
        }
    }
}