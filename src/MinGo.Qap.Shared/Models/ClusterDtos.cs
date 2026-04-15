using System.ComponentModel.DataAnnotations;

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
    [Required(ErrorMessage = "集群名称不能为空")]
    [StringLength(50, ErrorMessage = "集群名称长度不能超过 50 个字符")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "环境不能为空")]
    [StringLength(20, ErrorMessage = "环境名称长度不能超过 20 个字符")]
    public string Env { get; set; } = string.Empty;

    [Required(ErrorMessage = "Agent 地址不能为空")]
    [Url(ErrorMessage = "Agent 地址必须是有效的 URL")]
    [StringLength(200, ErrorMessage = "Agent 地址长度不能超过 200 个字符")]
    public string AgentUrl { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "描述长度不能超过 200 个字符")]
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
