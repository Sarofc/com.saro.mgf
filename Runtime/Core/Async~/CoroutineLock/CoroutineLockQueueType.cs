using System;
using System.Collections.Generic;

namespace Saro
{
    [FObjectSystem]
    public class CoroutineLockQueueTypeAwakeSystem: AwakeSystem<CoroutineLockQueueType>
    {
        public override void Awake(CoroutineLockQueueType self)
        {
            self.dictionary.Clear();
        }
    }

    [FObjectSystem]
    public class CoroutineLockQueueTypeDestroySystem: DestroySystem<CoroutineLockQueueType>
    {
        public override void Destroy(CoroutineLockQueueType self)
        {
            self.dictionary.Clear();
        }
    }
    
    public class CoroutineLockQueueType: FEntity
    {
        public Dictionary<long, CoroutineLockQueue> dictionary = new Dictionary<long, CoroutineLockQueue>();

        public bool TryGetValue(long key, out CoroutineLockQueue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        public void Remove(long key)
        {
            if (dictionary.TryGetValue(key, out CoroutineLockQueue value))
            {
                value.Dispose();
            }
            dictionary.Remove(key);
        }
        
        public void Add(long key, CoroutineLockQueue value)
        {
            dictionary.Add(key, value);
        }
    }
}