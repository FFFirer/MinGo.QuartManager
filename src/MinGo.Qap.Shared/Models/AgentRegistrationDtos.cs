using System.ComponentModel.DataAnnotations;

namespace MinGo.Qap.Shared.Models;

/// <summary>
/// 注册 Agent 请求
/// </summary>
public class RegisterAgentRequest
{
    /// <summary>
    /// Agent ID（首次注册时为 null，重连时携带）
    /// </summary>
    public string? AgentId { get; set; }

    /// <summary>
    /// Agent 名称（可选）
    /// </summary>
    [StringLength(100, ErrorMessage = "名称长度不能超过 100 个字符")]
    public string? Name { get; set; }

    /// <summary>
    /// Agent URL 地址
    /// </summary>
    [Required(ErrorMessage = "URL 不能为空")]
    [Url(ErrorMessage = "URL 格式无效")]
    [StringLength(512, ErrorMessage = "URL 长度不能超过 512 个字符")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Agent 版本
    /// </summary>
    public string? AgentVersion { get; set; }

    /// <summary>
    /// 启动时间（UTC）
    /// </summary>
    public DateTimeOffset StartedAt { get; set; }
}

/// <summary>
/// 注册 Agent 响应
/// </summary>
public class RegisterAgentResponse
{
    /// <summary>
    /// 分配的 Agent ID
    /// </summary>
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// Agent Token（用于后续认证）
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// 心跳间隔（秒）
    /// </summary>
    public int HeartbeatIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// 警告阈值（秒）- 超过此时间未收到心跳则标记为 Warning
    /// </summary>
    public int WarningThresholdSeconds { get; set; } = 30;

    /// <summary>
    /// 离线阈值（秒）- 超过此时间未收到心跳则标记为 Offline
    /// </summary>
    public int OfflineThresholdSeconds { get; set; } = 60;
}

/// <summary>
/// Agent 心跳请求（v2，使用 DateTimeOffset）
/// </summary>
public class AgentHeartbeatRequestV2
{
    /// <summary>
    /// Agent ID
    /// </summary>
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// 当前状态
    /// </summary>
    public string Status { get; set; } = "Online";

    /// <summary>
    /// 心跳时间（UTC）
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Scheduler 状态摘要
    /// </summary>
    public List<SchedulerStatusSummary>? SchedulerSummaries { get; set; }

    /// <summary>
    /// 元数据
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Scheduler 状态摘要（用于心跳）
/// </summary>
public class SchedulerStatusSummary
{
    public string SchedulerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int JobCount { get; set; }
    public int RunningJobCount { get; set; }
}

/// <summary>
/// Agent 心跳响应（v2）
/// </summary>
public class AgentHeartbeatResponseV2
{
    /// <summary>
    /// 服务端确认时间（UTC）
    /// </summary>
    public DateTimeOffset ServerTime { get; set; }

    /// <summary>
    /// 是否需要重新上报 Scheduler 信息
    /// </summary>
    public bool ShouldReportSchedulers { get; set; }

    /// <summary>
    /// 下一心跳间隔（秒，可选）
    /// </summary>
    public int? NextHeartbeatIntervalSeconds { get; set; }
}
