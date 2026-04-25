using Microsoft.Extensions.Logging;
using Quartz;

namespace MinGo.Sample.Agent.Jobs;

/// <summary>
/// Simple Hello World job - logs a message every 30 seconds
/// </summary>
public class HelloJob : IJob
{
    private readonly ILogger<HelloJob> _logger;

    public HelloJob(ILogger<HelloJob> logger)
    {
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("[HelloJob] Hello from Quartz.NET! Executed at {Time}", DateTime.Now);
        return Task.CompletedTask;
    }
}