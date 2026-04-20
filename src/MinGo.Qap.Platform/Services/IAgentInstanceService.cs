using MinGo.Qap.Shared.Enums;
using MinGo.Qap.Shared.Models;

namespace MinGo.Qap.Platform.Services;

/// <summary>
/// Agent 实例服务接口
/// </summary>
public interface IAgentInstanceService
{
    /// <summary>
    /// 注册新的 Agent 实例
    /// </summary>
    /// <param name="clusterId">所属 Cluster ID</param>
    /// <param name="request">注册请求</param>
    /// <param name="token">API Token（原始）</param>
    /// <returns>注册响应</returns>
    Task<AgentRegistrationResponse> RegisterAgentAsync(string clusterId, CreateAgentRequest request, string token);
    
    /// <summary>
    /// 更新 Agent 实例心跳
    /// </summary>
    /// <param name="agentId">Agent 实例 ID</param>
    /// <param name="request">心跳请求</param>
    /// <returns>心跳响应</returns>
    Task<AgentHeartbeatResponse> UpdateHeartbeatAsync(string agentId, AgentHeartbeatRequest request);
    
    /// <summary>
    /// 获取 Agent 实例信息
    /// </summary>
    /// <param name="agentId">Agent 实例 ID</param>
    /// <returns>Agent 实例信息</returns>
    Task<AgentInstanceDto?> GetInstanceAsync(string agentId);
    
    /// <summary>
    /// 获取集群的所有 Agent 实例
    /// </summary>
    /// <param name="clusterId">Cluster ID</param>
    /// <param name="includeDeleted">是否包含已删除的实例</param>
    /// <returns>Agent 实例列表</returns>
    Task<List<AgentInstanceDto>> GetInstancesByClusterAsync(string clusterId, bool includeDeleted = false);
    
    /// <summary>
    /// 获取集群的健康实例列表（状态为 Online 或 Warning）
    /// </summary>
    /// <param name="clusterId">Cluster ID</param>
    /// <returns>健康实例列表</returns>
    Task<List<AgentInstanceDto>> GetHealthyInstancesAsync(string clusterId);
    
    /// <summary>
    /// 更新 Agent 实例信息
    /// </summary>
    /// <param name="agentId">Agent 实例 ID</param>
    /// <param name="request">更新请求</param>
    /// <returns>更新后的实例信息</returns>
    Task<AgentInstanceDto?> UpdateInstanceAsync(string agentId, CreateAgentRequest request);
    
    /// <summary>
    /// 软删除 Agent 实例
    /// </summary>
    /// <param name="agentId">Agent 实例 ID</param>
    /// <returns>删除是否成功</returns>
    Task<bool> DeleteInstanceAsync(string agentId);
    
    /// <summary>
    /// 验证 Agent 实例令牌
    /// </summary>
    /// <param name="agentId">Agent 实例 ID</param>
    /// <param name="token">API Token（原始）</param>
    /// <returns>验证是否通过</returns>
    Task<bool> ValidateTokenAsync(string agentId, string token);
    
    /// <summary>
    /// 计算实例状态（基于最后心跳时间）
    /// </summary>
    /// <param name="lastHeartbeat">最后心跳时间</param>
    /// <returns>计算后的状态</returns>
    AgentStatus CalculateInstanceStatus(DateTime? lastHeartbeat);
    
    /// <summary>
    /// 更新实例状态（基于最后心跳时间）
    /// </summary>
    /// <param name="agentId">Agent 实例 ID</param>
    /// <param name="lastHeartbeat">最后心跳时间</param>
    /// <returns>更新后的状态</returns>
    Task<AgentStatus> UpdateInstanceStatusAsync(string agentId, DateTime? lastHeartbeat);
    
    /// <summary>
    /// 更新集群所有实例的状态（基于最后心跳时间）
    /// </summary>
    /// <param name="clusterId">Cluster ID</param>
    /// <returns>更新的实例数量</returns>
    Task<int> UpdateClusterInstanceStatusesAsync(string clusterId);
    
    /// <summary>
    /// 获取集群的实例统计信息
    /// </summary>
    /// <param name="clusterId">Cluster ID</param>
    /// <returns>实例统计信息</returns>
    Task<InstanceSummaryDto> GetInstanceSummaryAsync(string clusterId);
}