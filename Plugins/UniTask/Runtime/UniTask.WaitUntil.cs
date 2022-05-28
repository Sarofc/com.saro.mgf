#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Cysharp.Threading.Tasks.Internal;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Cysharp.Threading.Tasks
{
    public partial struct UniTask
    {
        public static UniTask WaitUntil(Func<bool> predicate, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default(CancellationToken))
        {
            return new UniTask(WaitUntilPromise.Create(predicate, timing, cancellationToken, out var token), token);
        }

        public static UniTask WaitWhile(Func<bool> predicate, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default(CancellationToken))
        {
            return new UniTask(WaitWhilePromise.Create(predicate, timing, cancellationToken, out var token), token);
        }

        public static UniTask WaitUntilCanceled(CancellationToken cancellationToken, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            return new UniTask(WaitUntilCanceledPromise.Create(cancellationToken, timing, out var token), token);
        }

        public static UniTask<U> WaitUntilValueChanged<T, U>(T target, Func<T, U> monitorFunction, PlayerLoopTiming monitorTiming = PlayerLoopTiming.Update, IEqualityComparer<U> equalityComparer = null, CancellationToken cancellationToken = default(CancellationToken))
          where T : class
        {
            var unityObject = target as UnityEngine.Object;
            var isUnityObject = target is UnityEngine.Object; // don't use (unityObject == null)

            return new UniTask<U>(isUnityObject
                ? WaitUntilValueChangedUnityObjectPromise<T, U>.Create(target, monitorFunction, equalityComparer, monitorTiming, cancellationToken, out var token)
                : WaitUntilValueChangedStandardObjectPromise<T, U>.Create(target, monitorFunction, equalityComparer, monitorTiming, cancellationToken, out token), token);
        }

        private sealed class WaitUntilPromise : IUniTaskSource, IPlayerLoopItem, ITaskPoolNode<WaitUntilPromise>
        {
            private static TaskPool<WaitUntilPromise> pool;
            private WaitUntilPromise nextNode;
            public ref WaitUntilPromise NextNode => ref nextNode;

            static WaitUntilPromise()
            {
                TaskPool.RegisterSizeGetter(typeof(WaitUntilPromise), () => pool.Size);
            }

            private Func<bool> predicate;
            private CancellationToken cancellationToken;
            private UniTaskCompletionSourceCore<object> core;

            private WaitUntilPromise()
            {
            }

            public static IUniTaskSource Create(Func<bool> predicate, PlayerLoopTiming timing, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new WaitUntilPromise();
                }

                result.predicate = predicate;
                result.cancellationToken = cancellationToken;

                TaskTracker.TrackActiveTask(result, 3);

                PlayerLoopHelper.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            public void GetResult(short token)
            {
                try
                {
                    core.GetResult(token);
                }
                finally
                {
                    TryReturn();
                }
            }

            public UniTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public UniTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                try
                {
                    if (!predicate())
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    core.TrySetException(ex);
                    return false;
                }

                core.TrySetResult(null);
                return false;
            }

            private bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                predicate = default;
                cancellationToken = default;
                return pool.TryPush(this);
            }
        }

        private sealed class WaitWhilePromise : IUniTaskSource, IPlayerLoopItem, ITaskPoolNode<WaitWhilePromise>
        {
            private static TaskPool<WaitWhilePromise> pool;
            private WaitWhilePromise nextNode;
            public ref WaitWhilePromise NextNode => ref nextNode;

            static WaitWhilePromise()
            {
                TaskPool.RegisterSizeGetter(typeof(WaitWhilePromise), () => pool.Size);
            }

            private Func<bool> predicate;
            private CancellationToken cancellationToken;
            private UniTaskCompletionSourceCore<object> core;

            private WaitWhilePromise()
            {
            }

            public static IUniTaskSource Create(Func<bool> predicate, PlayerLoopTiming timing, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new WaitWhilePromise();
                }

                result.predicate = predicate;
                result.cancellationToken = cancellationToken;

                TaskTracker.TrackActiveTask(result, 3);

                PlayerLoopHelper.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            public void GetResult(short token)
            {
                try
                {
                    core.GetResult(token);
                }
                finally
                {
                    TryReturn();
                }
            }

            public UniTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public UniTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                try
                {
                    if (predicate())
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    core.TrySetException(ex);
                    return false;
                }

                core.TrySetResult(null);
                return false;
            }

            private bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                predicate = default;
                cancellationToken = default;
                return pool.TryPush(this);
            }
        }

        private sealed class WaitUntilCanceledPromise : IUniTaskSource, IPlayerLoopItem, ITaskPoolNode<WaitUntilCanceledPromise>
        {
            private static TaskPool<WaitUntilCanceledPromise> pool;
            private WaitUntilCanceledPromise nextNode;
            public ref WaitUntilCanceledPromise NextNode => ref nextNode;

            static WaitUntilCanceledPromise()
            {
                TaskPool.RegisterSizeGetter(typeof(WaitUntilCanceledPromise), () => pool.Size);
            }

            private CancellationToken cancellationToken;
            private UniTaskCompletionSourceCore<object> core;

            private WaitUntilCanceledPromise()
            {
            }

            public static IUniTaskSource Create(CancellationToken cancellationToken, PlayerLoopTiming timing, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new WaitUntilCanceledPromise();
                }

                result.cancellationToken = cancellationToken;

                TaskTracker.TrackActiveTask(result, 3);

                PlayerLoopHelper.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            public void GetResult(short token)
            {
                try
                {
                    core.GetResult(token);
                }
                finally
                {
                    TryReturn();
                }
            }

            public UniTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public UniTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    core.TrySetResult(null);
                    return false;
                }

                return true;
            }

            private bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                cancellationToken = default;
                return pool.TryPush(this);
            }
        }

        // where T : UnityEngine.Object, can not add constraint
        private sealed class WaitUntilValueChangedUnityObjectPromise<T, U> : IUniTaskSource<U>, IPlayerLoopItem, ITaskPoolNode<WaitUntilValueChangedUnityObjectPromise<T, U>>
        {
            private static TaskPool<WaitUntilValueChangedUnityObjectPromise<T, U>> pool;
            private WaitUntilValueChangedUnityObjectPromise<T, U> nextNode;
            public ref WaitUntilValueChangedUnityObjectPromise<T, U> NextNode => ref nextNode;

            static WaitUntilValueChangedUnityObjectPromise()
            {
                TaskPool.RegisterSizeGetter(typeof(WaitUntilValueChangedUnityObjectPromise<T, U>), () => pool.Size);
            }

            private T target;
            private UnityEngine.Object targetAsUnityObject;
            private U currentValue;
            private Func<T, U> monitorFunction;
            private IEqualityComparer<U> equalityComparer;
            private CancellationToken cancellationToken;
            private UniTaskCompletionSourceCore<U> core;

            private WaitUntilValueChangedUnityObjectPromise()
            {
            }

            public static IUniTaskSource<U> Create(T target, Func<T, U> monitorFunction, IEqualityComparer<U> equalityComparer, PlayerLoopTiming timing, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource<U>.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new WaitUntilValueChangedUnityObjectPromise<T, U>();
                }

                result.target = target;
                result.targetAsUnityObject = target as UnityEngine.Object;
                result.monitorFunction = monitorFunction;
                result.currentValue = monitorFunction(target);
                result.equalityComparer = equalityComparer ?? UnityEqualityComparer.GetDefault<U>();
                result.cancellationToken = cancellationToken;

                TaskTracker.TrackActiveTask(result, 3);

                PlayerLoopHelper.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            public U GetResult(short token)
            {
                try
                {
                    return core.GetResult(token);
                }
                finally
                {
                    TryReturn();
                }
            }

            void IUniTaskSource.GetResult(short token)
            {
                GetResult(token);
            }

            public UniTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public UniTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested || targetAsUnityObject == null) // destroyed = cancel.
                {
                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                U nextValue = default(U);
                try
                {
                    nextValue = monitorFunction(target);
                    if (equalityComparer.Equals(currentValue, nextValue))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    core.TrySetException(ex);
                    return false;
                }

                core.TrySetResult(nextValue);
                return false;
            }

            private bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                target = default;
                currentValue = default;
                monitorFunction = default;
                equalityComparer = default;
                cancellationToken = default;
                return pool.TryPush(this);
            }
        }

        private sealed class WaitUntilValueChangedStandardObjectPromise<T, U> : IUniTaskSource<U>, IPlayerLoopItem, ITaskPoolNode<WaitUntilValueChangedStandardObjectPromise<T, U>>
            where T : class
        {
            private static TaskPool<WaitUntilValueChangedStandardObjectPromise<T, U>> pool;
            private WaitUntilValueChangedStandardObjectPromise<T, U> nextNode;
            public ref WaitUntilValueChangedStandardObjectPromise<T, U> NextNode => ref nextNode;

            static WaitUntilValueChangedStandardObjectPromise()
            {
                TaskPool.RegisterSizeGetter(typeof(WaitUntilValueChangedStandardObjectPromise<T, U>), () => pool.Size);
            }

            private WeakReference<T> target;
            private U currentValue;
            private Func<T, U> monitorFunction;
            private IEqualityComparer<U> equalityComparer;
            private CancellationToken cancellationToken;
            private UniTaskCompletionSourceCore<U> core;

            private WaitUntilValueChangedStandardObjectPromise()
            {
            }

            public static IUniTaskSource<U> Create(T target, Func<T, U> monitorFunction, IEqualityComparer<U> equalityComparer, PlayerLoopTiming timing, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource<U>.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new WaitUntilValueChangedStandardObjectPromise<T, U>();
                }

                result.target = new WeakReference<T>(target, false); // wrap in WeakReference.
                result.monitorFunction = monitorFunction;
                result.currentValue = monitorFunction(target);
                result.equalityComparer = equalityComparer ?? UnityEqualityComparer.GetDefault<U>();
                result.cancellationToken = cancellationToken;

                TaskTracker.TrackActiveTask(result, 3);

                PlayerLoopHelper.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            public U GetResult(short token)
            {
                try
                {
                    return core.GetResult(token);
                }
                finally
                {
                    TryReturn();
                }
            }

            void IUniTaskSource.GetResult(short token)
            {
                GetResult(token);
            }

            public UniTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public UniTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested || !target.TryGetTarget(out var t)) // doesn't find = cancel.
                {
                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                U nextValue = default(U);
                try
                {
                    nextValue = monitorFunction(t);
                    if (equalityComparer.Equals(currentValue, nextValue))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    core.TrySetException(ex);
                    return false;
                }

                core.TrySetResult(nextValue);
                return false;
            }

            private bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                target = default;
                currentValue = default;
                monitorFunction = default;
                equalityComparer = default;
                cancellationToken = default;
                return pool.TryPush(this);
            }
        }
    }
}
