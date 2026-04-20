using MinGo.Qap.Shared.Models;

namespace MinGo.Qap.Platform.Services;

/// <summary>
/// Agent 实例选择策略接口
/// </summary>
public interface IAgentSelectionStrategy
{
    /// <summary>
    /// 从健康实例中选择一个 Agent 实例
    /// </summary>
    /// <param name="clusterId">集群 ID</param>
    /// <param name="healthyInstances">可用的健康实例列表</param>
    /// <returns>选择的 Agent 实例，如果没有可用实例则返回 null</returns>
    AgentInstanceDto? SelectInstance(string clusterId, List<AgentInstanceDto> healthyInstances);
    
    /// <summary>
    /// 策略名称（用于日志和配置）
    /// </summary>
    string Name { get; }
}