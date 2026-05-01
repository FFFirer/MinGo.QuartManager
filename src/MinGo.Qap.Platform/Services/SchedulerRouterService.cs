using Microsoft.EntityFrameworkCore;
using MinGo.Qap.Platform.Data;
using MinGo.Qap.Platform.Data.Entities;

namespace MinGo.Qap.Platform.Services;

/// <summary>
/// Scheduler 路由服务
/// 根据 SchedulerName 选择一个可用的 Agent
/// </summary>
public class SchedulerRouterService
{
    private readonly PlatformDbContext _dbContext;
    private readonly ILogger<SchedulerRouterService> _logger;

    public SchedulerRouterService(
        PlatformDbContext dbContext,
        ILogger<SchedulerRouterService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// 根据 SchedulerName 选择一个可用的 Agent
    /// </summary>
    /// <param name="schedulerName">Scheduler 名称</param>
    /// <returns>健康的 Agent，如果没有则返回 null</returns>
    public async Task<Agent?> PickAgentForSchedulerAsync(string schedulerName)
    {
        // 1. 查 AgentScheduler 关联表，获取关联的 Agents
        var agentSchedulers = await _dbContext.AgentSchedulers
            .Include(a => a.Agent)
            .Include(a => a.SchedulerInfo)
            .Where(a => a.SchedulerInfo.SchedulerName == schedulerName)
            .ToListAsync();

        if (agentSchedulers.Count == 0)
        {
            _logger.LogWarning("No agents found for scheduler {SchedulerName}", schedulerName);
            return null;
        }

        // 2. 过滤健康的 Agent（Online 或 Warning）
        var healthy = agentSchedulers
            .Where(a => a.Agent.DeletedAt == null &&
                       (a.Agent.Status == "Online" || a.Agent.Status == "Warning"))
            .Select(a => a.Agent)
            .Distinct()
            .ToList();

        if (healthy.Count == 0)
        {
            _logger.LogWarning("No healthy agents found for scheduler {SchedulerName}", schedulerName);
            return null;
        }

        // 3. 随机选择（后续可扩展为轮询/一致性哈希）
        var selected = healthy[Random.Shared.Next(healthy.Count)];
        _logger.LogDebug(
            "Selected agent {AgentId} for scheduler {SchedulerName}",
            selected.Id, schedulerName);

        return selected;
    }
}
