using Microsoft.Extensions.Logging;
using MinGo.Qap.Shared.Attributes;
using Quartz;

namespace Sample.Jobs;

/// <summary>
/// 回显作业，用于测试
/// </summary>
public class EchoJob : IJob
{
    private readonly ILogger<EchoJob> _logger;

    public EchoJob(ILogger<EchoJob> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 要回显的消息内容
    /// </summary>
    [JobParameter("message", Required = false, DefaultValue = "Hello, Quartz!", Label = "Message")]
    public string Message { get; set; } = "Hello, Quartz!";

    public async Task Execute(IJobExecutionContext context)
    {
        var message = context.MergedJobDataMap["message"]?.ToString() ?? "Hello, Quartz!";
        _logger.LogInformation("[EchoJob] {Message} at {Time}", message, DateTime.Now);
        await Task.CompletedTask;
    }
}
