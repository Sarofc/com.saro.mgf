using System;
using System.Collections.Generic;

namespace Saro.Tasks
{
    public class FCancellationToken
    {
        private HashSet<Action> actions = new HashSet<Action>();
        public bool IsCancellationRequested { get; private set; }

        public void Add(Action callback)
        {
            // 如果action是null，绝对不能添加,要抛异常，说明有协程泄漏
            this.actions.Add(callback);
        }

        public void Remove(Action callback)
        {
            this.actions?.Remove(callback);
        }

        public void Cancel()
        {
            IsCancellationRequested = true;

            if (this.actions == null)
            {
                return;
            }

            if (this.actions.Count == 0)
            {
                return;
            }

            this.Invoke();
        }

        private void Invoke()
        {
            HashSet<Action> runActions = this.actions;
            this.actions = null;
            try
            {
                foreach (Action action in runActions)
                {
                    action.Invoke();
                }
            }
            catch (Exception e)
            {
                Log.ERROR($"{nameof(FCancellationToken)}", e.ToString());
            }
        }

        public async FVoid CancelAfter(long afterTimeCancel)
        {
            IsCancellationRequested = true;

            if (this.actions == null)
            {
                return;
            }

            if (this.actions.Count == 0)
            {
                return;
            }

            await Saro.FGame.Resolve<Saro.TimerComponent>().WaitAsync(afterTimeCancel);

            this.Invoke();
        }
    }
}