using MinGo.Qap.Shared.Models;
using Microsoft.Extensions.Configuration;
using Quartz;
using Quartz.Impl.Matchers;

namespace MinGo.Qap.Agent.Services;

/// <summary>
/// Quartz 服务接口
/// </summary>
public interface IQuartzService
{
    Task<JobDetailDto> CreateJobAsync(string schedulerName, CreateJobRequest request);
    Task UpdateJobAsync(string schedulerName, string jobKey, UpdateJobRequest request);
    Task DeleteJobAsync(string schedulerName, string jobKey);
    Task TriggerJobAsync(string schedulerName, string jobKey);
    Task PauseJobAsync(string schedulerName, string jobKey);
    Task ResumeJobAsync(string schedulerName, string jobKey);
    Task<JobDetailDto?> GetJobAsync(string schedulerName, string jobKey);
    Task<PagedResponse<JobSummaryDto>> GetJobsAsync(string schedulerName, JobQuery query);
    Task<SchedulerStateDto> GetSchedulerStateAsync(string schedulerName);
    Task<List<string>> GetSchedulerNamesAsync();
}

/// <summary>
/// Scheduler 状态 DTO
/// </summary>
public class SchedulerStateDto
{
    public string Name { get; set; } = string.Empty;
    public string InstanceId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? RunningSince { get; set; }
    public int NumberOfJobsExecuted { get; set; }
    public JobCountsDto JobCounts { get; set; } = new();
    /// <summary>
    /// 是否在集群模式下运行
    /// </summary>
    public bool IsClustered { get; set; }
}

/// <summary>
/// Quartz 服务实现
/// </summary>
public class QuartzService : IQuartzService
{
    private readonly IAgentSchedulerAccessor _schedulerAccessor;
    private readonly IJobConverter _converter;
    private readonly IJobRegistry _registry;
    private readonly ILogger<QuartzService> _logger;
    private readonly IConfiguration _configuration;

    public QuartzService(
        IAgentSchedulerAccessor schedulerAccessor,
        IJobConverter converter,
        IJobRegistry registry,
        ILogger<QuartzService> logger,
        IConfiguration configuration)
    {
        _schedulerAccessor = schedulerAccessor;
        _converter = converter;
        _registry = registry;
        _logger = logger;
        _configuration = configuration;
    }

    private IScheduler GetScheduler(string schedulerName)
    {
        var scheduler = _schedulerAccessor.GetScheduler(schedulerName);
        if (scheduler == null)
        {
            throw new ArgumentException($"Scheduler not found: {schedulerName}");
        }
        return scheduler;
    }

    public Task<List<string>> GetSchedulerNamesAsync()
    {
        return Task.FromResult(_schedulerAccessor.GetAll().Keys.ToList());
    }

    public async Task<JobDetailDto> CreateJobAsync(string schedulerName, CreateJobRequest request)
    {
        var scheduler = GetScheduler(schedulerName);
        _logger.LogInformation("Creating job {JobKey} on scheduler {SchedulerName}", request.JobKey, schedulerName);

        // 验证 Job 类型
        var jobType = _registry.Get(request.JobType);
        if (jobType == null)
        {
            throw new ArgumentException($"Unknown job type: {request.JobType}");
        }

        // 转换参数
        var jobDetail = _converter.ConvertToDetail(request, jobType);
        var trigger = _converter.ConvertToTrigger(request.JobKey, request.Schedule);

        // 调度 Job（replace: true 实现幂等）
        await scheduler.ScheduleJob(jobDetail, new[] { trigger }, replace: true, cancellationToken: default);

        _logger.LogInformation("Job created successfully: {JobKey}", request.JobKey);

        return await GetJobAsync(schedulerName, request.JobKey)
            ?? throw new InvalidOperationException("Failed to retrieve created job");
    }

