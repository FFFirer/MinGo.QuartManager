namespace MinGo.Qap.Shared.Enums;

/// <summary>
/// Cluster 状态
/// </summary>
public enum ClusterStatus
{
    /// <summary>
    /// 刚注册，等待首次心跳
    /// </summary>
    Pending,
    
    /// <summary>
    /// 正常在线
    /// </summary>
    Online,
    
    /// <summary>
    /// 心跳超时（Warning）
    /// </summary>
    Warning,
    
    /// <summary>
    /// 已离线
    /// </summary>
    Offline,
    
    /// <summary>
    /// 已删除（软删除）
    /// </summary>
    Deleted
}

/// <summary>
/// JobDefinition 同步状态
/// </summary>
public enum SyncStatus
{
    /// <summary>
    /// 等待同步到 Agent
    /// </summary>
    Pending,
    
    /// <summary>
    /// 已同步
    /// </summary>
    Synced,
    
    /// <summary>
    /// 同步失败
    /// </summary>
    Failed,
    
    /// <summary>
    /// 同步超时
    /// </summary>
    Timeout
}

/// <summary>
/// 调度类型
/// </summary>
public enum ScheduleType
{
    /// <summary>
    /// 执行一次
    /// </summary>
    Once,
    
    /// <summary>
    /// Cron 表达式
    /// </summary>
    Cron,
    
    /// <summary>
    /// 间隔执行
    /// </summary>
    Interval
}

/// <summary>
/// Misfire 策略
/// </summary>
public enum MisfirePolicy
{
    /// <summary>
    /// 立即执行并继续调度
    /// </summary>
    FireAndProceed,
    
    /// <summary>
    /// 忽略 Misfire，保持原有调度
    /// </summary>
    IgnoreMisfire,
    
    /// <summary>
    /// 什么都不做
    /// </summary>
    DoNothing
}
