﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;

namespace Saro.Tasks
{
    public struct AsyncFVoidMethodBuilder
    {
        // 1. Static Create method.
        [DebuggerHidden]
        public static AsyncFVoidMethodBuilder Create()
        {
            AsyncFVoidMethodBuilder builder = new AsyncFVoidMethodBuilder();
            return builder;
        }

        // 2. TaskLike Task property(void)
        [DebuggerHidden]
        public FVoid Task => default;

        // 3. SetException
        [DebuggerHidden]
        public void SetException(Exception exception)
        {
            Log.ERROR(exception);
        }

        // 4. SetResult
        [DebuggerHidden]
        public void SetResult()
        {
            // do nothing
        }

        // 5. AwaitOnCompleted
        [DebuggerHidden]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        // 6. AwaitUnsafeOnCompleted
        [DebuggerHidden]
        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
        }

        // 7. Start
        [DebuggerHidden]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        // 8. SetStateMachine
        [DebuggerHidden]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }
    }
}