namespace MinGo.Qap.Shared.Models;

/// <summary>
/// Job 调度配置
/// </summary>
public class ScheduleDto
{
    /// <summary>
    /// 调度类型: once, cron, interval
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Cron 表达式（Type=cron 时使用）
    /// </summary>
    public string? CronExpression { get; set; }
    
    /// <summary>
    /// 间隔秒数（Type=interval 时使用）
    /// </summary>
    public int? IntervalSeconds { get; set; }
    
    /// <summary>
    /// 执行时间（Type=once 时使用）
    /// </summary>
    public DateTime? RunAt { get; set; }
}

/// <summary>
/// Quartz 选项
/// </summary>
public class QuartzOptionsDto
{
    /// <summary>
    /// 禁止并发执行
    /// </summary>
    public bool DisallowConcurrentExecution { get; set; } = false;
    
    /// <summary>
    /// Misfire 策略
    /// </summary>
    public string MisfirePolicy { get; set; } = "FireAndProceed";
}

/// <summary>
/// 创建 Job 请求
/// </summary>
public class CreateJobRequest
{
    /// <summary>
    /// Job 唯一标识（Name + Group）
    /// </summary>
    public string JobKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Job 类型（来自 Manifest）
    /// </summary>
    public string JobType { get; set; } = string.Empty;
    
    /// <summary>
    /// Job 参数
    /// </summary>
    public Dictionary<string, object> Params { get; set; } = new();
    
    /// <summary>
    /// 调度配置
    /// </summary>
    public ScheduleDto Schedule { get; set; } = new();
    
    /// <summary>
    /// Quartz 选项
    /// </summary>
    public QuartzOptionsDto Options { get; set; } = new();
}

/// <summary>
/// 更新 Job 请求
/// </summary>
public class UpdateJobRequest
{
    public Dictionary<string, object>? Params { get; set; }
    public ScheduleDto? Schedule { get; set; }
    public QuartzOptionsDto? Options { get; set; }
}

/// <summary>
/// Job 定义 DTO
/// </summary>
public class JobDefinitionDto
{
    public string Id { get; set; } = string.Empty;
    public string ClusterId { get; set; } = string.Empty;
    public string JobKey { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public string Params { get; set; } = "{}";
    public string Schedule { get; set; } = "{}";
    public string Options { get; set; } = "{}";
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Job 摘要（列表展示用）
/// </summary>
public class JobSummaryDto
{
    public string JobKey { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ScheduleType { get; set; } = string.Empty;
    public string? CronExpression { get; set; }
    public DateTime? NextFireTime { get; set; }
    public DateTime? PreviousFireTime { get; set; }
}

/// <summary>
/// Job 详情 DTO（从 Quartz 实时获取）
/// </summary>
public class JobDetailDto
{
    public string JobKey { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ScheduleDto Schedule { get; set; } = new();
    public QuartzOptionsDto Options { get; set; } = new();
    public Dictionary<string, object> Params { get; set; } = new();
    public DateTime? NextFireTime { get; set; }
    public DateTime? PreviousFireTime { get; set; }
}
