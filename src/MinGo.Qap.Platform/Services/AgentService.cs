using Microsoft.EntityFrameworkCore;
using MinGo.Qap.Platform.Data;
using MinGo.Qap.Platform.Data.Entities;
using MinGo.Qap.Shared.Models;
using System.Security.Cryptography;
using System.Text;

namespace MinGo.Qap.Platform.Services;

/// <summary>
/// Agent 管理服务（取代 AgentInstanceService）
/// </summary>
public class AgentService
{
    private readonly PlatformDbContext _dbContext;
    private readonly ILogger<AgentService> _logger;
    private readonly IConfiguration _configuration;

    private const int DefaultHeartbeatIntervalSeconds = 30;
    private const int DefaultWarningThresholdSeconds = 30;
    private const int DefaultOfflineThresholdSeconds = 60;

    public AgentService(
        PlatformDbContext dbContext,
        ILogger<AgentService> logger,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// 注册 Agent（首次注册或重连）
    /// </summary>
    public async Task<RegisterAgentResponse> RegisterAsync(RegisterAgentRequest request, string token)
    {
        // 如果提供了 AgentId，尝试重连
        if (!string.IsNullOrEmpty(request.AgentId))
        {
            var existingAgent = await _dbContext.Agents
                .FirstOrDefaultAsync(a => a.Id == request.AgentId && a.DeletedAt == null);

            if (existingAgent != null)
            {
                // 重连：更新 Agent 信息
                return await ReconnectAsync(existingAgent, request, token);
            }
            else
            {
                _logger.LogWarning("AgentId {AgentId} provided for reconnection but not found, registering as new", request.AgentId);
            }
        }

        // 首次注册
        return await RegisterNewAsync(request, token);
    }

    /// <summary>
    /// 首次注册
    /// </summary>
    private async Task<RegisterAgentResponse> RegisterNewAsync(RegisterAgentRequest request, string token)
    {
        var agentId = GenerateAgentId();
        var tokenHash = HashToken(token);

        var agent = new Agent
        {
            Id = agentId,
            Name = request.Name ?? $"agent-{agentId[..8]}",
            Url = request.Url,
            Status = "Pending",
            AgentVersion = request.AgentVersion,
            TokenHash = tokenHash,
            StartedAt = request.StartedAt != default ? request.StartedAt : DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _dbContext.Agents.AddAsync(agent);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Agent registered: {AgentId} at {Url}", agentId, request.Url);

        return new RegisterAgentResponse
        {
            AgentId = agentId,
            Token = token,
            HeartbeatIntervalSeconds = DefaultHeartbeatIntervalSeconds,
            WarningThresholdSeconds = DefaultWarningThresholdSeconds,
            OfflineThresholdSeconds = DefaultOfflineThresholdSeconds
        };
    }

    /// <summary>
    /// Agent 重连
    /// </summary>
    private async Task<RegisterAgentResponse> ReconnectAsync(Agent agent, RegisterAgentRequest request, string token)
    {
        agent.Name = request.Name ?? agent.Name;
        agent.Url = request.Url;
        agent.AgentVersion = request.AgentVersion ?? agent.AgentVersion;
        agent.StartedAt = request.StartedAt != default ? request.StartedAt : DateTimeOffset.UtcNow;
        agent.Status = "Online";
        agent.TokenHash ??= HashToken(token);
        agent.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Agent reconnected: {AgentId} at {Url}", agent.Id, request.Url);

        return new RegisterAgentResponse
        {
            AgentId = agent.Id,
            Token = token,
            HeartbeatIntervalSeconds = DefaultHeartbeatIntervalSeconds,
            WarningThresholdSeconds = DefaultWarningThresholdSeconds,
            OfflineThresholdSeconds = DefaultOfflineThresholdSeconds
        };
    }

    /// <summary>
    /// 更新心跳
    /// </summary>
    public async Task<bool> UpdateHeartbeatAsync(string agentId)
    {
        var agent = await _dbContext.Agents
            .FirstOrDefaultAsync(a => a.Id == agentId && a.DeletedAt == null);

        if (agent == null) return false;

        agent.LastHeartbeat = DateTimeOffset.UtcNow;
        agent.Status = "Online";
        agent.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 获取 Agent 详情
    /// </summary>
    public async Task<AgentDetailDto?> GetAsync(string agentId)
    {
        var agent = await _dbContext.Agents
            .Include(a => a.AgentSchedulers)
                .ThenInclude(a => a.SchedulerInfo)
            .FirstOrDefaultAsync(a => a.Id == agentId && a.DeletedAt == null);

        if (agent == null) return null;

        return MapToDetail(agent);
    }

    /// <summary>
    /// 获取全部 Agent 列表（Dashboard 等场景使用）
    /// </summary>
    public async Task<List<AgentSummaryDto>> GetAllAsync()
    {
        var agents = await _dbContext.Agents
            .Include(a => a.AgentSchedulers)
            .Where(a => a.DeletedAt == null)
            .OrderByDescending(a => a.UpdatedAt)
            .ToListAsync();

        return agents.Select(MapToSummary).ToList();
    }

    /// <summary>
    /// 获取 Agent 列表（带分页）
    /// </summary>
    public async Task<PagedResponse<AgentSummaryDto>> GetPagedAsync(int page, int pageSize)
    {
        // 总数用于分页元数据
        var total = await _dbContext.Agents
            .Where(a => a.DeletedAt == null)
            .CountAsync();

        var skip = (page - 1) * pageSize;

        var agents = await _dbContext.Agents
            .Include(a => a.AgentSchedulers)
            .Where(a => a.DeletedAt == null)
            .OrderByDescending(a => a.UpdatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        var items = agents.Select(MapToSummary).ToList();

        return new PagedResponse<AgentSummaryDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// 软删除 Agent
    /// </summary>
    public async Task<bool> DeleteAsync(string agentId)
    {
        var agent = await _dbContext.Agents
            .FirstOrDefaultAsync(a => a.Id == agentId && a.DeletedAt == null);

        if (agent == null) return false;

        agent.DeletedAt = DateTimeOffset.UtcNow;
        agent.Status = "Offline";
        agent.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Agent deleted (soft): {AgentId}", agentId);
        return true;
    }

    /// <summary>
    /// 验证 Agent Token
    /// </summary>
    public async Task<bool> ValidateTokenAsync(string agentId, string token)
    {
        var agent = await _dbContext.Agents
            .FirstOrDefaultAsync(a => a.Id == agentId && a.DeletedAt == null);

        if (agent?.TokenHash == null) return false;

        var tokenHash = HashToken(token);
        return agent.TokenHash == tokenHash;
    }

    /// <summary>
    /// 更新 Agent 状态
    /// </summary>
    public async Task UpdateAgentStatusAsync()
    {
        var now = DateTimeOffset.UtcNow;
        var agents = await _dbContext.Agents
            .Where(a => a.DeletedAt == null)
            .ToListAsync();

        var updatedCount = 0;
        foreach (var agent in agents)
        {
            var newStatus = CalculateStatus(agent.LastHeartbeat, agent.Status);
            if (agent.Status != newStatus)
            {
                agent.Status = newStatus;
                agent.UpdatedAt = now;
                updatedCount++;
            }
        }

        if (updatedCount > 0)
        {
            await _dbContext.SaveChangesAsync();
            _logger.LogDebug("Updated status for {Count} agents", updatedCount);
        }
    }

    private string CalculateStatus(DateTimeOffset? lastHeartbeat, string currentStatus)
    {
        if (!lastHeartbeat.HasValue)
            return "Pending";

        var secondsSinceHeartbeat = (DateTimeOffset.UtcNow - lastHeartbeat.Value).TotalSeconds;

        if (secondsSinceHeartbeat <= DefaultWarningThresholdSeconds)
            return "Online";
        else if (secondsSinceHeartbeat <= DefaultOfflineThresholdSeconds)
            return "Warning";
        else
            return "Offline";
    }

    #region Helper Methods

    private string GenerateAgentId()
    {
        return $"agt-{Guid.NewGuid().ToString()[..12]}";
    }

    private string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private AgentSummaryDto MapToSummary(Agent agent)
    {
        return new AgentSummaryDto
        {
            Id = agent.Id,
            Name = agent.Name,
            Url = agent.Url,
            Status = agent.Status,
            AgentVersion = agent.AgentVersion,
            LastHeartbeat = agent.LastHeartbeat?.UtcDateTime,
            StartedAt = agent.StartedAt.UtcDateTime,
            SchedulerCount = agent.AgentSchedulers?.Count ?? 0
        };
    }

    private AgentDetailDto MapToDetail(Agent agent)
    {
        return new AgentDetailDto
        {
            Id = agent.Id,
            Name = agent.Name,
            Url = agent.Url,
            Status = agent.Status,
            AgentVersion = agent.AgentVersion,
            LastHeartbeat = agent.LastHeartbeat,
            LastReportedAt = agent.LastReportedAt,
            StartedAt = agent.StartedAt,
            CreatedAt = agent.CreatedAt,
            UpdatedAt = agent.UpdatedAt,
            Schedulers = agent.AgentSchedulers?.Select(s => new AgentSchedulerDto
            {
                SchedulerInfoId = s.SchedulerInfoId,
                SchedulerName = s.SchedulerInfo?.SchedulerName ?? string.Empty,
                SchedulerInstanceId = s.SchedulerInfo?.SchedulerInstanceId,
                Status = s.SchedulerInfo?.Status ?? "unknown",
                IsClustered = s.SchedulerInfo?.IsClustered ?? false,
                RunningSince = s.SchedulerInfo?.RunningSince,
                ReportedAt = s.ReportedAt
            }).ToList() ?? new()
        };
    }

    #endregion
}
