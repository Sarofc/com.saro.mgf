

namespace Saro.Events
{
    public sealed partial class EventPool<T> where T : BaseEventArgs
    {
        private sealed class Event : IReference
        {
            private object m_Sender;
            private T m_EventArgs;

            public Event()
            {
                m_Sender = null;
                m_EventArgs = null;
            }

            public object Sender { get => m_Sender; }
            public T EventArgs { get => m_EventArgs; }

            public static Event Acquire(object sender, T e)
            {
                Event @event = SharedPool.Rent<Event>();
                @event.m_Sender = sender;
                @event.m_EventArgs = e;

                return @event;
            }

            public static void Release(Event @event)
            {
                SharedPool.Return(@event);
            }

            public void IReferenceClear()
            {
                m_Sender = null;
                m_EventArgs = null;
            }
        }
    }
}