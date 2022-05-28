using System.Collections.Generic;
using Saro.Tasks;

namespace Saro
{
    [FObjectSystem]
    public class CoroutineLockQueueAwakeSystem: AwakeSystem<CoroutineLockQueue>
    {
        public override void Awake(CoroutineLockQueue self)
        {
            self.queue.Clear();
        }
    }

    [FObjectSystem]
    public class CoroutineLockQueueDestroySystem: DestroySystem<CoroutineLockQueue>
    {
        public override void Destroy(CoroutineLockQueue self)
        {
            self.queue.Clear();
        }
    }

    public struct CoroutineLockInfo
    {
        public FTask<CoroutineLock> Tcs;
        public int Time;
    }
    
    public class CoroutineLockQueue: FEntity
    {
        public Queue<CoroutineLockInfo> queue = new Queue<CoroutineLockInfo>();

        public void Add(FTask<CoroutineLock> tcs, int time)
        {
            queue.Enqueue(new CoroutineLockInfo(){Tcs = tcs, Time = time});
        }

        public int Count
        {
            get
            {
                return queue.Count;
            }
        }

        public CoroutineLockInfo Dequeue()
        {
            return queue.Dequeue();
        }
    }
}