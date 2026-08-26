using Microsoft.EntityFrameworkCore;
using MinGo.Qap.Platform.Caching;
using MinGo.Qap.Platform.Data;
using MinGo.Qap.Platform.Data.Entities;
using MinGo.Qap.Shared;
using MinGo.Qap.Shared.Models;

namespace MinGo.Qap.Platform.Services;

/// <summary>
/// Scheduler 信息管理服务
/// </summary>
public class SchedulerService
{
    private readonly PlatformDbContext _dbContext;
    private readonly IManifestCacheService _manifestCache;
    private readonly ILogger<SchedulerService> _logger;

    public SchedulerService(
        PlatformDbContext dbContext,
        IManifestCacheService manifestCache,
        ILogger<SchedulerService> logger)
    {
        _dbContext = dbContext;
        _manifestCache = manifestCache;
        _logger = logger;
    }

    /// <summary>
    /// 接收 Agent 上报的 Scheduler 信息（全量替换）
    /// </summary>
    public async Task ReportSchedulersAsync(string agentId, SchedulerReportRequest request)
    {
        using var activity = QapTelemetry.ActivitySource.StartActivity("qap.scheduler.report");
        activity?.SetTag("agent.id", agentId);
        activity?.SetTag("scheduler.count", request.Schedulers.Count);

        var agent = await _dbContext.Agents
            .FirstOrDefaultAsync(a => a.Id == agentId && a.DeletedAt == null);

        if (agent == null)
        {
            throw new ArgumentException($"Agent '{agentId}' not found");
        }

        var utcNow = DateTimeOffset.UtcNow;
        agent.LastReportedAt = utcNow;
        agent.UpdatedAt = utcNow;

        // 1. 删除旧的 Agent-Scheduler 关联
        var oldLinks = await _dbContext.AgentSchedulers
            .Where(a => a.AgentId == agentId)
            .ToListAsync();
        _dbContext.AgentSchedulers.RemoveRange(oldLinks);

        // 2. 遍历上报的 Scheduler 列表
        foreach (var dto in request.Schedulers)
        {
            // 查找或创建 SchedulerInfo
            var schedulerInfo = await FindOrCreateSchedulerInfoAsync(dto, utcNow);

            // 3. 创建新的关联
            var link = new AgentScheduler
            {
                AgentId = agentId,
                SchedulerInfoId = schedulerInfo.Id,
                ReportedAt = utcNow
            };
            await _dbContext.AgentSchedulers.AddAsync(link);
        }

        await _dbContext.SaveChangesAsync();

        // Agent 重新上报 Scheduler = Agent 重连或重启，清除 manifest 缓存
        _manifestCache.InvalidateForSchedulers(request.Schedulers.Select(s => s.SchedulerName));

        _logger.LogInformation(
            "Agent {AgentId} reported {Count} schedulers, manifest cache cleared",
            agentId, request.Schedulers.Count);
    }

    /// <summary>
    /// 查找或创建 SchedulerInfo 实体
    /// </summary>
    private async Task<SchedulerInfo> FindOrCreateSchedulerInfoAsync(SchedulerInfoDto dto, DateTimeOffset utcNow)
    {
        // 按 (SchedulerName, SchedulerInstanceId) 查找
        var existing = await _dbContext.SchedulerInfos
            .FirstOrDefaultAsync(s =>
                s.SchedulerName == dto.SchedulerName &&
                s.SchedulerInstanceId == dto.SchedulerInstanceId);

        if (existing != null)
        {
            // 更新现有信息
            existing.Status = dto.Status;
            existing.IsClustered = dto.IsClustered;
            existing.JobStoreType = dto.JobStoreType;
            existing.ThreadPoolType = dto.ThreadPoolType;
            existing.ThreadPoolSize = dto.ThreadPoolSize;
            existing.RunningSince = dto.RunningSince;
            existing.Version = dto.Version;
            existing.NumberOfJobsExecuted = dto.NumberOfJobsExecuted;
            existing.PropertiesJson = dto.Properties != null
                ? System.Text.Json.JsonSerializer.Serialize(dto.Properties)
                : existing.PropertiesJson;
            existing.LastReportedAt = utcNow;

            return existing;
        }

        // 创建新实体
        var schedulerInfo = new SchedulerInfo
        {
            Id = GenerateSchedulerId(),
            SchedulerName = dto.SchedulerName,
            SchedulerInstanceId = dto.SchedulerInstanceId,
            Status = dto.Status,
            IsClustered = dto.IsClustered,
            JobStoreType = dto.JobStoreType,
            ThreadPoolType = dto.ThreadPoolType,
            ThreadPoolSize = dto.ThreadPoolSize,
            RunningSince = dto.RunningSince,
            Version = dto.Version,
            NumberOfJobsExecuted = dto.NumberOfJobsExecuted,
            PropertiesJson = dto.Properties != null
                ? System.Text.Json.JsonSerializer.Serialize(dto.Properties)
                : null,
            FirstReportedAt = utcNow,
            LastReportedAt = utcNow
        };

        await _dbContext.SchedulerInfos.AddAsync(schedulerInfo);
        return schedulerInfo;
    }

