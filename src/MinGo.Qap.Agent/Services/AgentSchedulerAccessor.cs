using Quartz;

namespace MinGo.Qap.Agent.Services;

/// <summary>
/// IAgentSchedulerAccessor 的默认实现
/// </summary>
public class AgentSchedulerAccessor : IAgentSchedulerAccessor
{
    private readonly IReadOnlyDictionary<string, IScheduler> _schedulers;

    /// <summary>
    /// 从 Scheduler 字典创建
    /// </summary>
    public AgentSchedulerAccessor(IReadOnlyDictionary<string, IScheduler> schedulers)
    {
        _schedulers = schedulers ?? throw new ArgumentNullException(nameof(schedulers));
    }

    /// <summary>
    /// 从 Scheduler 列表创建
    /// </summary>
    public AgentSchedulerAccessor(IEnumerable<IScheduler> schedulers)
    {
        if (schedulers == null)
            throw new ArgumentNullException(nameof(schedulers));

        _schedulers = schedulers.ToDictionary(s => s.SchedulerName, s => s);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IScheduler> GetAll() => _schedulers;

    /// <inheritdoc />
    public IScheduler? GetScheduler(string schedulerName)
    {
        if (string.IsNullOrEmpty(schedulerName))
            return null;

        _schedulers.TryGetValue(schedulerName, out var scheduler);
        return scheduler;
    }

    /// <inheritdoc />
    public int Count => _schedulers.Count;
}
