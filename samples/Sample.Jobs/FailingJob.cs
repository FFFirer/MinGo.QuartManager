using Microsoft.Extensions.Logging;
using Quartz;

namespace Sample.Jobs;

/// <summary>
/// 失败作业，用于测试错误处理
/// </summary>
public class FailingJob : IJob
{
    private readonly ILogger<FailingJob> _logger;

    public FailingJob(ILogger<FailingJob> logger)
    {
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("[FailingJob] Starting at {Time}", DateTime.Now);
        
        // 模拟失败
        throw new Exception("This job is designed to fail!");
    }
}
