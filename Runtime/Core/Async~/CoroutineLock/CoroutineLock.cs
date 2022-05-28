using System;

namespace Saro
{
    [FObjectSystem]
    public class CoroutineLockAwakeSystem : AwakeSystem<CoroutineLock, int, long, int>
    {
        public override void Awake(CoroutineLock self, int type, long k, int count)
        {
            self.coroutineLockType = type;
            self.key = k;
            self.count = count;
        }
    }

    [FObjectSystem]
    public class CoroutineLockDestroySystem : DestroySystem<CoroutineLock>
    {
        public override void Destroy(CoroutineLock self)
        {
            if (self.coroutineLockType != -1)
            {
                CoroutineLockComponent.Instance.Notify(self.coroutineLockType, self.key, self.count + 1);
            }
            else
            {
                // CoroutineLockType.None说明协程锁超时了
                Log.ERROR($"coroutine lock timeout: {self.coroutineLockType} {self.key} {self.count}");
            }
            self.coroutineLockType = -1;
            self.key = 0;
            self.count = 0;
        }
    }

    public class CoroutineLock : FEntity
    {
        /// <summary>
        /// 锁类型
        /// <code>可自定义 枚举 传入</code>
        /// <code>-1为空，从0开始有效</code>
        /// </summary>
        public int coroutineLockType = -1;
        public long key;
        public int count;
    }
}