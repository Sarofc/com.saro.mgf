using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Internal;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Saro.Core
{
    public static class IAssetHandleAsyncExtension
    {
        #region IAssetHandle

        public static IAssetHandleAwaiter GetAwaiter(this IAssetHandle handle)
        {
            Error.ThrowArgumentNullException(handle, nameof(handle));
            return new IAssetHandleAwaiter(handle);
        }

        public static UniTask<UnityEngine.Object> WithCancellation(this IAssetHandle handle, CancellationToken cancellationToken)
        {
            return ToUniTask(handle, cancellationToken: cancellationToken);
        }

        public static UniTask<UnityEngine.Object> ToUniTask(this IAssetHandle handle, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default(CancellationToken))
        {
            Error.ThrowArgumentNullException(handle, nameof(handle));
            if (cancellationToken.IsCancellationRequested) return UniTask.FromCanceled<UnityEngine.Object>(cancellationToken);
            if (handle.IsDone) return UniTask.FromResult(handle.Asset);
            return new UniTask<UnityEngine.Object>(IAssetHandleConfiguredSource.Create(handle, timing, progress, cancellationToken, out var token), token);
        }

        public struct IAssetHandleAwaiter : ICriticalNotifyCompletion
        {
            private IAssetHandle handle;
            private Action<IAssetHandle> continuationAction;

            public IAssetHandleAwaiter(IAssetHandle handle)
            {
                this.handle = handle;
                continuationAction = null;
            }

            public bool IsCompleted => handle.IsDone;

            public UnityEngine.Object GetResult()
            {
                if (continuationAction != null)
                {
                    handle.Completed -= continuationAction;
                    continuationAction = null;
                    var result = handle.Asset;
                    handle = null;
                    return result;
                }
                else
                {
                    var result = handle.Asset;
                    handle = null;
                    return result;
                }
            }

            public void OnCompleted(Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(continuationAction);
                continuationAction = PooledDelegate<IAssetHandle>.Create(continuation);
                handle.Completed += continuationAction;
            }
        }

        private sealed class IAssetHandleConfiguredSource : IUniTaskSource<UnityEngine.Object>, IPlayerLoopItem, ITaskPoolNode<IAssetHandleConfiguredSource>
        {
            private static TaskPool<IAssetHandleConfiguredSource> pool;
            private IAssetHandleConfiguredSource nextNode;
            public ref IAssetHandleConfiguredSource NextNode => ref nextNode;

            static IAssetHandleConfiguredSource()
            {
                TaskPool.RegisterSizeGetter(typeof(IAssetHandleConfiguredSource), () => pool.Size);
            }

            private IAssetHandle handle;
            private IProgress<float> progress;
            private CancellationToken cancellationToken;
            private UniTaskCompletionSourceCore<UnityEngine.Object> core;

            private IAssetHandleConfiguredSource()
            { }

            public static IUniTaskSource<UnityEngine.Object> Create(IAssetHandle handle, PlayerLoopTiming timing, IProgress<float> progress, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource<UnityEngine.Object>.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new IAssetHandleConfiguredSource();
                }

                result.handle = handle;
                result.progress = progress;
                result.cancellationToken = cancellationToken;

                TaskTracker.TrackActiveTask(result, 3);

                PlayerLoopHelper.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            public UnityEngine.Object GetResult(short token)
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
                if (cancellationToken.IsCancellationRequested)
                {
                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                if (progress != null)
                {
                    progress.Report(handle.Progress);
                }

                if (handle.IsDone)
                {
                    core.TrySetResult(handle.Asset);
                    return false;
                }

                return true;
            }

            private bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                handle = default;
                progress = default;
                cancellationToken = default;
                return pool.TryPush(this);
            }
        }

        #endregion
    }
}
