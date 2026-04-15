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
    Task<JobDefinitionDto> CreateAsync(string clusterId, CreateJobRequest request);
    Task<JobDefinitionDto?> GetAsync(string clusterId, string jobKey);
    Task<List<JobSummaryDto>> GetByClusterAsync(string clusterId, JobQuery query);
    Task UpdateAsync(string clusterId, string jobKey, UpdateJobRequest request);
    Task DeleteAsync(string clusterId, string jobKey);
    Task TriggerAsync(string clusterId, string jobKey);
    Task PauseAsync(string clusterId, string jobKey);
    Task ResumeAsync(string clusterId, string jobKey);
}

/// <summary>
/// Job 服务实现
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

    public async Task<JobDefinitionDto> CreateAsync(string clusterId, CreateJobRequest request)
    {
        var jobDefId = $"job-{Guid.NewGuid().ToString()[..8]}";

        // 1. 记录为 Pending
        var jobDef = new JobDefinition
        {
            Id = jobDefId,
            ClusterId = clusterId,
            JobKey = request.JobKey,
            JobType = request.JobType,
            Params = JsonSerializer.Serialize(request.Params),
            Schedule = JsonSerializer.Serialize(request.Schedule),
            Options = JsonSerializer.Serialize(request.Options),
            Status = SyncStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.JobDefinitions.Add(jobDef);
        await _dbContext.SaveChangesAsync();

        try
        {
            // 2. 转发到 Agent
            var result = await _agentProxy.PostAsync<JobDetailDto>(clusterId, "jobs", request);

            // 3. 更新为 Synced
            jobDef.Status = SyncStatus.Synced;
            jobDef.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Job created successfully: {JobKey} in cluster {ClusterId}",
                request.JobKey, clusterId);

            return MapToDto(jobDef);
        }
        catch (AgentException ex)
        {
            // 4. 标记为 Failed
            jobDef.Status = SyncStatus.Failed;
            jobDef.ErrorMessage = ex.Message;
            jobDef.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            _logger.LogError(ex, "Failed to create job: {JobKey} in cluster {ClusterId}",
                request.JobKey, clusterId);

            throw;
        }
    }

    public async Task<JobDefinitionDto?> GetAsync(string clusterId, string jobKey)
    {
        // 实时从 Agent 获取
        try
        {
            var job = await _agentProxy.GetAsync<JobDetailDto>(clusterId, $"jobs/{jobKey}");
            if (job == null) return null;

            return new JobDefinitionDto
            {
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
                .FirstOrDefaultAsync(j => j.ClusterId == clusterId && j.JobKey == jobKey);

            return jobDef != null ? MapToDto(jobDef) : null;
        }
    }

    public async Task<List<JobSummaryDto>> GetByClusterAsync(string clusterId, JobQuery query)
    {
        // 实时从 Agent 获取
        try
        {
            var jobs = await _agentProxy.GetAsync<List<JobSummaryDto>>(clusterId, 
                $"jobs?page={query.Page}&pageSize={query.PageSize}&status={query.Status}&group={query.Group}&keyword={query.Keyword}");
            
            return jobs ?? new List<JobSummaryDto>();
        }
        catch (AgentException)
        {
            // 如果 Agent 不可用，返回本地备份
            var dbQuery = _dbContext.JobDefinitions
                .Where(j => j.ClusterId == clusterId);

            if (!string.IsNullOrEmpty(query.Status) && 
                Enum.TryParse<SyncStatus>(query.Status, true, out var status))
            {
                dbQuery = dbQuery.Where(j => j.Status == status);
            }

            if (!string.IsNullOrEmpty(query.Keyword))
            {
                dbQuery = dbQuery.Where(j => j.JobKey.Contains(query.Keyword));
            }

            var jobDefs = await dbQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return jobDefs.Select(j => new JobSummaryDto
            {
                JobKey = j.JobKey,
                JobType = j.JobType,
                Status = j.Status.ToString()
            }).ToList();
        }
    }

    public async Task UpdateAsync(string clusterId, string jobKey, UpdateJobRequest request)
    {
        // 1. 更新本地备份
        var jobDef = await _dbContext.JobDefinitions
            .FirstOrDefaultAsync(j => j.ClusterId == clusterId && j.JobKey == jobKey);

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
            jobDef.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();

        // 2. 转发到 Agent
        try
        {
            await _agentProxy.PutAsync<object>(clusterId, $"jobs/{jobKey}", request);

            // 3. 更新为 Synced
            if (jobDef != null)
            {
                jobDef.Status = SyncStatus.Synced;
                await _dbContext.SaveChangesAsync();
            }

            _logger.LogInformation("Job updated successfully: {JobKey} in cluster {ClusterId}",
                jobKey, clusterId);
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

    public async Task DeleteAsync(string clusterId, string jobKey)
    {
        // 1. 转发到 Agent
        await _agentProxy.DeleteAsync(clusterId, $"jobs/{jobKey}");

        // 2. 删除本地备份
        var jobDef = await _dbContext.JobDefinitions
            .FirstOrDefaultAsync(j => j.ClusterId == clusterId && j.JobKey == jobKey);

        if (jobDef != null)
        {
            _dbContext.JobDefinitions.Remove(jobDef);
            await _dbContext.SaveChangesAsync();
        }

        _logger.LogInformation("Job deleted successfully: {JobKey} in cluster {ClusterId}",
            jobKey, clusterId);
    }

    public async Task TriggerAsync(string clusterId, string jobKey)
    {
        await _agentProxy.PostAsync<object>(clusterId, $"jobs/{jobKey}/trigger", new { });
        
        _logger.LogInformation("Job triggered: {JobKey} in cluster {ClusterId}",
            jobKey, clusterId);
    }

    public async Task PauseAsync(string clusterId, string jobKey)
    {
        await _agentProxy.PostAsync<object>(clusterId, $"jobs/{jobKey}/pause", new { });
        
        _logger.LogInformation("Job paused: {JobKey} in cluster {ClusterId}",
            jobKey, clusterId);
    }

    public async Task ResumeAsync(string clusterId, string jobKey)
    {
        await _agentProxy.PostAsync<object>(clusterId, $"jobs/{jobKey}/resume", new { });
        
        _logger.LogInformation("Job resumed: {JobKey} in cluster {ClusterId}",
            jobKey, clusterId);
    }

    #region Helper Methods

    private JobDefinitionDto MapToDto(JobDefinition jobDef)
    {
        return new JobDefinitionDto
        {
            Id = jobDef.Id,
            ClusterId = jobDef.ClusterId,
            JobKey = jobDef.JobKey,
            JobType = jobDef.JobType,
            Params = jobDef.Params,
            Schedule = jobDef.Schedule,
            Options = jobDef.Options,
            Status = jobDef.Status.ToString(),
            ErrorMessage = jobDef.ErrorMessage,
            CreatedAt = jobDef.CreatedAt,
            UpdatedAt = jobDef.UpdatedAt
        };
    }

    #endregion
}
