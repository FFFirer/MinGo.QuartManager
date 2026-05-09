using System.ComponentModel.DataAnnotations;

namespace MinGo.Qap.Shared.Models;

/// <summary>
/// Job 调度配置
/// </summary>
public class ScheduleDto
{
    /// <summary>
    /// 调度类型: once, cron, interval
    /// </summary>
    [Required(ErrorMessage = "调度类型不能为空")]
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Cron 表达式（Type=cron 时使用）
    /// </summary>
    public string? CronExpression { get; set; }
    
    /// <summary>
    /// 间隔秒数（Type=interval 时使用）
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "间隔秒数必须大于 0")]
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
    /// 持久化 Job：即使没有 Trigger 也保留在 Scheduler 中
    /// </summary>
    public bool StoreDurable { get; set; } = false;

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
    [Required(ErrorMessage = "Job Key 不能为空")]
    public JobKeyDto JobKey { get; set; }
    
    /// <summary>
    /// Job 类型的结构化限定名
    /// </summary>
    [Required(ErrorMessage = "Job 类型不能为空")]
    public JobTypeQualifiedName JobType { get; set; } = new();
    
    /// <summary>
    /// Job 参数
    /// </summary>
    public Dictionary<string, object> Params { get; set; } = new();
    
    /// <summary>
    /// 调度配置
    /// </summary>
    [Required(ErrorMessage = "调度配置不能为空")]
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
    public string SchedulerName { get; set; } = string.Empty;
    public JobKeyDto JobKey { get; set; }
    public JobTypeQualifiedName JobType { get; set; } = new();
    public string Params { get; set; } = "{}";
    public string Schedule { get; set; } = "{}";
    public string Options { get; set; } = "{}";
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>关联的 Trigger 列表（Agent 实时数据）</summary>
    public List<TriggerSummaryDto>? Triggers { get; set; }
}

/// <summary>
/// Job 摘要（列表展示用）
/// </summary>
public class JobSummaryDto
{
    public JobKeyDto JobKey { get; set; }
    public JobTypeQualifiedName JobType { get; set; } = new();
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
    public JobKeyDto JobKey { get; set; }
    public JobTypeQualifiedName JobType { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ScheduleDto Schedule { get; set; } = new();
    public QuartzOptionsDto Options { get; set; } = new();
    public Dictionary<string, object> Params { get; set; } = new();
    public DateTime? NextFireTime { get; set; }
    public DateTime? PreviousFireTime { get; set; }

    /// <summary>关联的 Trigger 列表</summary>
    public List<TriggerSummaryDto> Triggers { get; set; } = new();
}
