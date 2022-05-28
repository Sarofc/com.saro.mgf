using System.Collections.Generic;

namespace Saro.Tasks
{
    // TODO canceltoken
    public partial class FTask
    {
        private class CoroutineBlocker
        {
            private int m_Count;

            private List<FTask> m_Tcss = new List<FTask>();

            public CoroutineBlocker(int count)
            {
                m_Count = count;
            }

            public async FTask WaitAsync()
            {
                if (--m_Count < 0)
                {
                    return;
                }

                if (m_Count == 0)
                {
                    List<FTask> t = m_Tcss;
                    m_Tcss = null;
                    foreach (FTask ttcs in t)
                    {
                        ttcs.SetResult();
                    }

                    return;
                }

                FTask tcs = FTask.Create(true);
                m_Tcss.Add(tcs);
                await tcs;
            }

            public void Cancel()
            {
                m_Count = 1;
            }
        }

        public static async FTask WaitAny<T>(IList<FTask<T>> tasks, FCancellationToken cancellationToken = null)
        {
            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(2);
            foreach (FTask<T> task in tasks)
            {
                RunOneTask(task).Coroutine();
            }

            void CancelAction()
            {
                coroutineBlocker.Cancel();
            }

            try
            {
                cancellationToken?.Add(CancelAction);
            }
            finally
            {
                cancellationToken?.Remove(CancelAction);
            }

            await coroutineBlocker.WaitAsync();

            async FVoid RunOneTask(FTask<T> task)
            {
                await task;
                await coroutineBlocker.WaitAsync();
            }
        }

        public static async FTask WaitAny(IList<FTask> tasks, FCancellationToken cancellationToken = null)
        {
            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(2);
            foreach (FTask task in tasks)
            {
                RunOneTask(task).Coroutine();
            }

            void CancelAction()
            {
                coroutineBlocker.Cancel();
            }

            try
            {
                cancellationToken?.Add(CancelAction);
            }
            finally
            {
                cancellationToken?.Remove(CancelAction);
            }

            await coroutineBlocker.WaitAsync();

            async FVoid RunOneTask(FTask task)
            {
                await task;
                await coroutineBlocker.WaitAsync();
            }
        }

        // issue：
        // WaitAll 带返回值的 FTask，结束后，task的状态还是 pending 状态
        public static async FTask WaitAll<T>(IList<FTask<T>> tasks, FCancellationToken cancellationToken = null)
        {
            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(tasks.Count + 1);
            foreach (FTask<T> task in tasks)
            {
                RunOneTask(task).Coroutine();
            }

            void CancelAction()
            {
                coroutineBlocker.Cancel();
            }

            try
            {
                cancellationToken?.Add(CancelAction);
            }
            finally
            {
                cancellationToken?.Remove(CancelAction);
            }

            await coroutineBlocker.WaitAsync();

            async FVoid RunOneTask(FTask<T> task)
            {
                await task;
                await coroutineBlocker.WaitAsync();
            }
        }

        public static async FTask WaitAll(IList<FTask> tasks, FCancellationToken cancellationToken = null)
        {
            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(tasks.Count + 1);
            foreach (FTask task in tasks)
            {
                RunOneTask(task).Coroutine();
            }

            void CancelAction()
            {
                coroutineBlocker.Cancel();
            }

            try
            {
                cancellationToken?.Add(CancelAction);
            }
            finally
            {
                cancellationToken?.Remove(CancelAction);
            }

            await coroutineBlocker.WaitAsync();

            async FVoid RunOneTask(FTask task)
            {
                await task;
                await coroutineBlocker.WaitAsync();
            }
        }
    }

    public static class FTaskExtension
    {
        public static FTask AsFTask(this UnityEngine.AsyncOperation op)
        {
            var tcs = FTask.Create();
            op.completed += _op =>
            {
                tcs.SetResult();
            };
            return tcs;
        }
    }
}