    public async Task UpdateJobAsync(string schedulerName, string jobKey, UpdateJobRequest request)
    {
        var scheduler = GetScheduler(schedulerName);
        _logger.LogInformation("Updating job: {JobKey}", jobKey);

        var (name, group) = ParseJobKey(jobKey);
        var jobKeyObj = new JobKey(name, group);

        // 获取现有 Job
        var existingJob = await scheduler.GetJobDetail(jobKeyObj);
        if (existingJob == null)
        {
            throw new ArgumentException($"Job not found: {jobKey}");
        }

        // 更新 Trigger（如果提供了 Schedule）
        if (request.Schedule != null)
        {
            var triggerName = $"{name}_trigger";
            var triggerKey = new TriggerKey(triggerName, group);

            // 删除旧 Trigger
            await scheduler.UnscheduleJob(triggerKey);

            // 创建新 Trigger
            var newTrigger = _converter.ConvertToTrigger(jobKey, request.Schedule);
            await scheduler.ScheduleJob(newTrigger);
        }

        // 更新 JobData（如果提供了 Params）
        if (request.Params != null)
        {
            var newJobData = new JobDataMap();
            // 复制现有数据
            foreach (var key in existingJob.JobDataMap.Keys)
            {
                newJobData[key] = existingJob.JobDataMap[key];
            }
            // 更新新参数
            foreach (var param in request.Params)
            {
                newJobData[param.Key] = param.Value;
            }

            // 重新创建 JobDetail（Quartz 不支持直接修改 JobData）
            var newJob = existingJob.GetJobBuilder()
                .UsingJobData(newJobData)
                .Build();

            // 替换 Job
            await scheduler.AddJob(newJob, replace: true);
        }

        _logger.LogInformation("Job updated successfully: {JobKey}", jobKey);
    }

    public async Task DeleteJobAsync(string schedulerName, string jobKey)
    {
        var scheduler = GetScheduler(schedulerName);
        _logger.LogInformation("Deleting job: {JobKey}", jobKey);

        var (name, group) = ParseJobKey(jobKey);
        var jobKeyObj = new JobKey(name, group);

        var deleted = await scheduler.DeleteJob(jobKeyObj);
        if (!deleted)
        {
            throw new ArgumentException($"Job not found or could not be deleted: {jobKey}");
        }

        _logger.LogInformation("Job deleted successfully: {JobKey}", jobKey);
    }

    public async Task TriggerJobAsync(string schedulerName, string jobKey)
    {
        var scheduler = GetScheduler(schedulerName);
        _logger.LogInformation("Triggering job: {JobKey}", jobKey);

        var (name, group) = ParseJobKey(jobKey);
        var jobKeyObj = new JobKey(name, group);

        await scheduler.TriggerJob(jobKeyObj);

        _logger.LogInformation("Job triggered successfully: {JobKey}", jobKey);
    }

    public async Task PauseJobAsync(string schedulerName, string jobKey)
    {
        var scheduler = GetScheduler(schedulerName);
        _logger.LogInformation("Pausing job: {JobKey}", jobKey);

        var (name, group) = ParseJobKey(jobKey);
        var jobKeyObj = new JobKey(name, group);

        await scheduler.PauseJob(jobKeyObj);

        _logger.LogInformation("Job paused successfully: {JobKey}", jobKey);
    }

    public async Task ResumeJobAsync(string schedulerName, string jobKey)
    {
        var scheduler = GetScheduler(schedulerName);
        _logger.LogInformation("Resuming job: {JobKey}", jobKey);

        var (name, group) = ParseJobKey(jobKey);
        var jobKeyObj = new JobKey(name, group);

        await scheduler.ResumeJob(jobKeyObj);

        _logger.LogInformation("Job resumed successfully: {JobKey}", jobKey);
    }

