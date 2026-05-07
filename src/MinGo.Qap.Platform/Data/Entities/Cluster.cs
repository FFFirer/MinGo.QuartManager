using Microsoft.EntityFrameworkCore;
using MinGo.Qap.Shared.Enums;

namespace MinGo.Qap.Platform.Data.Entities;

/// <summary>
/// Cluster 实体
/// </summary>
public class Cluster
{
    /// <summary>
    /// Cluster ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// 显示名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 环境（prod/staging/dev）
    /// </summary>
    public string Env { get; set; } = string.Empty;
    
    /// <summary>
    /// Agent URL（已弃用，迁移期间可为空）
    /// </summary>
    public string? AgentUrl { get; set; }
    
    /// <summary>
    /// 最后心跳时间（已弃用，迁移期间保留）
    /// </summary>
    public DateTimeOffset? LastHeartbeat { get; set; }
    
    /// <summary>
    /// 状态
    /// </summary>
    public ClusterStatus Status { get; set; } = ClusterStatus.Pending;
    
    /// <summary>
    /// API Token 哈希
    /// </summary>
    public string? TokenHash { get; set; }
    
    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }
    
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
    /// 关联的 Agent 实例
    /// </summary>
    public List<AgentInstance> AgentInstances { get; set; } = new();
    
    /// <summary>
    /// 关联的 JobDefinitions
    /// </summary>
    public List<JobDefinition> JobDefinitions { get; set; } = new();
}

/// <summary>
/// JobDefinition 实体（平台备份）
/// </summary>
public class JobDefinition
{
    /// <summary>
    /// ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// 所属 Cluster ID
    /// </summary>
    public string ClusterId { get; set; } = string.Empty;
    
    /// <summary>
    /// JobKey（Name.Group）
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
    /// 同步状态
    /// </summary>
    public SyncStatus Status { get; set; } = SyncStatus.Pending;
    
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
    
    /// <summary>
    /// 关联的 Cluster
    /// </summary>
    public Cluster Cluster { get; set; } = null!;
}
