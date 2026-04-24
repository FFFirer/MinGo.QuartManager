using MinGo.Qap.Shared.Models;

namespace MinGo.Qap.Shared.Interfaces;

/// <summary>
/// Agent 注册接口
/// </summary>
public interface IAgentRegistry
{
    /// <summary>
    /// 向 Platform 注册 Agent 实例
    /// </summary>
    Task<AgentRegistrationResult> RegisterAsync(AgentRegistrationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 注销 Agent 实例
    /// </summary>
    Task<bool> DeregisterAsync(string agentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送心跳
    /// </summary>
    Task<HeartbeatResult> SendHeartbeatAsync(string agentId, AgentHeartbeatRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Agent 注册请求
/// </summary>
public record AgentRegistrationRequest(
    string ClusterId,
    string? Name,
    string Url,
    string? QuartzInstanceId,
    string? AgentVersion
);

/// <summary>
/// Agent 注册结果
/// </summary>
public record AgentRegistrationResult(
    bool Success,
    string? AgentId,
    string? Message,
    int? HeartbeatIntervalSeconds
);

/// <summary>
/// 心跳结果
/// </summary>
public record HeartbeatResult(
    bool Success,
    string? Message
);