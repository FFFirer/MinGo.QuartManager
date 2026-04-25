using Microsoft.Extensions.Logging;
using Quartz;

namespace MinGo.Sample.Agent.Jobs;

/// <summary>
/// Manual trigger job - can be triggered via API
/// </summary>
public class ManualTriggerJob : IJob
{
    private readonly ILogger<ManualTriggerJob> _logger;

    public ManualTriggerJob(ILogger<ManualTriggerJob> logger)
    {
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var startTime = DateTime.Now;
        _logger.LogInformation("[ManualTriggerJob] Started at {Time}", startTime);

        // Simulate some work
        await Task.Delay(500);

        var endTime = DateTime.Now;
        _logger.LogInformation("[ManualTriggerJob] Completed at {Time}. Duration: {Duration}ms",
            endTime, (endTime - startTime).TotalMilliseconds);
    }
}