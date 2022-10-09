using System;

public struct SharedPoolInfo
{
    public Type Type { get; private set; }
    public int UnusedCount { get; private set; }
    public int UsingCount { get; private set; }
    public int RentCount { get; private set; }
    public int ReturnCount { get; private set; }
    public int AddCount { get; private set; }
    public int RemoveCount { get; private set; }

    public SharedPoolInfo(Type type, int unusedReferenceCount, int usingReferenceCount, int acquireReferenceCount, int releaseReferenceCount, int addReferenceCount, int removeReferenceCount)
    {
        Type = type;
        UnusedCount = unusedReferenceCount;
        UsingCount = usingReferenceCount;
        RentCount = acquireReferenceCount;
        ReturnCount = releaseReferenceCount;
        AddCount = addReferenceCount;
        RemoveCount = removeReferenceCount;
    }
}
