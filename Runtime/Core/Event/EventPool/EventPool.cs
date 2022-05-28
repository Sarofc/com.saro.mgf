using Saro.Collections;
using System;
using System.Collections.Generic;

namespace Saro.Events
{
    /*
     * thread unsafe
     */
    public sealed partial class EventPool<T> where T : BaseEventArgs
    {
        private readonly TMultiMap<int, EventHandler<T>> m_EventHanders;
        private readonly Queue<Event> m_Events;
        private readonly Dictionary<object, LinkedListNode<EventHandler<T>>> m_CachedNodes;
        private readonly Dictionary<object, LinkedListNode<EventHandler<T>>> m_TempNodes;
        private readonly EEventPoolMode m_EventPoolMode;

        private EventHandler<T> m_DefaultHandler;

        public EventPool(EEventPoolMode eventPoolMode)
        {
            m_EventPoolMode = eventPoolMode;

            m_EventHanders = new TMultiMap<int, EventHandler<T>>();
            m_Events = new Queue<Event>();
            m_CachedNodes = new Dictionary<object, LinkedListNode<EventHandler<T>>>();
            m_TempNodes = new Dictionary<object, LinkedListNode<EventHandler<T>>>();
            m_DefaultHandler = null;
        }

        /// <summary>
        /// 所有 事件处理函数 的数量
        /// </summary>
        public int EventHandlerCount => m_EventHanders.Count;
        /// <summary>
        /// 所有 事件ID 的数量
        /// </summary>
        public int EventCount => m_Events.Count;


        /// <summary>
        /// 事件队列轮询
        /// </summary>
        public void Update()
        {
            lock (m_Events)
            {
                while (m_Events.Count > 0)
                {
                    var evt = m_Events.Dequeue();
                    HandleEvent(evt.Sender, evt.EventArgs);
                    Event.Release(evt);
                }
            }
        }

        public void Dispose()
        {
            Clear();
            m_EventHanders.Clear();
            m_CachedNodes.Clear();
            m_TempNodes.Clear();
            m_DefaultHandler = null;
        }

        /// <summary>
        /// 清理事件队列，仅影响 BroadcastQueued
        /// </summary>
        public void Clear()
        {
            lock (m_Events)
            {
                m_Events.Clear();
            }
        }

        /// <summary>
        /// 此事件绑定的 处理函数 数量
        /// </summary>
        /// <param name="id">事件ID</param>
        /// <returns></returns>
        public int Count(int id)
        {
            if (m_EventHanders.TryGetValue(id, out TLinkedListRange<EventHandler<T>> range))
            {
                return range.Count;
            }

            return 0;
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="id">事件ID</param>
        /// <param name="handler">事件处理函数</param>
        /// <returns></returns>
        public bool HasEvent(int id, EventHandler<T> handler)
        {
            if (handler == null)
            {
                throw new MyEventException("Event handler is invalid.");
            }

            return m_EventHanders.Contains(id, handler);
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="id">事件ID</param>
        /// <param name="handler">事件处理函数</param>
        public void Subscribe(int id, EventHandler<T> handler)
        {
            if (handler == null)
            {
                throw new MyEventException("Event handler is invalid.");
            }

            if (!m_EventHanders.ContainsKey(id))
            {
                m_EventHanders.Add(id, handler);
            }
            else if ((m_EventPoolMode & EEventPoolMode.AllowMultiHandler) == 0)
            {
                throw new MyEventException(string.Format("MultiHandler is not allowed, Event id : {0}", id));
            }
            else if ((m_EventPoolMode & EEventPoolMode.AllowDuplicateHandler) == 0 && HasEvent(id, handler))
            {
                throw new MyEventException(string.Format("DuplicateHandler is not allowed, Event id : {0}", id));
            }
            else
            {
                m_EventHanders.Add(id, handler);
            }
        }

        /// <summary>
        /// 反注册事件
        /// </summary>
        /// <param name="id">事件ID</param>
        /// <param name="handler">事件处理函数</param>
        public void Unsubscribe(int id, EventHandler<T> handler)
        {
            if (handler == null)
            {
                throw new MyEventException("Event handler is invalid");
            }

            if (m_CachedNodes.Count > 0)
            {
                foreach (KeyValuePair<object, LinkedListNode<EventHandler<T>>> cacheNode in m_CachedNodes)
                {
                    if (cacheNode.Value != null && cacheNode.Value.Value == handler)
                    {
                        m_TempNodes.Add(cacheNode.Key, cacheNode.Value.Next);
                    }
                }

                if (m_TempNodes.Count > 0)
                {
                    foreach (KeyValuePair<object, LinkedListNode<EventHandler<T>>> tempNode in m_TempNodes)
                    {
                        m_CachedNodes[tempNode.Key] = tempNode.Value;
                    }

                    m_TempNodes.Clear();
                }
            }

            if (!m_EventHanders.Remove(id, handler))
            {
                throw new MyEventException((string.Format("Event handler not found: {0}", id)));
            }
        }

        /// <summary>
        /// 反注册事件
        /// </summary>
        /// <param name="id">事件ID</param>
        public void UnsubscribeAll(int id)
        {
            if (!m_EventHanders.RemoveAll(id))
            {
                //throw new MyEventException((string.Format("Event handler not found: {0}", id)));
            }
        }

        /// <summary>
        /// 设置默认事件处理器
        /// </summary>
        /// <param name="handler"></param>
        public void SetDefaultHandler(EventHandler<T> handler)
        {
            m_DefaultHandler = handler;
        }

        /// <summary>
        /// 广播时间，但延迟到下一帧
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void BroadcastQueued(object sender, T e)
        {
            Event evt = Event.Acquire(sender, e);
            lock (m_Events)
            {
                m_Events.Enqueue(evt);
            }
        }

        /// <summary>
        /// 广播事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Broadcast(object sender, T e)
        {
            HandleEvent(sender, e);
        }

        private void HandleEvent(object sender, T e)
        {
            bool handlerException = false;
            if (m_EventHanders.TryGetValue(e.ID, out TLinkedListRange<EventHandler<T>> range))
            {
                LinkedListNode<EventHandler<T>> current = range.Head;
                while (current != null && current != range.Tail)
                {
                    // 处理在 Handler 里 Unsubscribe 事件，导致 链表断开的问题
                    m_CachedNodes[e] = current.Next != range.Tail ? current.Next : null;
                    current.Value(sender, e);
                    current = m_CachedNodes[e];
                }

                m_CachedNodes.Remove(e);
            }
            else if (m_DefaultHandler != null)
            {
                m_DefaultHandler(sender, e);
            }
            else if ((m_EventPoolMode & EEventPoolMode.AllowNoHandler) == 0)
            {
                handlerException = true;
            }

            SharedPool.Return(e);

            if (handlerException)
            {
                throw new MyEventException(string.Format("NoEventHandler is not allowed, event id: {0}", e.ID));
            }
        }
    }
}