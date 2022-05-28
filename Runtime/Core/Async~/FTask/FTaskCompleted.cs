using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Saro.Tasks
{
    [AsyncMethodBuilder(typeof(AsyncFTaskCompletedMethodBuilder))]
    public struct FTaskCompleted : ICriticalNotifyCompletion
    {
        [DebuggerHidden]
        public FTaskCompleted GetAwaiter()
        {
            return this;
        }

        [DebuggerHidden]
        public bool IsCompleted
        {
            get
            {
                return true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void GetResult()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void OnCompleted(Action continuation)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void UnsafeOnCompleted(Action continuation)
        {
        }
    }
}