using Microsoft.EntityFrameworkCore;
using MinGo.Qap.Shared.Enums;

namespace MinGo.Qap.Platform.Data.Entities;

/// <summary>
/// Agent 实例实体
/// </summary>
public class AgentInstance
{
    /// <summary>
    /// Agent 实例 ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// 所属 Cluster ID
    /// </summary>
    public string ClusterId { get; set; } = string.Empty;
    
    /// <summary>
    /// 显示名称（可选）
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
    /// 上次心跳时间
    /// </summary>
    public DateTimeOffset? LastHeartbeat { get; set; }
    
    /// <summary>
    /// Quartz 实例 ID（用于集群）
    /// </summary>
    public string? QuartzInstanceId { get; set; }
    
    /// <summary>
    /// API Token 哈希
    /// </summary>
    public string? TokenHash { get; set; }
    
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
    
    /// <summary>
    /// 删除时间（软删除）
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }
    
    /// <summary>
    /// 关联的 Cluster
    /// </summary>
    public Cluster Cluster { get; set; } = null!;
}