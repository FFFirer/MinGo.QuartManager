using Microsoft.Extensions.Logging;
using MinGo.Qap.Shared.Attributes;
using Quartz;

namespace MinGo.Sample.Agent.Jobs;

/// <summary>
/// Manual trigger job - can be triggered via API
/// </summary>
[QuartzJob("sample", Description = "手动触发示例任务")]
public class ManualTriggerJob : IJob
{
    private readonly ILogger<ManualTriggerJob> _logger;

    [JobParameter("workDuration", Description = "模拟工作时长（毫秒）", DefaultValue = 500)]
    public int WorkDuration { get; set; } = 500;

    public ManualTriggerJob(ILogger<ManualTriggerJob> logger)
    {
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var startTime = DateTime.Now;
        var duration = context.MergedJobDataMap.GetInt("workDuration");
        
        _logger.LogInformation("[ManualTriggerJob] Started at {Time}", startTime);

        // Simulate some work
        await Task.Delay(duration);

        var endTime = DateTime.Now;
        _logger.LogInformation("[ManualTriggerJob] Completed at {Time}. Duration: {Duration}ms",
            endTime, (endTime - startTime).TotalMilliseconds);
    }
}