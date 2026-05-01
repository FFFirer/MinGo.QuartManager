namespace MinGo.Qap.Shared.Enums;

/// <summary>
/// Cluster 状态（保留用于迁移兼容）
/// </summary>
public enum ClusterStatus
{
    Pending,
    Online,
    Warning,
    Offline,
    Deleted
}

/// <summary>
/// JobDefinition 同步状态（保留用于迁移兼容）
/// </summary>
public enum SyncStatus
{
    Pending,
    Synced,
    Failed,
    Timeout
}


