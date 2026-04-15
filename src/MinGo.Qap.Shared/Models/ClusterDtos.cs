namespace MinGo.Qap.Shared.Models;

/// <summary>
/// Cluster 数据传输对象
/// </summary>
public class ClusterDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Env { get; set; } = string.Empty;
    public string AgentUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? LastHeartbeat { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Cluster 摘要（列表展示用）
/// </summary>
public class ClusterSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Env { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? LastHeartbeat { get; set; }
    public int JobCount { get; set; }
}

/// <summary>
/// 创建 Cluster 请求
/// </summary>
public class CreateClusterRequest
{
    public string Name { get; set; } = string.Empty;
    public string Env { get; set; } = string.Empty;
    public string AgentUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>
/// Cluster 创建响应（包含 Token）
/// </summary>
public class CreateClusterResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
