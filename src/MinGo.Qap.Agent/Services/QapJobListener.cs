using MinGo.Qap.Shared.Models;
using Quartz;

namespace MinGo.Qap.Agent.Services;

/// <summary>
/// Quartz Job 执行监听器 - 自动收集执行日志
/// </summary>
public class QapJobListener : IJobListener
{
    public string Name => "QapJobListener";

    private readonly ILogCollectionService _logService;
    private readonly ILogger<QapJobListener> _logger;

    public QapJobListener(ILogCollectionService logService, ILogger<QapJobListener> logger)
    {
        _logService = logService;
        _logger = logger;
    }

    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var jobKey = context.JobDetail.Key.ToString();
            _logService.RecordJobStarted(jobKey);
            _logger.LogDebug("Job started: {JobKey}", jobKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in JobToBeExecuted listener");
        }

        return Task.CompletedTask;
    }

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogWarning("Job vetoed: {JobKey}", context.JobDetail.Key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in JobExecutionVetoed listener");
        }

        return Task.CompletedTask;
    }

    public Task JobWasExecuted(
        IJobExecutionContext context,
        JobExecutionException? jobException,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var jobKey = context.JobDetail.Key.ToString();
            var success = jobException == null;

            _logService.RecordJobCompleted(
                jobKey,
                success,
                jobException?.Message,
                jobException?.StackTrace,
                (long)context.JobRunTime.TotalMilliseconds);

            if (success)
            {
                _logger.LogDebug(
                    "Job completed: {JobKey}, Duration: {Duration}ms",
                    jobKey,
                    context.JobRunTime.TotalMilliseconds);
            }
            else
            {
                _logger.LogError(
                    jobException,
                    "Job failed: {JobKey}, Duration: {Duration}ms",
                    jobKey,
                    context.JobRunTime.TotalMilliseconds);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in JobWasExecuted listener");
        }

        return Task.CompletedTask;
    }
}
