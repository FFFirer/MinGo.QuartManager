using MinGo.Qap.Shared.Enums;
using MinGo.Qap.Shared.Models;
using Quartz;
using System.Collections.Specialized;

namespace MinGo.Qap.Agent.Services;

/// <summary>
/// Job 转换器：将请求转换为 Quartz 对象
/// </summary>
public interface IJobConverter
{
    /// <summary>
    /// 将创建请求转换为 JobDetail
    /// </summary>
    IJobDetail ConvertToDetail(CreateJobRequest request, JobTypeInfoDto jobType);

    /// <summary>
    /// 将调度配置转换为 Trigger。当 Schedule 类型为 "none" 时返回 null。
    /// </summary>
    ITrigger? ConvertToTrigger(TriggerKey triggerKey, JobKey jobKey, ScheduleDto schedule);

    /// <summary>
    /// 转换 Misfire 策略
    /// </summary>
    string ConvertMisfirePolicy(string policy);
}

/// <summary>
/// Job 转换器实现
/// </summary>
public class JobConverter : IJobConverter
{
    public IJobDetail ConvertToDetail(CreateJobRequest request, JobTypeInfoDto jobType)
    {
        var jobKey = request.JobKey;
        
        // 构建 JobDataMap
        var jobDataMap = new JobDataMap();
        jobDataMap["jobType"] = request.JobType.ToAssemblyQualifiedName();
        
        if (request.Params != null)
        {
            foreach (var param in request.Params)
            {
                jobDataMap[param.Key] = param.Value;
            }
        }

        // 解析实际 Job 类型
        Type? actualType = null;
        var aqnString = jobType.JobTypeQualifiedName?.ToAssemblyQualifiedName();
        if (!string.IsNullOrEmpty(aqnString))
        {
            actualType = Type.GetType(aqnString);
            if (actualType == null)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    actualType = assembly.GetType(aqnString);
                    if (actualType != null) break;
                }
            }
        }

        // 创建 JobBuilder
        var jobBuilder = actualType != null
            ? JobBuilder.Create(actualType)
            : JobBuilder.Create();

        jobBuilder.WithIdentity(jobKey.Name, jobKey.Group)
            .UsingJobData(jobDataMap);

        // 持久化 Job
        if (request.Options?.StoreDurable == true)
        {
            jobBuilder.StoreDurably(true);
        }

        // 并发控制
        if (request.Options?.DisallowConcurrentExecution == true)
        {
            // 在 V1 中，我们通过 Job 类上的属性控制
            // 这里只需要记录下来，Job 类应该已经标注 [DisallowConcurrentExecution]
            jobDataMap["disallowConcurrent"] = true;
        }

        return jobBuilder.Build();
    }

    public ITrigger? ConvertToTrigger(TriggerKey triggerKey, JobKey jobKey, ScheduleDto schedule)
    {
        // Schedule=None：不创建 Trigger
        if (string.Equals(schedule.Type, "none", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return schedule.Type?.ToLower() switch
        {
            "once" => BuildOnceTrigger(triggerKey, jobKey, schedule),
            "cron" => BuildCronTrigger(triggerKey, jobKey, schedule),
            "interval" => BuildIntervalTrigger(triggerKey, jobKey, schedule),
            _ => throw new ArgumentException($"Unknown schedule type: {schedule.Type}")
        } ?? throw new ArgumentException("Schedule type is required");
    }

    private ITrigger BuildOnceTrigger(TriggerKey triggerKey, JobKey jobKey, ScheduleDto schedule)
    {
        var triggerBuilder = TriggerBuilder.Create()
            .WithIdentity(triggerKey);

        if (schedule.RunAt.HasValue)
        {
            triggerBuilder.StartAt(schedule.RunAt.Value);
        }
        else
        {
            triggerBuilder.StartNow();
        }

        // 只执行一次
        return triggerBuilder
            .ForJob(jobKey)
            .WithSimpleSchedule(x => x.WithRepeatCount(0))
            .Build();
    }

    private ITrigger BuildCronTrigger(TriggerKey triggerKey, JobKey jobKey, ScheduleDto schedule)
    {
        if (string.IsNullOrWhiteSpace(schedule.CronExpression))
        {
            throw new ArgumentException("Cron expression is required for cron schedule");
        }

        return TriggerBuilder.Create()
            .WithIdentity(triggerKey)
            .ForJob(jobKey)
            .WithCronSchedule(schedule.CronExpression, x =>
            {
                // 可以根据 Misfire 策略配置
                x.WithMisfireHandlingInstructionFireAndProceed();
            })
            .Build();
    }

    private ITrigger BuildIntervalTrigger(TriggerKey triggerKey, JobKey jobKey, ScheduleDto schedule)
    {
        if (!schedule.IntervalSeconds.HasValue || schedule.IntervalSeconds.Value <= 0)
        {
            throw new ArgumentException("IntervalSeconds is required and must be positive for interval schedule");
        }

        return TriggerBuilder.Create()
            .WithIdentity(triggerKey)
            .ForJob(jobKey)
            .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(schedule.IntervalSeconds.Value)
                .RepeatForever()
                .WithMisfireHandlingInstructionFireNow())
            .Build();
    }

    public string ConvertMisfirePolicy(string policy)
    {
        return policy?.ToLower() switch
        {
            "fireandproceed" => "FireAndProceed",
            "ignoremisfire" => "IgnoreMisfire",
            "donothing" => "DoNothing",
            _ => "FireAndProceed" // default
        };
    }

}
