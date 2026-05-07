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
    /// 将调度配置转换为 Trigger
    /// </summary>
    ITrigger ConvertToTrigger(string jobKey, ScheduleDto schedule);
    
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
        // 解析 JobKey
        var (name, group) = ParseJobKey(request.JobKey);
        
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

        jobBuilder.WithIdentity(name, group)
            .UsingJobData(jobDataMap);

        // 并发控制
        if (request.Options?.DisallowConcurrentExecution == true)
        {
            // 在 V1 中，我们通过 Job 类上的属性控制
            // 这里只需要记录下来，Job 类应该已经标注 [DisallowConcurrentExecution]
            jobDataMap["disallowConcurrent"] = true;
        }

        return jobBuilder.Build();
    }

    public ITrigger ConvertToTrigger(string jobKey, ScheduleDto schedule)
    {
        var (name, group) = ParseJobKey(jobKey);
        
        // Trigger Key: 与 Job 相同 group，名称加后缀
        var triggerName = $"{name}_trigger";

        return schedule.Type?.ToLower() switch
        {
            "once" => BuildOnceTrigger(triggerName, group, schedule),
            "cron" => BuildCronTrigger(triggerName, group, schedule),
            "interval" => BuildIntervalTrigger(triggerName, group, schedule),
            _ => throw new ArgumentException($"Unknown schedule type: {schedule.Type}")
        } ?? throw new ArgumentException("Schedule type is required");
    }

    private ITrigger BuildOnceTrigger(string name, string group, ScheduleDto schedule)
    {
        var triggerBuilder = TriggerBuilder.Create()
            .WithIdentity(name, group);

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
            .WithSimpleSchedule(x => x.WithRepeatCount(0))
            .Build();
    }

    private ITrigger BuildCronTrigger(string name, string group, ScheduleDto schedule)
    {
        if (string.IsNullOrWhiteSpace(schedule.CronExpression))
        {
            throw new ArgumentException("Cron expression is required for cron schedule");
        }

        return TriggerBuilder.Create()
            .WithIdentity(name, group)
            .WithCronSchedule(schedule.CronExpression, x =>
            {
                // 可以根据 Misfire 策略配置
                x.WithMisfireHandlingInstructionFireAndProceed();
            })
            .Build();
    }

    private ITrigger BuildIntervalTrigger(string name, string group, ScheduleDto schedule)
    {
        if (!schedule.IntervalSeconds.HasValue || schedule.IntervalSeconds.Value <= 0)
        {
            throw new ArgumentException("IntervalSeconds is required and must be positive for interval schedule");
        }

        return TriggerBuilder.Create()
            .WithIdentity(name, group)
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

    /// <summary>
    /// 解析 JobKey
    /// </summary>
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

        // 没有 group 分隔符，使用默认 group
        return (jobKey, "default");
    }
}
