using Microsoft.Extensions.Logging;
using MinGo.Qap.Shared.Attributes;
using Quartz;

namespace MinGo.Sample.Agent.Jobs;

/// <summary>
/// Scheduled job - reports cluster health status periodically
/// </summary>
[QuartzJob("sample", Description = "集群健康检查任务")]
public class ScheduledJob : IJob
{
    private readonly ILogger<ScheduledJob> _logger;

    [JobParameter("simulateDelay", Description = "模拟延迟毫秒数", DefaultValue = 100)]
    public int SimulateDelay { get; set; } = 100;

    public ScheduledJob(ILogger<ScheduledJob> logger)
    {
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var now = DateTime.Now;
        var delay = context.MergedJobDataMap.GetInt("simulateDelay");
        
        _logger.LogInformation("[ScheduledJob] Cluster health check at {Time}", now);
        
        // Simulate health check work
        await Task.Delay(delay);
        
        // Report health status
        _logger.LogInformation("[ScheduledJob] Health: OK, Active Jobs: {ActiveJobs}, Memory: {Memory}MB",
            2,
            GC.GetTotalMemory(false) / 1024 / 1024);
    }
}