    public async Task<JobDetailDto?> GetJobAsync(string schedulerName, string jobKey)
    {
        var scheduler = GetScheduler(schedulerName);
        var (name, group) = ParseJobKey(jobKey);
        var jobKeyObj = new JobKey(name, group);

        var jobDetail = await scheduler.GetJobDetail(jobKeyObj);
        if (jobDetail == null)
        {
            return null;
        }

        // 获取 Trigger
        var triggers = await scheduler.GetTriggersOfJob(jobKeyObj);
        var trigger = triggers.FirstOrDefault();

        // 获取 Trigger 状态
        var triggerState = trigger != null
            ? await scheduler.GetTriggerState(trigger.Key)
            : TriggerState.None;

        // 构建 DTO
        return new JobDetailDto
        {
            JobKey = jobKey,
            JobType = jobDetail.JobDataMap["jobType"]?.ToString() ?? "unknown",
            Group = jobDetail.Key.Group,
            Status = MapTriggerState(triggerState),
            Description = jobDetail.Description ?? string.Empty,
            NextFireTime = trigger?.GetNextFireTimeUtc()?.DateTime,
            PreviousFireTime = trigger?.GetPreviousFireTimeUtc()?.DateTime,
            Params = jobDetail.JobDataMap.WrappedMap.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value),
            Schedule = ExtractSchedule(trigger),
            Options = ExtractOptions(jobDetail)
        };
    }

    public async Task<PagedResponse<JobSummaryDto>> GetJobsAsync(string schedulerName, JobQuery query)
    {
        var scheduler = GetScheduler(schedulerName);
        var result = new List<JobSummaryDto>();

        // 获取所有 Job Group
        var groups = await scheduler.GetJobGroupNames();

        foreach (var group in groups)
        {
            // 过滤 Group
            if (!string.IsNullOrEmpty(query.Group) && group != query.Group)
            {
                continue;
            }

            var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(group));

            foreach (var jobKey in jobKeys)
            {
                var jobDetail = await scheduler.GetJobDetail(jobKey);
                if (jobDetail == null) continue;

                // 获取 Trigger
                var triggers = await scheduler.GetTriggersOfJob(jobKey);
                var trigger = triggers.FirstOrDefault();
                var triggerState = trigger != null
                    ? await scheduler.GetTriggerState(trigger.Key)
                    : TriggerState.None;

                // 过滤 Status
                var status = MapTriggerState(triggerState);
                if (!string.IsNullOrEmpty(query.Status) && status != query.Status)
                {
                    continue;
                }

                // 过滤 Keyword
                var fullKey = $"{jobKey.Group}.{jobKey.Name}";
                if (!string.IsNullOrEmpty(query.Keyword) &&
                    !fullKey.Contains(query.Keyword, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var scheduleType = GetScheduleType(trigger);
                var cronExpression = trigger is ICronTrigger cronTrigger
                    ? cronTrigger.CronExpressionString
                    : null;

                result.Add(new JobSummaryDto
                {
                    JobKey = fullKey,
                    JobType = jobDetail.JobDataMap["jobType"]?.ToString() ?? "unknown",
                    Group = jobKey.Group,
                    Status = status,
                    ScheduleType = scheduleType,
                    CronExpression = cronExpression,
                    NextFireTime = trigger?.GetNextFireTimeUtc()?.DateTime,
                    PreviousFireTime = trigger?.GetPreviousFireTimeUtc()?.DateTime
                });
            }
        }

        var total = result.Count;
        var items = result
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        return new PagedResponse<JobSummaryDto>
        {
            Items = items,
            Total = total,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<SchedulerStateDto> GetSchedulerStateAsync(string schedulerName)
    {
        var scheduler = GetScheduler(schedulerName);
        var metaData = await scheduler.GetMetaData();
        var jobGroups = await scheduler.GetJobGroupNames();

        var totalJobs = 0;
        var normalCount = 0;
        var pausedCount = 0;
        var blockedCount = 0;

        foreach (var group in jobGroups)
        {
            var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(group));
            totalJobs += jobKeys.Count;

            foreach (var jobKey in jobKeys)
            {
                var triggers = await scheduler.GetTriggersOfJob(jobKey);
                foreach (var trigger in triggers)
                {
                    var state = await scheduler.GetTriggerState(trigger.Key);
                    switch (state)
                    {
                        case TriggerState.Normal:
                            normalCount++;
                            break;
                        case TriggerState.Paused:
                            pausedCount++;
                            break;
                        case TriggerState.Blocked:
                            blockedCount++;
                            break;
                    }
                }
            }
        }

        // 判断是否运行在集群模式
        bool isClustered = false;
        var clusteredConfig = _configuration["quartz:quartz.jobStore.clustered"] ??
                              _configuration["quartz.jobStore.clustered"];
        if (!string.IsNullOrEmpty(clusteredConfig) && clusteredConfig.ToLowerInvariant() == "true")
        {
            isClustered = true;
        }
        else
        {
            // 备用检查：JobStore 类型是否为 AdoJobStore
            isClustered = metaData.JobStoreType?.FullName?.Contains("AdoJobStore") == true;
        }

        return new SchedulerStateDto
        {
            Name = scheduler.SchedulerName,
            InstanceId = scheduler.SchedulerInstanceId,
            Status = metaData.RunningSince.HasValue ? "running" : "standby",
            RunningSince = metaData.RunningSince,
            NumberOfJobsExecuted = metaData.NumberOfJobsExecuted,
            JobCounts = new JobCountsDto
            {
                TotalJobs = totalJobs,
                Normal = normalCount,
                Paused = pausedCount,
                Blocked = blockedCount,
                Executing = 0 // V1 简化，不统计执行中
            },
            IsClustered = isClustered
        };
    }

    #region Helper Methods

    private (string name, string group) ParseJobKey(string jobKey)
    {
        if (string.IsNullOrWhiteSpace(jobKey))
        {
            throw new ArgumentException("JobKey is required");
        }

        var parts = jobKey.Split('.', 2);
        if (parts.Length == 2)
        {
            return (parts[1], parts[0]);
        }

        return (jobKey, "default");
    }

    private string MapTriggerState(TriggerState state)
    {
        return state switch
        {
            TriggerState.Normal => "normal",
            TriggerState.Paused => "paused",
            TriggerState.Blocked => "blocked",
            TriggerState.Complete => "complete",
            TriggerState.Error => "error",
            TriggerState.None => "none",
            _ => "unknown"
        };
    }

    private string GetScheduleType(ITrigger? trigger)
    {
        return trigger switch
        {
            ICronTrigger => "cron",
            ISimpleTrigger simple => simple.RepeatCount == 0 ? "once" : "interval",
            _ => "unknown"
        };
    }

    private ScheduleDto ExtractSchedule(ITrigger? trigger)
    {
        if (trigger == null) return new ScheduleDto();

        return trigger switch
        {
            ICronTrigger cron => new ScheduleDto
            {
                Type = "cron",
                CronExpression = cron.CronExpressionString
            },
            ISimpleTrigger simple => simple.RepeatCount == 0
                ? new ScheduleDto { Type = "once", RunAt = simple.StartTimeUtc != default ? simple.StartTimeUtc.DateTime : null }
                : new ScheduleDto { Type = "interval", IntervalSeconds = simple.RepeatInterval != default ? (int)simple.RepeatInterval.TotalSeconds : null },
            _ => new ScheduleDto()
        };
    }

    private QuartzOptionsDto ExtractOptions(IJobDetail jobDetail)
    {
        var disallowConcurrent = jobDetail.JobDataMap.ContainsKey("disallowConcurrent")
            && jobDetail.JobDataMap.GetBoolean("disallowConcurrent");

        return new QuartzOptionsDto
        {
            DisallowConcurrentExecution = disallowConcurrent,
            MisfirePolicy = "FireAndProceed" // 简化，V1 不存储实际策略
        };
    }

    #endregion
}
