using System;

namespace Saro.Events
{
    /// <summary>
    /// 全局事件管理器
    /// </summary>
    public sealed partial class EventManager
    {
        public static EventManager Global => Main.Resolve<EventManager>();

        private readonly EventPool<GameEventArgs> m_EventPool;

        public EventManager()
        {
            m_EventPool = new EventPool<GameEventArgs>(EEventPoolMode.AllowNoHandler | EEventPoolMode.AllowMultiHandler);
        }

        public int EventHandlerCount => m_EventPool.EventHandlerCount;
        public int EventCount => m_EventPool.EventCount;

        public int Count(int id)
        {
            return m_EventPool.Count(id);
        }

        public bool HasEvent(int id, EventHandler<GameEventArgs> handler)
        {
            return m_EventPool.HasEvent(id, handler);
        }

        public void Subscribe(int id, EventHandler<GameEventArgs> handler)
        {
            m_EventPool.Subscribe(id, handler);
        }

        public void Unsubscribe(int id, EventHandler<GameEventArgs> handler)
        {
            m_EventPool.Unsubscribe(id, handler);
        }

        public void UnsubscribeAll(int id)
        {
            m_EventPool.UnsubscribeAll(id);
        }

        public void SetDefaultHandler(EventHandler<GameEventArgs> handler)
        {
            m_EventPool.SetDefaultHandler(handler);
        }

        public void BroadcastQueued(object sender, GameEventArgs e)
        {
            m_EventPool.BroadcastQueued(sender, e);
        }

        public void Broadcast(object sender, GameEventArgs e)
        {
            m_EventPool.Broadcast(sender, e);
        }
    }

    partial class EventManager : IService, IServiceAwake, IServiceUpdate, IDisposable
    {
        void IServiceAwake.Awake() { }

        void IServiceUpdate.Update()
        {
            m_EventPool.Update();
        }

        void IDisposable.Dispose()
        {
            m_EventPool.Dispose();
        }
    }
}