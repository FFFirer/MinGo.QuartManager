namespace MinGo.Qap.Shared.Models;

/// <summary>
/// 作业执行日志 DTO（Agent 上报用）
/// </summary>
public class ExecutionLogDto
{
    /// <summary>
    /// 作业 Key
    /// </summary>
    public JobKeyDto JobKey { get; set; }
    
    /// <summary>
    /// Scheduler 名称
    /// </summary>
    public string? SchedulerName { get; set; }
    
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTimeOffset StartTime { get; set; }
    
    /// <summary>
    /// 结束时间
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
    /// 自定义字段
    /// </summary>
    public Dictionary<string, object>? CustomFields { get; set; }
}

/// <summary>
/// 执行日志持久化查询返回 DTO
/// </summary>
public class ExecutionLogEntryDto
{
    public string Id { get; set; } = string.Empty;
    public string SchedulerName { get; set; } = string.Empty;
    public JobKeyDto JobKey { get; set; } = new();
    public string AgentId { get; set; } = string.Empty;
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public long? DurationMs { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? StackTrace { get; set; }
}