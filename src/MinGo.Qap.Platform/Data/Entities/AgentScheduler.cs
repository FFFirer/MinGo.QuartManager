namespace MinGo.Qap.Platform.Data.Entities;

/// <summary>
/// Agent-Scheduler 多对多关联实体
/// </summary>
public class AgentScheduler
{
    /// <summary>
    /// Agent ID
    /// </summary>
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// SchedulerInfo ID
    /// </summary>
    public string SchedulerInfoId { get; set; } = string.Empty;

    /// <summary>
    /// 上报时间（DateTimeOffset.UtcNow）
    /// </summary>
    public DateTimeOffset ReportedAt { get; set; }

    // Navigation

    /// <summary>
    /// 关联的 Agent
    /// </summary>
    public Agent Agent { get; set; } = null!;

    /// <summary>
    /// 关联的 Scheduler
    /// </summary>
    public SchedulerInfo SchedulerInfo { get; set; } = null!;
}
