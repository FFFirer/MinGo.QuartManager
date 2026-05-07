using MinGo.Qap.Shared.Enums;

namespace MinGo.Qap.Shared.Models;

/// <summary>
/// Agent 实例信息
/// </summary>
public class AgentInstanceDto
{
    /// <summary>
    /// Agent 实例 ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// 实例显示名称
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Agent URL
    /// </summary>
    public string Url { get; set; } = string.Empty;
    
    /// <summary>
    /// 状态
    /// </summary>
    public AgentStatus Status { get; set; } = AgentStatus.Pending;
    
    /// <summary>
    /// 最后心跳时间
    /// </summary>
    public DateTimeOffset? LastHeartbeat { get; set; }
    
    /// <summary>
    /// Quartz 实例 ID
    /// </summary>
    public string? QuartzInstanceId { get; set; }
    
    /// <summary>
    /// Agent 版本
    /// </summary>
    public string? AgentVersion { get; set; }
    
    /// <summary>
    /// 启动时间
    /// </summary>
    public DateTimeOffset? StartedAt { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }
}

/// <summary>
/// 创建 Agent 实例请求
/// </summary>
public class CreateAgentRequest
{
    /// <summary>
    /// 实例显示名称
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Agent URL
    /// </summary>
    public string Url { get; set; } = string.Empty;
    
    /// <summary>
    /// Agent 版本
    /// </summary>
    public string? AgentVersion { get; set; }
    
    /// <summary>
    /// Quartz 实例 ID（可选，由 Agent 生成）
    /// </summary>
    public string? QuartzInstanceId { get; set; }
}

/// <summary>
/// Agent 实例摘要
/// </summary>
public class AgentSummaryDto
{
    /// <summary>
    /// Agent 实例 ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// 实例显示名称
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Agent URL
    /// </summary>
    public string Url { get; set; } = string.Empty;
    
    /// <summary>
    /// 状态
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// 最后心跳时间
    /// </summary>
    public DateTimeOffset? LastHeartbeat { get; set; }
    
    /// <summary>
    /// Agent 版本
    /// </summary>
    public string? AgentVersion { get; set; }
    
    /// <summary>
    /// 启动时间
    /// </summary>
    public DateTimeOffset? StartedAt { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// 关联的 Scheduler 数量
    /// </summary>
    public int SchedulerCount { get; set; }
}