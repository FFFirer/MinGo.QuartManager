namespace MinGo.Qap.Shared.Models;

/// <summary>
/// 作业执行日志 DTO
/// </summary>
public class ExecutionLogDto
{
    /// <summary>
    /// 作业 Key
    /// </summary>
    public string JobKey { get; set; } = string.Empty;
    
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