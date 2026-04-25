using Microsoft.Extensions.Logging;
using Quartz;

namespace MinGo.Sample.Agent.Jobs;

/// <summary>
/// Scheduled job - reports cluster health status periodically
/// </summary>
public class ScheduledJob : IJob
{
    private readonly ILogger<ScheduledJob> _logger;

    public ScheduledJob(ILogger<ScheduledJob> logger)
    {
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var now = DateTime.Now;
        
        _logger.LogInformation("[ScheduledJob] Cluster health check at {Time}", now);
        
        // Simulate health check work
        await Task.Delay(100);
        
        // Report health status
        _logger.LogInformation("[ScheduledJob] Health: OK, Active Jobs: {ActiveJobs}, Memory: {Memory}MB",
            2,
            GC.GetTotalMemory(false) / 1024 / 1024);
    }
}