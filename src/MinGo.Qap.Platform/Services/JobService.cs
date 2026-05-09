using Microsoft.EntityFrameworkCore;
using MinGo.Qap.Platform.Data;
using MinGo.Qap.Platform.Data.Entities;
using MinGo.Qap.Shared.Enums;
using MinGo.Qap.Shared.Models;
using System.Text.Json;

namespace MinGo.Qap.Platform.Services;

/// <summary>
/// Job 服务接口
/// </summary>
public interface IJobService
{
    Task<JobDefinitionDto> CreateAsync(string schedulerName, CreateJobRequest request);
    Task<JobDefinitionDto?> GetAsync(string schedulerName, JobKeyDto jobKey);
    Task<PagedResponse<JobSummaryDto>> GetBySchedulerAsync(string schedulerName, JobQuery query);
    Task UpdateAsync(string schedulerName, JobKeyDto jobKey, UpdateJobRequest request);
    Task DeleteAsync(string schedulerName, JobKeyDto jobKey);
    Task TriggerAsync(string schedulerName, JobKeyDto jobKey);
    Task PauseAsync(string schedulerName, JobKeyDto jobKey);
    Task ResumeAsync(string schedulerName, JobKeyDto jobKey);
}

/// <summary>
/// 声明冲突异常（409）
/// </summary>
public class DeclarationConflictException : Exception
{
    public DeclarationConflictException(string message) : base(message) { }
}

/// <summary>
/// Job 服务实现 — 声明式创建
/// </summary>
public class JobService : IJobService
{
    private readonly PlatformDbContext _dbContext;
    private readonly IAgentProxyService _agentProxy;
    private readonly ILogger<JobService> _logger;

    public JobService(
        PlatformDbContext dbContext,
        IAgentProxyService agentProxy,
        ILogger<JobService> logger)
    {
        _dbContext = dbContext;
        _agentProxy = agentProxy;
        _logger = logger;
    }

    public async Task<JobDefinitionDto> CreateAsync(string schedulerName, CreateJobRequest request)
    {
        var jobKey = request.JobKey;
        var jobKeyStr = jobKey.ToString();

        // 1. 去重检查
        var existing = await _dbContext.JobDefinitions
            .FirstOrDefaultAsync(j => j.SchedulerName == schedulerName && j.Name == jobKey.Name && j.Group == jobKey.Group);

        JobDefinition jobDef;

        if (existing != null)
        {
            switch (existing.Status)
            {
                case SyncStatus.Synced:
                    throw new DeclarationConflictException("Job已存在");

                case SyncStatus.Pending:
                    // 更新声明（覆盖参数）
                    jobDef = existing;
                    jobDef.Params = JsonSerializer.Serialize(request.Params);
                    jobDef.Schedule = JsonSerializer.Serialize(request.Schedule);
                    jobDef.Options = JsonSerializer.Serialize(request.Options);
                    jobDef.UpdatedAt = DateTimeOffset.UtcNow;
                    _logger.LogInformation("Updating existing pending declaration: {JobKey} on {SchedulerName}",
                        jobKeyStr, schedulerName);
                    break;

                case SyncStatus.Failed:
                    // 重试
                    jobDef = existing;
                    jobDef.Params = JsonSerializer.Serialize(request.Params);
                    jobDef.Schedule = JsonSerializer.Serialize(request.Schedule);
                    jobDef.Options = JsonSerializer.Serialize(request.Options);
                    jobDef.Status = SyncStatus.Pending;
                    jobDef.ErrorMessage = null;
                    jobDef.UpdatedAt = DateTimeOffset.UtcNow;
                    _logger.LogInformation("Retrying failed declaration: {JobKey} on {SchedulerName}",
                        jobKeyStr, schedulerName);
                    break;

                default:
                    jobDef = existing;
                    break;
            }
        }
        else
        {
            // 新建声明
            jobDef = new JobDefinition
            {
                Id = $"job-{Guid.NewGuid().ToString()[..8]}",
                SchedulerName = schedulerName,
                Name = jobKey.Name,
                Group = jobKey.Group,
                JobKey = jobKeyStr,
                JobType = request.JobType.ToAssemblyQualifiedName(),
                Params = JsonSerializer.Serialize(request.Params),
                Schedule = JsonSerializer.Serialize(request.Schedule),
                Options = JsonSerializer.Serialize(request.Options),
                Status = SyncStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _dbContext.JobDefinitions.Add(jobDef);
        }

        await _dbContext.SaveChangesAsync();

        try
        {
            // 2. 调用 Agent 的 PUT /api/agent/jobs（幂等替换）
            var result = await _agentProxy.PutAsync<JobDetailDto>(schedulerName, "jobs", request);

            // 3. 回写结果
            jobDef.Status = SyncStatus.Synced;
            jobDef.ResultJson = JsonSerializer.Serialize(result);
            jobDef.UpdatedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Job declared successfully: {JobKey} on scheduler {SchedulerName}",
                jobKeyStr, schedulerName);

            return MapToDto(jobDef);
        }
        catch (AgentException ex)
        {
            // 4. 标记为 Failed（保留记录）
            jobDef.Status = SyncStatus.Failed;
            jobDef.ErrorMessage = ex.Message;
            jobDef.UpdatedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync();

            _logger.LogError(ex, "Failed to declare job: {JobKey} on scheduler {SchedulerName}",
                jobKeyStr, schedulerName);

            throw;
        }
    }

