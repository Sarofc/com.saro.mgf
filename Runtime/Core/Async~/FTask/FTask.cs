using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace Saro.Tasks
{
    [AsyncMethodBuilder(typeof(AsyncFTaskMethodBuilder))]
    public partial class FTask : ICriticalNotifyCompletion
    {
        public static FTaskCompleted CompletedTask => new FTaskCompleted();

        private static readonly Queue<FTask> s_Queue = new Queue<FTask>();

        public static FTask Create(bool fromPool = false)
        {
            if (!fromPool) return new FTask();

            if (s_Queue.Count == 0) return new FTask() { m_FromPool = true };

            return s_Queue.Dequeue();
        }

        private void Release()
        {
            if (!m_FromPool) return;

            m_State = EAwaiterStatus.Pending;
            m_Callback = null;
            s_Queue.Enqueue(this);
            // 太多了，回收一下
            if (s_Queue.Count > 1000)
            {
                s_Queue.Clear();
            }
        }

        private EAwaiterStatus m_State;
        private bool m_FromPool;
        private object m_Callback;

        private FTask()
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public FTask GetAwaiter()
        {
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void Coroutine()
        {
            InnerCoroutine().Coroutine();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        private async FVoid InnerCoroutine()
        {
            await this;
        }

        public bool IsCompleted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [DebuggerHidden]
            get
            {
                return m_State != EAwaiterStatus.Pending;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void UnsafeOnCompleted(Action action)
        {
            if (m_State != EAwaiterStatus.Pending)
            {
                action?.Invoke();
                return;
            }

            m_Callback = action;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void OnCompleted(Action action)
        {
            UnsafeOnCompleted(action);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void GetResult()
        {
            switch (m_State)
            {
                case EAwaiterStatus.Succeeded:
                    Release();
                    break;
                case EAwaiterStatus.Faulted:
                    var exp = m_Callback as ExceptionDispatchInfo;
                    m_Callback = null;
                    exp?.Throw();
                    Release();
                    break;
                default:
                    throw new NotSupportedException("FTask does not allow call GetResult directly when task not completed. Please use 'await'.");

            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void SetResult()
        {
            if (m_State != EAwaiterStatus.Pending)
            {
                throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
            }

            m_State = EAwaiterStatus.Succeeded;

            var action = m_Callback as Action;
            m_Callback = null;
            action?.Invoke();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void SetException(Exception e)
        {
            if (m_State != EAwaiterStatus.Pending)
            {
                throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
            }

            m_State = EAwaiterStatus.Faulted;

            var action = m_Callback as Action;
            m_Callback = ExceptionDispatchInfo.Capture(e);
            action?.Invoke();
        }
    }

    [AsyncMethodBuilder(typeof(AsyncFTaskMethodBuilder<>))]
    public class FTask<T> : ICriticalNotifyCompletion
    {
        private static readonly Queue<FTask<T>> s_Queue = new Queue<FTask<T>>();

        /// <summary>
        /// 请不要随便使用FTask的对象池，除非你完全搞懂了FTask!!!
        /// 假如开启了池,await之后不能再操作FTask，否则可能操作到再次从池中分配出来的FTask，产生灾难性的后果
        /// SetResult的时候请现将tcs置空，避免多次对同一个FTask.SetResult
        /// </summary>
        public static FTask<T> Create(bool fromPool = false)
        {
            if (!fromPool)
            {
                return new FTask<T>();
            }

            if (s_Queue.Count == 0)
            {
                return new FTask<T>() { m_FromPool = true };
            }
            return s_Queue.Dequeue();
        }

        private void Release()
        {
            if (!m_FromPool)
            {
                return;
            }
            m_Callback = null;
            m_Result = default;
            m_State = EAwaiterStatus.Pending;
            s_Queue.Enqueue(this);
            // 太多了，回收一下
            if (s_Queue.Count > 1000)
            {
                s_Queue.Clear();
            }
        }

        public T Result => m_Result;

        private bool m_FromPool;
        private EAwaiterStatus m_State;
        private T m_Result;
        private object m_Callback; // Action or ExceptionDispatchInfo

        private FTask()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        private async FVoid InnerCoroutine()
        {
            await this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void Coroutine()
        {
            InnerCoroutine().Coroutine();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public FTask<T> GetAwaiter()
        {
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public T GetResult()
        {
            switch (m_State)
            {
                case EAwaiterStatus.Succeeded:
                    T v = m_Result;
                    Release();
                    return v;
                case EAwaiterStatus.Faulted:
                    ExceptionDispatchInfo c = m_Callback as ExceptionDispatchInfo;
                    m_Callback = null;
                    c?.Throw();
                    Release();
                    return default;
                default:
                    throw new NotSupportedException("FTask does not allow call GetResult directly when task not completed. Please use 'await'.");
            }
        }


        public bool IsCompleted
        {
            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return m_State != EAwaiterStatus.Pending;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void UnsafeOnCompleted(Action action)
        {
            if (m_State != EAwaiterStatus.Pending)
            {
                action?.Invoke();
                return;
            }

            m_Callback = action;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void OnCompleted(Action action)
        {
            UnsafeOnCompleted(action);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void SetResult(T result)
        {
            if (m_State != EAwaiterStatus.Pending)
            {
                throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
            }

            m_State = EAwaiterStatus.Succeeded;

            m_Result = result;

            Action c = m_Callback as Action;
            m_Callback = null;
            c?.Invoke();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void SetException(Exception e)
        {
            if (m_State != EAwaiterStatus.Pending)
            {
                throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
            }

            m_State = EAwaiterStatus.Faulted;

            Action c = m_Callback as Action;
            m_Callback = ExceptionDispatchInfo.Capture(e);
            c?.Invoke();
        }
    }
}