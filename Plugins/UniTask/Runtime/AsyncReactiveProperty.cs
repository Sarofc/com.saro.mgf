using System;
using System.Threading;

namespace Cysharp.Threading.Tasks
{
    public interface IReadOnlyAsyncReactiveProperty<T> : IUniTaskAsyncEnumerable<T>
    {
        T Value { get; }
        IUniTaskAsyncEnumerable<T> WithoutCurrent();
        UniTask<T> WaitAsync(CancellationToken cancellationToken = default);
    }

    public interface IAsyncReactiveProperty<T> : IReadOnlyAsyncReactiveProperty<T>
    {
        new T Value { get; set; }
    }

    [Serializable]
    public class AsyncReactiveProperty<T> : IAsyncReactiveProperty<T>, IDisposable
    {
        private TriggerEvent<T> triggerEvent;

#if UNITY_2018_3_OR_NEWER
        [UnityEngine.SerializeField]
        private
#endif
        T latestValue;

        public T Value
        {
            get
            {
                return latestValue;
            }
            set
            {
                this.latestValue = value;
                triggerEvent.SetResult(value);
            }
        }

        public AsyncReactiveProperty(T value)
        {
            this.latestValue = value;
            this.triggerEvent = default;
        }

        public IUniTaskAsyncEnumerable<T> WithoutCurrent()
        {
            return new WithoutCurrentEnumerable(this);
        }

        public IUniTaskAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            return new Enumerator(this, cancellationToken, true);
        }

        public void Dispose()
        {
            triggerEvent.SetCompleted();
        }

        public static implicit operator T(AsyncReactiveProperty<T> value)
        {
            return value.Value;
        }

        public override string ToString()
        {
            if (isValueType) return latestValue.ToString();
            return latestValue?.ToString();
        }

        public UniTask<T> WaitAsync(CancellationToken cancellationToken = default)
        {
            return new UniTask<T>(WaitAsyncSource.Create(this, cancellationToken, out var token), token);
        }

        private static bool isValueType;

        static AsyncReactiveProperty()
        {
            isValueType = typeof(T).IsValueType;
        }

        private sealed class WaitAsyncSource : IUniTaskSource<T>, ITriggerHandler<T>, ITaskPoolNode<WaitAsyncSource>
        {
            private static Action<object> cancellationCallback = CancellationCallback;
            private static TaskPool<WaitAsyncSource> pool;
            private WaitAsyncSource nextNode;
            ref WaitAsyncSource ITaskPoolNode<WaitAsyncSource>.NextNode => ref nextNode;

            static WaitAsyncSource()
            {
                TaskPool.RegisterSizeGetter(typeof(WaitAsyncSource), () => pool.Size);
            }

            private AsyncReactiveProperty<T> parent;
            private CancellationToken cancellationToken;
            private CancellationTokenRegistration cancellationTokenRegistration;
            private UniTaskCompletionSourceCore<T> core;

            private WaitAsyncSource()
            {
            }

            public static IUniTaskSource<T> Create(AsyncReactiveProperty<T> parent, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource<T>.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new WaitAsyncSource();
                }

                result.parent = parent;
                result.cancellationToken = cancellationToken;

                if (cancellationToken.CanBeCanceled)
                {
                    result.cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(cancellationCallback, result);
                }

                result.parent.triggerEvent.Add(result);

                TaskTracker.TrackActiveTask(result, 3);

                token = result.core.Version;
                return result;
            }

