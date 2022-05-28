#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Threading;

namespace Cysharp.Threading.Tasks
{
    public class AsyncLazy
    {
        private static Action<object> continuation = SetCompletionSource;
        private Func<UniTask> taskFactory;
        private UniTaskCompletionSource completionSource;
        private UniTask.Awaiter awaiter;
        private object syncLock;
        private bool initialized;

        public AsyncLazy(Func<UniTask> taskFactory)
        {
            this.taskFactory = taskFactory;
            this.completionSource = new UniTaskCompletionSource();
            this.syncLock = new object();
            this.initialized = false;
        }

        internal AsyncLazy(UniTask task)
        {
            this.taskFactory = null;
            this.completionSource = new UniTaskCompletionSource();
            this.syncLock = null;
            this.initialized = true;

            var awaiter = task.GetAwaiter();
            if (awaiter.IsCompleted)
            {
                SetCompletionSource(awaiter);
            }
            else
            {
                this.awaiter = awaiter;
                awaiter.SourceOnCompleted(continuation, this);
            }
        }

        public UniTask Task
        {
            get
            {
                EnsureInitialized();
                return completionSource.Task;
            }
        }


        public UniTask.Awaiter GetAwaiter() => Task.GetAwaiter();

        private void EnsureInitialized()
        {
            if (Volatile.Read(ref initialized))
            {
                return;
            }

            EnsureInitializedCore();
        }

        private void EnsureInitializedCore()
        {
            lock (syncLock)
            {
                if (!Volatile.Read(ref initialized))
                {
                    var f = Interlocked.Exchange(ref taskFactory, null);
                    if (f != null)
                    {
                        var task = f();
                        var awaiter = task.GetAwaiter();
                        if (awaiter.IsCompleted)
                        {
                            SetCompletionSource(awaiter);
                        }
                        else
                        {
                            this.awaiter = awaiter;
                            awaiter.SourceOnCompleted(continuation, this);
                        }

                        Volatile.Write(ref initialized, true);
                    }
                }
            }
        }

        private void SetCompletionSource(in UniTask.Awaiter awaiter)
        {
            try
            {
                awaiter.GetResult();
                completionSource.TrySetResult();
            }
            catch (Exception ex)
            {
                completionSource.TrySetException(ex);
            }
        }

        private static void SetCompletionSource(object state)
        {
            var self = (AsyncLazy)state;
            try
            {
                self.awaiter.GetResult();
                self.completionSource.TrySetResult();
            }
            catch (Exception ex)
            {
                self.completionSource.TrySetException(ex);
            }
            finally
            {
                self.awaiter = default;
            }
        }
    }

    public class AsyncLazy<T>
    {
        private static Action<object> continuation = SetCompletionSource;
        private Func<UniTask<T>> taskFactory;
        private UniTaskCompletionSource<T> completionSource;
        private UniTask<T>.Awaiter awaiter;
        private object syncLock;
        private bool initialized;

        public AsyncLazy(Func<UniTask<T>> taskFactory)
        {
            this.taskFactory = taskFactory;
            this.completionSource = new UniTaskCompletionSource<T>();
            this.syncLock = new object();
            this.initialized = false;
        }

        internal AsyncLazy(UniTask<T> task)
        {
            this.taskFactory = null;
            this.completionSource = new UniTaskCompletionSource<T>();
            this.syncLock = null;
            this.initialized = true;

            var awaiter = task.GetAwaiter();
            if (awaiter.IsCompleted)
            {
                SetCompletionSource(awaiter);
            }
            else
            {
                this.awaiter = awaiter;
                awaiter.SourceOnCompleted(continuation, this);
            }
        }

        public UniTask<T> Task
        {
            get
            {
                EnsureInitialized();
                return completionSource.Task;
            }
        }


        public UniTask<T>.Awaiter GetAwaiter() => Task.GetAwaiter();

        private void EnsureInitialized()
        {
            if (Volatile.Read(ref initialized))
            {
                return;
            }

            EnsureInitializedCore();
        }

        private void EnsureInitializedCore()
        {
            lock (syncLock)
            {
                if (!Volatile.Read(ref initialized))
                {
                    var f = Interlocked.Exchange(ref taskFactory, null);
                    if (f != null)
                    {
                        var task = f();
                        var awaiter = task.GetAwaiter();
                        if (awaiter.IsCompleted)
                        {
                            SetCompletionSource(awaiter);
                        }
                        else
                        {
                            this.awaiter = awaiter;
                            awaiter.SourceOnCompleted(continuation, this);
                        }

                        Volatile.Write(ref initialized, true);
                    }
                }
            }
        }

        private void SetCompletionSource(in UniTask<T>.Awaiter awaiter)
        {
            try
            {
                var result = awaiter.GetResult();
                completionSource.TrySetResult(result);
            }
            catch (Exception ex)
            {
                completionSource.TrySetException(ex);
            }
        }

        private static void SetCompletionSource(object state)
        {
            var self = (AsyncLazy<T>)state;
            try
            {
                var result = self.awaiter.GetResult();
                self.completionSource.TrySetResult(result);
            }
            catch (Exception ex)
            {
                self.completionSource.TrySetException(ex);
            }
            finally
            {
                self.awaiter = default;
            }
        }
    }
}
