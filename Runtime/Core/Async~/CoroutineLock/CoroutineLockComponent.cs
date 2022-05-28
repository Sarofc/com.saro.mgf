using System.Collections.Generic;
using Saro.Collections;
using Saro.Tasks;

namespace Saro
{
    [FObjectSystem]
    public class CoroutineLockComponentAwakeSystem : AwakeSystem<CoroutineLockComponent, int>
    {
        public override void Awake(CoroutineLockComponent self, int capacity)
        {
            CoroutineLockComponent.Instance = self;
            self.list.Capacity = capacity;
            for (int i = 0; i < self.list.Capacity; ++i)
            {
                self.list.Add(FEntity.CreateWithId<CoroutineLockQueueType>(self, ++self.idGenerator));
            }
        }
    }

    [FObjectSystem]
    public class CoroutineLockComponentDestroySystem : DestroySystem<CoroutineLockComponent>
    {
        public override void Destroy(CoroutineLockComponent self)
        {
            self.list.Clear();
            self.nextFrameRun.Clear();
            self.timers.Clear();
            self.timeOutIds.Clear();
            self.timerOutTimer.Clear();
            self.idGenerator = 0;
            self.minTime = 0;
            CoroutineLockComponent.Instance = null;
        }
    }

    public class CoroutineLockComponentUpdateSystem : UpdateSystem<CoroutineLockComponent>
    {
        public override void Update(CoroutineLockComponent self)
        {
            // 检测超时的CoroutineLock
            TimeoutCheck(self);

            int count = self.nextFrameRun.Count;
            // 注意这里不能将this.nextFrameRun.Count 放到for循环中，因为循环过程中会有对象继续加入队列
            for (int i = 0; i < count; ++i)
            {
                (int coroutineLockType, long key) = self.nextFrameRun.Dequeue();
                self.Notify(coroutineLockType, key, 0);
            }
        }

        private void TimeoutCheck(CoroutineLockComponent self)
        {
            // 超时的锁
            if (self.timers.Count == 0)
            {
                return;
            }

            long timeNow = FGame.TimeInfo.ClientFrameTime();

            if (timeNow < self.minTime)
            {
                return;
            }

            foreach (KeyValuePair<long, List<CoroutineLockTimer>> kv in self.timers)
            {
                long k = kv.Key;
                if (k > timeNow)
                {
                    self.minTime = k;
                    break;
                }

                self.timeOutIds.Enqueue(k);
            }

            self.timerOutTimer.Clear();

            while (self.timeOutIds.Count > 0)
            {
                long time = self.timeOutIds.Dequeue();
                foreach (CoroutineLockTimer coroutineLockTimer in self.timers[time])
                {
                    self.timerOutTimer.Enqueue(coroutineLockTimer);
                }
                self.timers.Remove(time);
            }

            while (self.timerOutTimer.Count > 0)
            {
                CoroutineLockTimer coroutineLockTimer = self.timerOutTimer.Dequeue();
                if (coroutineLockTimer.CoroutineLockInstanceID != coroutineLockTimer.CoroutineLock.InstanceID)
                {
                    continue;
                }

                CoroutineLock coroutineLock = coroutineLockTimer.CoroutineLock;
                // 超时直接调用下一个锁
                self.NextFrameRun(coroutineLock.coroutineLockType, coroutineLock.key);
                coroutineLock.coroutineLockType = -1; // 上面调用了下一个, dispose不再调用
            }
        }
    }

    public static class CoroutineLockComponentSystem
    {
        public static void NextFrameRun(this CoroutineLockComponent self, int coroutineLockType, long key)
        {
            self.nextFrameRun.Enqueue((coroutineLockType, key));
        }

        public static void AddTimer(this CoroutineLockComponent self, long tillTime, CoroutineLock coroutineLock)
        {
            self.timers.Add(tillTime, new CoroutineLockTimer(coroutineLock));
            if (tillTime < self.minTime)
            {
                self.minTime = tillTime;
            }
        }

        public static async FTask<CoroutineLock> Wait(this CoroutineLockComponent self, int coroutineLockType, long key, int time = 60000)
        {
            CoroutineLockQueueType coroutineLockQueueType = self.list[(int)coroutineLockType];

            if (!coroutineLockQueueType.TryGetValue(key, out CoroutineLockQueue queue))
            {
                coroutineLockQueueType.Add(key, FEntity.CreateWithId<CoroutineLockQueue>(self, ++self.idGenerator, true));
                return self.CreateCoroutineLock(coroutineLockType, key, time, 1);
            }

            FTask<CoroutineLock> tcs = FTask<CoroutineLock>.Create(true);
            queue.Add(tcs, time);

            return await tcs;
        }

        private static CoroutineLock CreateCoroutineLock(this CoroutineLockComponent self, int coroutineLockType, long key, int time, int count)
        {
            CoroutineLock coroutineLock = FEntity.CreateWithId<CoroutineLock, int, long, int>(self, ++self.idGenerator, coroutineLockType, key, count, true);
            if (time > 0)
            {
                self.AddTimer(FGame.TimeInfo.ClientFrameTime() + time, coroutineLock);
            }
            return coroutineLock;
        }

        public static void Notify(this CoroutineLockComponent self, int coroutineLockType, long key, int count)
        {
            CoroutineLockQueueType coroutineLockQueueType = self.list[(int)coroutineLockType];
            if (!coroutineLockQueueType.TryGetValue(key, out CoroutineLockQueue queue))
            {
                return;
            }

            if (queue.Count == 0)
            {
                coroutineLockQueueType.Remove(key);
                return;
            }

#if NOT_UNITY // TODO 统一宏
            const int frameCoroutineCount = 5;
#else
            const int frameCoroutineCount = 10;
#endif

            if (count > frameCoroutineCount)
            {
                self.NextFrameRun(coroutineLockType, key);
                return;
            }

            CoroutineLockInfo coroutineLockInfo = queue.Dequeue();
            coroutineLockInfo.Tcs.SetResult(self.CreateCoroutineLock(coroutineLockType, key, coroutineLockInfo.Time, count));
        }
    }

    public class CoroutineLockComponent : FEntity
    {
        public static CoroutineLockComponent Instance { get; internal set; }

        //public List<CoroutineLockQueueType> list = new List<CoroutineLockQueueType>((int) CoroutineLockType.Max);
        public List<CoroutineLockQueueType> list = new List<CoroutineLockQueueType>();
        public Queue<(int, long)> nextFrameRun = new Queue<(int, long)>();
        public TSortedMultiMap<long, CoroutineLockTimer> timers = new TSortedMultiMap<long, CoroutineLockTimer>();
        public Queue<long> timeOutIds = new Queue<long>();
        public Queue<CoroutineLockTimer> timerOutTimer = new Queue<CoroutineLockTimer>();
        public long idGenerator;
        public long minTime;
    }
}