using System;

public struct SharedPoolInfo
{
    public Type Type { get; private set; }
    public int UnusedCount { get; private set; }
    public int UsingCount { get; private set; }
    public int AcquireCount { get; private set; }
    public int ReleaseCount { get; private set; }
    public int AddCount { get; private set; }
    public int RemoveCount { get; private set; }

    public SharedPoolInfo(Type type, int unusedReferenceCount, int usingReferenceCount, int acquireReferenceCount, int releaseReferenceCount, int addReferenceCount, int removeReferenceCount)
    {
        Type = type;
        UnusedCount = unusedReferenceCount;
        UsingCount = usingReferenceCount;
        AcquireCount = acquireReferenceCount;
        ReleaseCount = releaseReferenceCount;
        AddCount = addReferenceCount;
        RemoveCount = removeReferenceCount;
    }
}
