namespace MinGo.Qap.Platform.Data.Entities;

/// <summary>
/// 作业执行日志实体（Agent 上报后持久化）
/// </summary>
public class ExecutionLog
{
    /// <summary>
    /// ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Scheduler 名称
    /// </summary>
    public string SchedulerName { get; set; } = string.Empty;

    /// <summary>
    /// Job Name
    /// </summary>
    public string JobName { get; set; } = string.Empty;

    /// <summary>
    /// Job Group
    /// </summary>
    public string JobGroup { get; set; } = "DEFAULT";

    /// <summary>
    /// 上报 Agent ID
    /// </summary>
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// 执行开始时间
    /// </summary>
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// 执行结束时间
    /// </summary>
    public DateTimeOffset? EndTime { get; set; }

    /// <summary>
    /// 执行时长（毫秒）
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 堆栈跟踪
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// 记录创建时间（Platform 接收时间）
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
