namespace MinGo.Qap.Platform.Data.Entities;

/// <summary>
/// Agent 实体（取代 AgentInstance）
/// </summary>
public class Agent
{
    /// <summary>
    /// Agent ID（agt-xxx）
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Agent 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Agent HTTP 端点
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// 状态: Pending/Online/Warning/Offline
    /// </summary>
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Agent 版本
    /// </summary>
    public string? AgentVersion { get; set; }

    /// <summary>
    /// API Token 哈希
    /// </summary>
    public string? TokenHash { get; set; }

    // ⚠ 所有时间字段：DateTimeOffset + timestamptz + 只接受 UtcNow

    /// <summary>
    /// 上次心跳时间（从 Agent 心跳接收 UtcNow）
    /// </summary>
    public DateTimeOffset? LastHeartbeat { get; set; }

    /// <summary>
    /// 最后上报时间（Platform 记录：DateTimeOffset.UtcNow）
    /// </summary>
    public DateTimeOffset? LastReportedAt { get; set; }

    /// <summary>
    /// 启动时间（DateTimeOffset.UtcNow）
    /// </summary>
    public DateTimeOffset StartedAt { get; set; }

    /// <summary>
    /// 创建时间（DateTimeOffset.UtcNow）
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// 更新时间（DateTimeOffset.UtcNow）
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// 删除时间（软删除，DateTimeOffset.UtcNow）
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    // Navigation

    /// <summary>
    /// 关联的 Schedulers
    /// </summary>
    public List<AgentScheduler> AgentSchedulers { get; set; } = new();
}