    /// <summary>
    /// 获取 Scheduler 详情（含关联 Agents）
    /// </summary>
    public async Task<SchedulerDetailDto?> GetSchedulerAsync(string schedulerName)
    {
        var schedulerInfo = await _dbContext.SchedulerInfos
            .Include(s => s.AgentSchedulers)
                .ThenInclude(a => a.Agent)
            .FirstOrDefaultAsync(s => s.SchedulerName == schedulerName);

        if (schedulerInfo == null) return null;

        return MapToDetail(schedulerInfo);
    }

    /// <summary>
    /// 获取所有 Scheduler
    /// </summary>
    public async Task<List<SchedulerSummaryDto>> GetAllSchedulersAsync()
    {
        var schedulers = await _dbContext.SchedulerInfos
            .Include(s => s.AgentSchedulers)
            .OrderByDescending(s => s.LastReportedAt)
            .ToListAsync();

        return schedulers.Select(MapToSummary).ToList();
    }

    /// <summary>
    /// 获取 Scheduler 关联的 Agents
    /// </summary>
    public async Task<List<SchedulerAgentDto>> GetAgentsBySchedulerAsync(string schedulerName)
    {
        var schedulerInfo = await _dbContext.SchedulerInfos
            .Include(s => s.AgentSchedulers)
                .ThenInclude(a => a.Agent)
            .FirstOrDefaultAsync(s => s.SchedulerName == schedulerName);

        if (schedulerInfo == null) return new();

        return schedulerInfo.AgentSchedulers
            .Where(a => a.Agent.DeletedAt == null)
            .Select(a => new SchedulerAgentDto
            {
                AgentId = a.AgentId,
                AgentName = a.Agent.Name,
                AgentUrl = a.Agent.Url,
                AgentStatus = a.Agent.Status,
                ReportedAt = a.ReportedAt
            }).ToList();
    }

    /// <summary>
    /// 获取 Agent 关联的 Schedulers
    /// </summary>
    public async Task<List<AgentSchedulerDto>> GetSchedulersByAgentAsync(string agentId)
    {
        var agent = await _dbContext.Agents
            .Include(a => a.AgentSchedulers)
                .ThenInclude(a => a.SchedulerInfo)
            .FirstOrDefaultAsync(a => a.Id == agentId && a.DeletedAt == null);

        if (agent == null) return new();

        return agent.AgentSchedulers
            .Select(a => new AgentSchedulerDto
            {
                SchedulerInfoId = a.SchedulerInfoId,
                SchedulerName = a.SchedulerInfo?.SchedulerName ?? string.Empty,
                SchedulerInstanceId = a.SchedulerInfo?.SchedulerInstanceId,
                Status = a.SchedulerInfo?.Status ?? "unknown",
                IsClustered = a.SchedulerInfo?.IsClustered ?? false,
                RunningSince = a.SchedulerInfo?.RunningSince,
                ReportedAt = a.ReportedAt
            }).ToList();
    }

    #region Helper Methods

    private string GenerateSchedulerId()
    {
        return $"sch-{Guid.NewGuid().ToString()[..12]}";
    }

    private SchedulerSummaryDto MapToSummary(SchedulerInfo schedulerInfo)
    {
        return new SchedulerSummaryDto
        {
            Id = schedulerInfo.Id,
            SchedulerName = schedulerInfo.SchedulerName,
            SchedulerInstanceId = schedulerInfo.SchedulerInstanceId,
            Status = schedulerInfo.Status,
            IsClustered = schedulerInfo.IsClustered,
            RunningSince = schedulerInfo.RunningSince,
            LastReportedAt = schedulerInfo.LastReportedAt,
            AgentCount = schedulerInfo.AgentSchedulers?.Count ?? 0
        };
    }

    private SchedulerDetailDto MapToDetail(SchedulerInfo schedulerInfo)
    {
        Dictionary<string, string>? properties = null;
        if (!string.IsNullOrEmpty(schedulerInfo.PropertiesJson))
        {
            try
            {
                properties = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(schedulerInfo.PropertiesJson);
            }
            catch { }
        }

        return new SchedulerDetailDto
        {
            Id = schedulerInfo.Id,
            SchedulerName = schedulerInfo.SchedulerName,
            SchedulerInstanceId = schedulerInfo.SchedulerInstanceId,
            Status = schedulerInfo.Status,
            IsClustered = schedulerInfo.IsClustered,
            JobStoreType = schedulerInfo.JobStoreType,
            ThreadPoolType = schedulerInfo.ThreadPoolType,
            ThreadPoolSize = schedulerInfo.ThreadPoolSize,
            RunningSince = schedulerInfo.RunningSince,
            Version = schedulerInfo.Version,
            NumberOfJobsExecuted = schedulerInfo.NumberOfJobsExecuted,
            Properties = properties,
            FirstReportedAt = schedulerInfo.FirstReportedAt,
            LastReportedAt = schedulerInfo.LastReportedAt,
            Agents = schedulerInfo.AgentSchedulers?
                .Where(a => a.Agent.DeletedAt == null)
                .Select(a => new SchedulerAgentDto
                {
                    AgentId = a.AgentId,
                    AgentName = a.Agent.Name,
                    AgentUrl = a.Agent.Url,
                    AgentStatus = a.Agent.Status,
                    ReportedAt = a.ReportedAt
                }).ToList() ?? new()
        };
    }

    #endregion
}