    public async Task<JobDefinitionDto?> GetAsync(string schedulerName, JobKeyDto jobKey)
    {
        var jobKeyStr = jobKey.ToString();
        // 实时从 Agent 获取
        try
        {
            var url = BuildJobUrl(jobKey);
            var job = await _agentProxy.GetAsync<JobDetailDto>(schedulerName, url);
            if (job == null) return null;

            return new JobDefinitionDto
            {
                SchedulerName = schedulerName,
                JobKey = job.JobKey,
                JobType = job.JobType,
                Params = JsonSerializer.Serialize(job.Params),
                Schedule = JsonSerializer.Serialize(job.Schedule),
                Options = JsonSerializer.Serialize(job.Options),
                Status = job.Status
            };
        }
        catch (AgentException)
        {
            // 如果 Agent 不可用，返回本地备份
            var jobDef = await _dbContext.JobDefinitions
                .FirstOrDefaultAsync(j => j.SchedulerName == schedulerName && j.Name == jobKey.Name && j.Group == jobKey.Group);

            return jobDef != null ? MapToDto(jobDef) : null;
        }
    }

    public async Task<PagedResponse<JobSummaryDto>> GetBySchedulerAsync(string schedulerName, JobQuery query)
    {
        // 实时从 Agent 获取
        try
        {
            var result = await _agentProxy.GetAsync<PagedResponse<JobSummaryDto>>(schedulerName,
                $"jobs?page={query.Page}&pageSize={query.PageSize}&status={query.Status}&group={query.Group}&keyword={query.Keyword}");

            return result ?? new PagedResponse<JobSummaryDto>
            {
                Items = new List<JobSummaryDto>(),
                Total = 0,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }
        catch (AgentException)
        {
            // 如果 Agent 不可用，返回本地备份
            var dbQuery = _dbContext.JobDefinitions
                .Where(j => j.SchedulerName == schedulerName);

            if (!string.IsNullOrEmpty(query.Status) &&
                Enum.TryParse<SyncStatus>(query.Status, true, out var status))
            {
                dbQuery = dbQuery.Where(j => j.Status == status);
            }

            if (!string.IsNullOrEmpty(query.Keyword))
            {
                dbQuery = dbQuery.Where(j => j.JobKey.Contains(query.Keyword));
            }

            var total = await dbQuery.CountAsync();

            var jobDefs = await dbQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResponse<JobSummaryDto>
            {
                Items = jobDefs.Select(j => new JobSummaryDto
                {
                    JobKey = new JobKeyDto(j.Name, j.Group),
                    JobType = JobTypeQualifiedName.ParseFrom(j.JobType),
                    Status = j.Status.ToString()
                }).ToList(),
                Total = total,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }
    }

    public async Task UpdateAsync(string schedulerName, JobKeyDto jobKey, UpdateJobRequest request)
    {
        var jobKeyStr = jobKey.ToString();
        // 1. 更新本地声明
        var jobDef = await _dbContext.JobDefinitions
            .FirstOrDefaultAsync(j => j.SchedulerName == schedulerName && j.Name == jobKey.Name && j.Group == jobKey.Group);

        if (jobDef != null)
        {
            if (request.Params != null)
            {
                jobDef.Params = JsonSerializer.Serialize(request.Params);
            }
            if (request.Schedule != null)
            {
                jobDef.Schedule = JsonSerializer.Serialize(request.Schedule);
            }
            if (request.Options != null)
            {
                jobDef.Options = JsonSerializer.Serialize(request.Options);
            }

            jobDef.Status = SyncStatus.Pending;
            jobDef.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _dbContext.SaveChangesAsync();

        // 2. 转发到 Agent
        try
        {
            var url = BuildJobUrl(jobKey);
            await _agentProxy.PutAsync<object>(schedulerName, url, request);

            // 3. 更新为 Synced
            if (jobDef != null)
            {
                jobDef.Status = SyncStatus.Synced;
                await _dbContext.SaveChangesAsync();
            }
        }
        catch (AgentException ex)
        {
            // 4. 标记为 Failed
            if (jobDef != null)
            {
                jobDef.Status = SyncStatus.Failed;
                jobDef.ErrorMessage = ex.Message;
                await _dbContext.SaveChangesAsync();
            }

            throw;
        }
    }

    public async Task DeleteAsync(string schedulerName, JobKeyDto jobKey)
    {
        var jobKeyStr = jobKey.ToString();
        // 1. 转发到 Agent
        var url = BuildJobUrl(jobKey);
        await _agentProxy.DeleteAsync(schedulerName, url);

        // 2. 删除本地声明
        var jobDef = await _dbContext.JobDefinitions
            .FirstOrDefaultAsync(j => j.SchedulerName == schedulerName && j.Name == jobKey.Name && j.Group == jobKey.Group);

        if (jobDef != null)
        {
            _dbContext.JobDefinitions.Remove(jobDef);
            await _dbContext.SaveChangesAsync();
        }

        _logger.LogInformation("Job deleted successfully: {JobKey} for scheduler {SchedulerName}",
            jobKeyStr, schedulerName);
    }

    public async Task TriggerAsync(string schedulerName, JobKeyDto jobKey)
    {
        var url = BuildJobUrl(jobKey) + "/trigger";
        await _agentProxy.PostAsync<object>(schedulerName, url, new { });

        _logger.LogInformation("Job triggered: {JobKey} for scheduler {SchedulerName}",
            jobKey.ToString(), schedulerName);
    }

    public async Task PauseAsync(string schedulerName, JobKeyDto jobKey)
    {
        var url = BuildJobUrl(jobKey) + "/pause";
        await _agentProxy.PostAsync<object>(schedulerName, url, new { });

        _logger.LogInformation("Job paused: {JobKey} for scheduler {SchedulerName}",
            jobKey.ToString(), schedulerName);
    }

    public async Task ResumeAsync(string schedulerName, JobKeyDto jobKey)
    {
        var url = BuildJobUrl(jobKey) + "/resume";
        await _agentProxy.PostAsync<object>(schedulerName, url, new { });

        _logger.LogInformation("Job resumed: {JobKey} for scheduler {SchedulerName}",
            jobKey.ToString(), schedulerName);
    }

    #region Helper Methods

    private JobDefinitionDto MapToDto(JobDefinition jobDef)
    {
        return new JobDefinitionDto
        {
            Id = jobDef.Id,
            SchedulerName = jobDef.SchedulerName,
            JobKey = new JobKeyDto(jobDef.Name, jobDef.Group),
            JobType = JobTypeQualifiedName.ParseFrom(jobDef.JobType),
            Params = jobDef.Params,
            Schedule = jobDef.Schedule,
            Options = jobDef.Options,
            Status = jobDef.Status.ToString(),
            ErrorMessage = jobDef.ErrorMessage,
            CreatedAt = jobDef.CreatedAt,
            UpdatedAt = jobDef.UpdatedAt
        };
    }

    /// <summary>
    /// 构建 Agent API 的 Job URL 路径，根据 Group 是否为 DEFAULT 决定省略 group 段。
    /// </summary>
    private static string BuildJobUrl(JobKeyDto jobKey)
    {
        return jobKey.Group == "DEFAULT"
            ? $"jobs/{jobKey.Name}"
            : $"jobs/{jobKey.Name}/{jobKey.Group}";
    }

    #endregion
}
