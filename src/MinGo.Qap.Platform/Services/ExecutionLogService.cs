using Microsoft.EntityFrameworkCore;
using MinGo.Qap.Platform.Data;
using MinGo.Qap.Platform.Data.Entities;
using MinGo.Qap.Shared;
using MinGo.Qap.Shared.Models;

namespace MinGo.Qap.Platform.Services;

/// <summary>
/// 执行日志服务接口
/// </summary>
public interface IExecutionLogService
{
    /// <summary>
    /// 批量接收 Agent 上报的执行日志
    /// </summary>
    Task<int> ReceiveLogsAsync(string agentId, List<ExecutionLogDto> logs);

    /// <summary>
    /// 分页查询执行日志
    /// </summary>
    Task<PagedResponse<ExecutionLogEntryDto>> QueryAsync(
        string schedulerName,
        JobKeyDto? jobKey = null,
        int page = 1,
        int pageSize = 20);
}

/// <summary>
/// 执行日志服务实现
/// </summary>
public class ExecutionLogService : IExecutionLogService
{
    private readonly PlatformDbContext _dbContext;
    private readonly ILogger<ExecutionLogService> _logger;

    public ExecutionLogService(
        PlatformDbContext dbContext,
        ILogger<ExecutionLogService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<int> ReceiveLogsAsync(string agentId, List<ExecutionLogDto> logs)
    {
        using var activity = QapTelemetry.ActivitySource.StartActivity("qap.logs.receive");
        activity?.SetTag("agent.id", agentId);
        activity?.SetTag("log.count", logs?.Count ?? 0);

        if (logs == null || logs.Count == 0) return 0;

        var entities = logs.Select(log => new ExecutionLog
        {
            Id = $"log-{Guid.NewGuid().ToString()[..8]}",
            SchedulerName = log.SchedulerName ?? string.Empty,
            JobName = log.JobKey.Name,
            JobGroup = log.JobKey.Group,
            AgentId = agentId,
            StartTime = log.StartTime,
            EndTime = log.EndTime,
            DurationMs = log.DurationMs,
            Success = log.Success,
            ErrorMessage = log.ErrorMessage,
            StackTrace = log.StackTrace,
            CreatedAt = DateTimeOffset.UtcNow
        }).ToList();

        _dbContext.ExecutionLogs.AddRange(entities);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Received {Count} execution logs from agent {AgentId}", entities.Count, agentId);

        QapTelemetry.LogsReceived.Add(entities.Count,
            new KeyValuePair<string, object?>("agent.id", agentId));
        QapTelemetry.LogsBatchSize.Record(entities.Count,
            new KeyValuePair<string, object?>("agent.id", agentId));

        return entities.Count;
    }

    public async Task<PagedResponse<ExecutionLogEntryDto>> QueryAsync(
        string schedulerName,
        JobKeyDto? jobKey = null,
        int page = 1,
        int pageSize = 20)
    {
        var query = _dbContext.ExecutionLogs
            .Where(e => e.SchedulerName == schedulerName);

        if (jobKey != null)
        {
            var jk = jobKey.Value;
            query = query.Where(e => e.JobName == jk.Name && e.JobGroup == jk.Group);
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(e => e.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new ExecutionLogEntryDto
            {
                Id = e.Id,
                SchedulerName = e.SchedulerName,
                JobKey = new JobKeyDto(e.JobName, e.JobGroup),
                AgentId = e.AgentId,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                DurationMs = e.DurationMs,
                Success = e.Success,
                ErrorMessage = e.ErrorMessage,
                StackTrace = e.StackTrace
            })
            .ToListAsync();

        return new PagedResponse<ExecutionLogEntryDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }
}
