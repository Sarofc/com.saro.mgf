using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;

namespace Saro.Tasks
{
    public struct AsyncFTaskMethodBuilder
    {
        private FTask m_Tcs;

        // 1. Static Create method.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public static AsyncFTaskMethodBuilder Create()
        {
            AsyncFTaskMethodBuilder builder = new AsyncFTaskMethodBuilder() { m_Tcs = FTask.Create(true) };
            return builder;
        }

        // 2. TaskLike Task property.
        [DebuggerHidden]
        public FTask Task => this.m_Tcs;

        // 3. SetException
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void SetException(Exception exception)
        {
            this.m_Tcs.SetException(exception);
        }

        // 4. SetResult
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void SetResult()
        {
            this.m_Tcs.SetResult();
        }

        // 5. AwaitOnCompleted
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        // 6. AwaitUnsafeOnCompleted
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        // 7. Start
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        // 8. SetStateMachine
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }
    }

    public struct AsyncFTaskMethodBuilder<T>
    {
        private FTask<T> m_Tcs;

        // 1. Static Create method.
        [DebuggerHidden]
        public static AsyncFTaskMethodBuilder<T> Create()
        {
            AsyncFTaskMethodBuilder<T> builder = new AsyncFTaskMethodBuilder<T>() { m_Tcs = FTask<T>.Create(true) };
            return builder;
        }

        // 2. TaskLike Task property.
        [DebuggerHidden]
        public FTask<T> Task => this.m_Tcs;

        // 3. SetException
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void SetException(Exception exception)
        {
            this.m_Tcs.SetException(exception);
        }

        // 4. SetResult
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void SetResult(T ret)
        {
            this.m_Tcs.SetResult(ret);
        }

        // 5. AwaitOnCompleted
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        // 6. AwaitUnsafeOnCompleted
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        // 7. Start
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        // 8. SetStateMachine
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }
    }
}