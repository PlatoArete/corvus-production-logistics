namespace CorvusProductionLogistics;

public readonly struct LogisticsNetworkSnapshot
{
    public LogisticsNetworkSnapshot(int conduitCount, int storageCount, int workTableCount)
    {
        ConduitCount = conduitCount;
        StorageCount = storageCount;
        WorkTableCount = workTableCount;
    }

    public int ConduitCount { get; }

    public int StorageCount { get; }

    public int WorkTableCount { get; }
}