            private bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                cancellationTokenRegistration.Dispose();
                cancellationTokenRegistration = default;
                parent.triggerEvent.Remove(this);
                parent = null;
                cancellationToken = default;
                return pool.TryPush(this);
            }

            private static void CancellationCallback(object state)
            {
                var self = (WaitAsyncSource)state;
                self.OnCanceled(self.cancellationToken);
            }

            // IUniTaskSource

            public T GetResult(short token)
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

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public UniTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public UniTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            // ITriggerHandler

            ITriggerHandler<T> ITriggerHandler<T>.Prev { get; set; }
            ITriggerHandler<T> ITriggerHandler<T>.Next { get; set; }

            public void OnCanceled(CancellationToken cancellationToken)
            {
                core.TrySetCanceled(cancellationToken);
            }

            public void OnCompleted()
            {
                // Complete as Cancel.
                core.TrySetCanceled(CancellationToken.None);
            }

            public void OnError(Exception ex)
            {
                core.TrySetException(ex);
            }

            public void OnNext(T value)
            {
                core.TrySetResult(value);
            }
        }

        private sealed class WithoutCurrentEnumerable : IUniTaskAsyncEnumerable<T>
        {
            private readonly AsyncReactiveProperty<T> parent;

            public WithoutCurrentEnumerable(AsyncReactiveProperty<T> parent)
            {
                this.parent = parent;
            }

            public IUniTaskAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new Enumerator(parent, cancellationToken, false);
            }
        }

        private sealed class Enumerator : MoveNextSource, IUniTaskAsyncEnumerator<T>, ITriggerHandler<T>
        {
            private static Action<object> cancellationCallback = CancellationCallback;
            private readonly AsyncReactiveProperty<T> parent;
            private readonly CancellationToken cancellationToken;
            private readonly CancellationTokenRegistration cancellationTokenRegistration;
            private T value;
            private bool isDisposed;
            private bool firstCall;

            public Enumerator(AsyncReactiveProperty<T> parent, CancellationToken cancellationToken, bool publishCurrentValue)
            {
                this.parent = parent;
                this.cancellationToken = cancellationToken;
                this.firstCall = publishCurrentValue;

                parent.triggerEvent.Add(this);
                TaskTracker.TrackActiveTask(this, 3);

                if (cancellationToken.CanBeCanceled)
                {
                    cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(cancellationCallback, this);
                }
            }

            public T Current => value;

            ITriggerHandler<T> ITriggerHandler<T>.Prev { get; set; }
            ITriggerHandler<T> ITriggerHandler<T>.Next { get; set; }

            public UniTask<bool> MoveNextAsync()
            {
                // raise latest value on first call.
                if (firstCall)
                {
                    firstCall = false;
                    value = parent.Value;
                    return CompletedTasks.True;
                }

                completionSource.Reset();
                return new UniTask<bool>(this, completionSource.Version);
            }

            public UniTask DisposeAsync()
            {
                if (!isDisposed)
                {
                    isDisposed = true;
                    TaskTracker.RemoveTracking(this);
                    completionSource.TrySetCanceled(cancellationToken);
                    parent.triggerEvent.Remove(this);
                }
                return default;
            }

            public void OnNext(T value)
            {
                this.value = value;
                completionSource.TrySetResult(true);
            }

            public void OnCanceled(CancellationToken cancellationToken)
            {
                DisposeAsync().Forget();
            }

            public void OnCompleted()
            {
                completionSource.TrySetResult(false);
            }

            public void OnError(Exception ex)
            {
                completionSource.TrySetException(ex);
            }

            private static void CancellationCallback(object state)
            {
                var self = (Enumerator)state;
                self.DisposeAsync().Forget();
            }
        }
    }

    public class ReadOnlyAsyncReactiveProperty<T> : IReadOnlyAsyncReactiveProperty<T>, IDisposable
    {
        private TriggerEvent<T> triggerEvent;
        private T latestValue;
        private IUniTaskAsyncEnumerator<T> enumerator;

        public T Value
        {
            get
            {
                return latestValue;
            }
        }

        public ReadOnlyAsyncReactiveProperty(T initialValue, IUniTaskAsyncEnumerable<T> source, CancellationToken cancellationToken)
        {
            latestValue = initialValue;
            ConsumeEnumerator(source, cancellationToken).Forget();
        }

        public ReadOnlyAsyncReactiveProperty(IUniTaskAsyncEnumerable<T> source, CancellationToken cancellationToken)
        {
            ConsumeEnumerator(source, cancellationToken).Forget();
        }

        private async UniTaskVoid ConsumeEnumerator(IUniTaskAsyncEnumerable<T> source, CancellationToken cancellationToken)
        {
            enumerator = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await enumerator.MoveNextAsync())
                {
                    var value = enumerator.Current;
                    this.latestValue = value;
                    triggerEvent.SetResult(value);
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
                enumerator = null;
            }
        }

        public IUniTaskAsyncEnumerable<T> WithoutCurrent()
        {
            return new WithoutCurrentEnumerable(this);
        }

        public IUniTaskAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            return new Enumerator(this, cancellationToken, true);
        }

        public void Dispose()
        {
            if (enumerator != null)
            {
                enumerator.DisposeAsync().Forget();
            }

            triggerEvent.SetCompleted();
        }

        public static implicit operator T(ReadOnlyAsyncReactiveProperty<T> value)
        {
            return value.Value;
        }

        public override string ToString()
        {
            if (isValueType) return latestValue.ToString();
            return latestValue?.ToString();
        }

        public UniTask<T> WaitAsync(CancellationToken cancellationToken = default)
        {
            return new UniTask<T>(WaitAsyncSource.Create(this, cancellationToken, out var token), token);
        }

        private static bool isValueType;

        static ReadOnlyAsyncReactiveProperty()
        {
            isValueType = typeof(T).IsValueType;
        }

        private sealed class WaitAsyncSource : IUniTaskSource<T>, ITriggerHandler<T>, ITaskPoolNode<WaitAsyncSource>
        {
            private static Action<object> cancellationCallback = CancellationCallback;
            private static TaskPool<WaitAsyncSource> pool;
            private WaitAsyncSource nextNode;
            ref WaitAsyncSource ITaskPoolNode<WaitAsyncSource>.NextNode => ref nextNode;

            static WaitAsyncSource()
            {
                TaskPool.RegisterSizeGetter(typeof(WaitAsyncSource), () => pool.Size);
            }

            private ReadOnlyAsyncReactiveProperty<T> parent;
            private CancellationToken cancellationToken;
            private CancellationTokenRegistration cancellationTokenRegistration;
            private UniTaskCompletionSourceCore<T> core;

            private WaitAsyncSource()
            {
            }

            public static IUniTaskSource<T> Create(ReadOnlyAsyncReactiveProperty<T> parent, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource<T>.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new WaitAsyncSource();
                }

                result.parent = parent;
                result.cancellationToken = cancellationToken;

                if (cancellationToken.CanBeCanceled)
                {
                    result.cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(cancellationCallback, result);
                }

                result.parent.triggerEvent.Add(result);

                TaskTracker.TrackActiveTask(result, 3);

                token = result.core.Version;
                return result;
            }

            private bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                cancellationTokenRegistration.Dispose();
                cancellationTokenRegistration = default;
                parent.triggerEvent.Remove(this);
                parent = null;
                cancellationToken = default;
                return pool.TryPush(this);
            }

            private static void CancellationCallback(object state)
            {
                var self = (WaitAsyncSource)state;
                self.OnCanceled(self.cancellationToken);
            }

            // IUniTaskSource

            public T GetResult(short token)
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

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public UniTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public UniTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            // ITriggerHandler

            ITriggerHandler<T> ITriggerHandler<T>.Prev { get; set; }
            ITriggerHandler<T> ITriggerHandler<T>.Next { get; set; }

            public void OnCanceled(CancellationToken cancellationToken)
            {
                core.TrySetCanceled(cancellationToken);
            }

            public void OnCompleted()
            {
                // Complete as Cancel.
                core.TrySetCanceled(CancellationToken.None);
            }

            public void OnError(Exception ex)
            {
                core.TrySetException(ex);
            }

            public void OnNext(T value)
            {
                core.TrySetResult(value);
            }
        }

        private sealed class WithoutCurrentEnumerable : IUniTaskAsyncEnumerable<T>
        {
            private readonly ReadOnlyAsyncReactiveProperty<T> parent;

            public WithoutCurrentEnumerable(ReadOnlyAsyncReactiveProperty<T> parent)
            {
                this.parent = parent;
            }

            public IUniTaskAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new Enumerator(parent, cancellationToken, false);
            }
        }

        private sealed class Enumerator : MoveNextSource, IUniTaskAsyncEnumerator<T>, ITriggerHandler<T>
        {
            private static Action<object> cancellationCallback = CancellationCallback;
            private readonly ReadOnlyAsyncReactiveProperty<T> parent;
            private readonly CancellationToken cancellationToken;
            private readonly CancellationTokenRegistration cancellationTokenRegistration;
            private T value;
            private bool isDisposed;
            private bool firstCall;

            public Enumerator(ReadOnlyAsyncReactiveProperty<T> parent, CancellationToken cancellationToken, bool publishCurrentValue)
            {
                this.parent = parent;
                this.cancellationToken = cancellationToken;
                this.firstCall = publishCurrentValue;

                parent.triggerEvent.Add(this);
                TaskTracker.TrackActiveTask(this, 3);

                if (cancellationToken.CanBeCanceled)
                {
                    cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(cancellationCallback, this);
                }
            }

            public T Current => value;
            ITriggerHandler<T> ITriggerHandler<T>.Prev { get; set; }
            ITriggerHandler<T> ITriggerHandler<T>.Next { get; set; }

            public UniTask<bool> MoveNextAsync()
            {
                // raise latest value on first call.
                if (firstCall)
                {
                    firstCall = false;
                    value = parent.Value;
                    return CompletedTasks.True;
                }

                completionSource.Reset();
                return new UniTask<bool>(this, completionSource.Version);
            }

            public UniTask DisposeAsync()
            {
                if (!isDisposed)
                {
                    isDisposed = true;
                    TaskTracker.RemoveTracking(this);
                    completionSource.TrySetCanceled(cancellationToken);
                    parent.triggerEvent.Remove(this);
                }
                return default;
            }

            public void OnNext(T value)
            {
                this.value = value;
                completionSource.TrySetResult(true);
            }

            public void OnCanceled(CancellationToken cancellationToken)
            {
                DisposeAsync().Forget();
            }

            public void OnCompleted()
            {
                completionSource.TrySetResult(false);
            }

            public void OnError(Exception ex)
            {
                completionSource.TrySetException(ex);
            }

            private static void CancellationCallback(object state)
            {
                var self = (Enumerator)state;
                self.DisposeAsync().Forget();
            }
        }
    }

    public static class StateExtensions
    {
        public static ReadOnlyAsyncReactiveProperty<T> ToReadOnlyAsyncReactiveProperty<T>(this IUniTaskAsyncEnumerable<T> source, CancellationToken cancellationToken)
        {
            return new ReadOnlyAsyncReactiveProperty<T>(source, cancellationToken);
        }

        public static ReadOnlyAsyncReactiveProperty<T> ToReadOnlyAsyncReactiveProperty<T>(this IUniTaskAsyncEnumerable<T> source, T initialValue, CancellationToken cancellationToken)
        {
            return new ReadOnlyAsyncReactiveProperty<T>(initialValue, source, cancellationToken);
        }
    }
}