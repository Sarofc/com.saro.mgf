using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Saro
{
    public class ThreadSynchronizationContext : SynchronizationContext
    {
        public new static ThreadSynchronizationContext Current => s_Instance;

        private static readonly ThreadSynchronizationContext s_Instance = new(Thread.CurrentThread.ManagedThreadId);

        private readonly int m_ThreadId;

        // 线程同步队列,发送接收socket回调都放到该队列,由poll线程统一执行
        private readonly ConcurrentQueue<Action> m_Queue;

        private Action m_TempAction;

        private ThreadSynchronizationContext(int threadId)
        {
            m_ThreadId = threadId;
            m_Queue = new();
        }

        internal void Update()
        {
            while (true)
            {
                // 目前如果积累了大量消息，某几帧可能会卡顿
                // TODO 时间切片？
                // TODO 缓存了 Action，能避免gc？
                if (!m_Queue.TryDequeue(out m_TempAction))
                {
                    return;
                }

                try
                {
                    m_TempAction();
                    //m_TempAction = null;
                }
                catch (Exception e)
                {
                    Log.ERROR("ThreadSynchronizationContext", e.ToString());
                }
            }
        }

        public override void Post(SendOrPostCallback callback, object state)
        {
            Post(() => callback(state));
        }

        public void Post(Action action)
        {
            if (Thread.CurrentThread.ManagedThreadId == m_ThreadId)
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    Log.ERROR("ThreadSynchronizationContext", e.ToString());
                }

                return;
            }

            m_Queue.Enqueue(action);
        }

        public void PostNext(Action action)
        {
            m_Queue.Enqueue(action);
        }
    }
}