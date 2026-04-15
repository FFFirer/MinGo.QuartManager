using Quartz;

namespace MinGo.Qap.Agent.Services;

/// <summary>
/// Agent 健康检查服务
/// </summary>
public class HealthCheckService
{
    private readonly IScheduler _scheduler;
    private readonly ILogger<HealthCheckService> _logger;

    public HealthCheckService(IScheduler scheduler, ILogger<HealthCheckService> logger)
    {
        _scheduler = scheduler;
        _logger = logger;
    }

    /// <summary>
    /// 执行健康检查
    /// </summary>
    public async Task<HealthStatus> CheckHealthAsync()
    {
        try
        {
            var schedulerMetaData = await _scheduler.GetMetaData();
            var isStarted = schedulerMetaData.RunningSince.HasValue;
            var isShutdown = _scheduler.IsShutdown;

            var status = new HealthStatus
            {
                Healthy = isStarted && !isShutdown,
                SchedulerStatus = isShutdown ? "shutdown" : 
                                 (isStarted ? "running" : "standby"),
                RunningSince = schedulerMetaData.RunningSince,
                NumberOfJobsExecuted = schedulerMetaData.NumberOfJobsExecuted,
                Version = schedulerMetaData.Version
            };

            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return new HealthStatus
            {
                Healthy = false,
                SchedulerStatus = "error",
                ErrorMessage = ex.Message
            };
        }
    }
}

/// <summary>
/// 健康状态
/// </summary>
public class HealthStatus
{
    /// <summary>
    /// 是否健康
    /// </summary>
    public bool Healthy { get; set; }

    /// <summary>
    /// Scheduler 状态: running, standby, shutdown, error
    /// </summary>
    public string SchedulerStatus { get; set; } = string.Empty;

    /// <summary>
    /// 启动时间
    /// </summary>
    public DateTimeOffset? RunningSince { get; set; }

    /// <summary>
    /// 已执行的 Job 数量
    /// </summary>
    public int NumberOfJobsExecuted { get; set; }

    /// <summary>
    /// Quartz 版本
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// 错误信息（如果有）
    /// </summary>
    public string? ErrorMessage { get; set; }
}
