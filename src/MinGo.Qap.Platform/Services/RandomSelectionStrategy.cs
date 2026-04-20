using MinGo.Qap.Shared.Models;

namespace MinGo.Qap.Platform.Services;

/// <summary>
/// 随机选择策略实现
/// </summary>
public class RandomSelectionStrategy : IAgentSelectionStrategy
{
    private readonly Random _random = new();
    
    public string Name => "Random";
    
    public AgentInstanceDto? SelectInstance(string clusterId, List<AgentInstanceDto> healthyInstances)
    {
        if (healthyInstances == null || healthyInstances.Count == 0)
        {
            return null;
        }
        
        var index = _random.Next(healthyInstances.Count);
        return healthyInstances[index];
    }
}