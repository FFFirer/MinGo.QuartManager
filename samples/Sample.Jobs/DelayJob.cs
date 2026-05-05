using Microsoft.Extensions.Logging;
using MinGo.Qap.Shared.Attributes;
using Quartz;

namespace Sample.Jobs;

/// <summary>
/// 延迟作业，用于测试并发执行
/// </summary>
public class DelayJob : IJob
{
    private readonly ILogger<DelayJob> _logger;

    public DelayJob(ILogger<DelayJob> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 延迟秒数
    /// </summary>
    [JobParameter("delaySeconds", Required = false, DefaultValue = 5, Label = "Delay Seconds")]
    public int DelaySeconds { get; set; } = 5;

    public async Task Execute(IJobExecutionContext context)
    {
        var delaySeconds = context.MergedJobDataMap.ContainsKey("delaySeconds") ? Convert.ToInt32(context.MergedJobDataMap["delaySeconds"]) : 5;
        _logger.LogInformation("[DelayJob] Starting with delay of {DelaySeconds} seconds at {Time}", delaySeconds, DateTime.Now);
        
        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
        
        _logger.LogInformation("[DelayJob] Completed at {Time}", DateTime.Now);
    }
}
