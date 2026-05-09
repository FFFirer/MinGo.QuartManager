using MinGo.Qap.Shared.Enums;

namespace MinGo.Qap.Platform.Data.Entities;

/// <summary>
/// JobDefinition 实体（声明式创建记录）
/// </summary>
public class JobDefinition
{
    /// <summary>
    /// ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// 目标 Scheduler 名称
    /// </summary>
    public string SchedulerName { get; set; } = string.Empty;
    
    /// <summary>
    /// Job Name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Job Group
    /// </summary>
    public string Group { get; set; } = "DEFAULT";

    /// <summary>
    /// JobKey（兼容旧数据，新代码不写入）
    /// </summary>
    public string JobKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Job 类型
    /// </summary>
    public string JobType { get; set; } = string.Empty;
    
    /// <summary>
    /// 参数（JSON）
    /// </summary>
    public string Params { get; set; } = "{}";
    
    /// <summary>
    /// 调度配置（JSON）
    /// </summary>
    public string Schedule { get; set; } = "{}";
    
    /// <summary>
    /// 选项（JSON）
    /// </summary>
    public string Options { get; set; } = "{}";
    
    /// <summary>
    /// 声明状态
    /// </summary>
    public SyncStatus Status { get; set; } = SyncStatus.Pending;
    
    /// <summary>
    /// Agent 回写结果（JSON 序列化 JobDetailDto）
    /// </summary>
    public string? ResultJson { get; set; }
    
    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }
}
