using Microsoft.Extensions.Logging;
using MinGo.Qap.Shared.Attributes;
using Quartz;

namespace MinGo.Sample.Agent.Jobs;

/// <summary>
/// Simple Hello World job - logs a message every 30 seconds
/// </summary>
[QuartzJob("sample", Description = "Hello World 示例任务")]
public class HelloJob : IJob
{
    private readonly ILogger<HelloJob> _logger;

    [JobParameter("message", Description = "日志消息内容", DefaultValue = "Hello from Quartz.NET!")]
    public string Message { get; set; } = "Hello from Quartz.NET!";

    public HelloJob(ILogger<HelloJob> logger)
    {
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        var msg = context.MergedJobDataMap.GetString("message") ?? Message;
        _logger.LogInformation("[HelloJob] {Message} Executed at {Time}", msg, DateTime.Now);
        return Task.CompletedTask;
    }
}