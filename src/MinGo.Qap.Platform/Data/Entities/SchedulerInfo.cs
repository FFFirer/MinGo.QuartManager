namespace MinGo.Qap.Platform.Data.Entities;

/// <summary>
/// Scheduler 运行时信息实体
/// </summary>
public class SchedulerInfo
{
    /// <summary>
    /// Scheduler 信息 ID（sch-xxx）
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Scheduler 名称
    /// </summary>
    public string SchedulerName { get; set; } = string.Empty;

    /// <summary>
    /// Scheduler 实例 ID（Quartz 分配）
    /// </summary>
    public string? SchedulerInstanceId { get; set; }

    /// <summary>
    /// 状态: running/standby/unknown
    /// </summary>
    public string Status { get; set; } = "unknown";

    /// <summary>
    /// 是否集群模式
    /// </summary>
    public bool IsClustered { get; set; }

    /// <summary>
    /// JobStore 类型
    /// </summary>
    public string? JobStoreType { get; set; }

    /// <summary>
    /// 线程池类型
    /// </summary>
    public string? ThreadPoolType { get; set; }

    /// <summary>
    /// 线程池大小
    /// </summary>
    public int ThreadPoolSize { get; set; }

    /// <summary>
    /// 开始运行时间（Agent 上报时转 UTC）
    /// </summary>
    public DateTimeOffset? RunningSince { get; set; }

    /// <summary>
    /// Quartz 版本
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// 已执行 Job 数量
    /// </summary>
    public int NumberOfJobsExecuted { get; set; }

    /// <summary>
    /// 扩展属性（JSON）
    /// </summary>
    public string? PropertiesJson { get; set; }

    // 时间字段（DateTimeOffset + UTC）

    /// <summary>
    /// 首次上报时间（DateTimeOffset.UtcNow）
    /// </summary>
    public DateTimeOffset FirstReportedAt { get; set; }

    /// <summary>
    /// 最后上报时间（DateTimeOffset.UtcNow）
    /// </summary>
    public DateTimeOffset LastReportedAt { get; set; }

    // Navigation

    /// <summary>
    /// 关联的 Agents
    /// </summary>
    public List<AgentScheduler> AgentSchedulers { get; set; } = new();
}
