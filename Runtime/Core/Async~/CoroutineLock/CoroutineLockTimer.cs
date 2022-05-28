namespace Saro
{
    public struct CoroutineLockTimer
    {
        public CoroutineLock CoroutineLock;
        public long CoroutineLockInstanceID;

        public CoroutineLockTimer(CoroutineLock coroutineLock)
        {
            this.CoroutineLock = coroutineLock;
            this.CoroutineLockInstanceID = coroutineLock.InstanceID;
        }
    }
}