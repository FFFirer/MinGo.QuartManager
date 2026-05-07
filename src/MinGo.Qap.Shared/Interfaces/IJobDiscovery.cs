using MinGo.Qap.Shared.Models;

namespace MinGo.Qap.Shared.Interfaces;

/// <summary>
/// 作业发现接口
/// </summary>
public interface IJobDiscovery
{
    /// <summary>
    /// 发现程序集中的作业类型
    /// </summary>
    Task<IEnumerable<DiscoveredJobInfo>> DiscoverJobsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 从指定程序集发现作业
    /// </summary>
    Task<IEnumerable<DiscoveredJobInfo>> DiscoverJobsFromAssemblyAsync(string assemblyPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// 从指定程序集名称发现作业
    /// </summary>
    Task<IEnumerable<DiscoveredJobInfo>> DiscoverJobsFromAssemblyNameAsync(string assemblyName, CancellationToken cancellationToken = default);
}

/// <summary>
/// 发现的作业信息
/// </summary>
public record DiscoveredJobInfo(
    string JobKey,
    JobTypeQualifiedName JobTypeQualifiedName,
    string? Description,
    List<ParameterInfoDto>? Parameters,
    ScheduleInfo? Schedule
);

/// <summary>
/// 调度信息
/// </summary>
public record ScheduleInfo(
    ScheduleType Type,
    string? CronExpression,
    TimeSpan? Interval,
    DateTimeOffset? StartTime,
    DateTimeOffset? EndTime
);

/// <summary>
/// 调度类型
/// </summary>
public enum ScheduleType
{
    Cron = 1,
    Interval = 2,
    Once = 3
}