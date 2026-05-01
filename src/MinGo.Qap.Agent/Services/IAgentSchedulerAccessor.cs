using Quartz;

namespace MinGo.Qap.Agent.Services;

/// <summary>
/// Agent 端访问宿主程序中所有 IScheduler 的接口。
/// 命名避免与 Quartz.NET 内部的 ISchedulerRepository 冲突。
/// </summary>
public interface IAgentSchedulerAccessor
{
    /// <summary>
    /// 获取所有已注册的 Scheduler
    /// </summary>
    /// <returns>Scheduler 名称到 IScheduler 实例的字典</returns>
    IReadOnlyDictionary<string, IScheduler> GetAll();

    /// <summary>
    /// 按名称获取 Scheduler
    /// </summary>
    /// <param name="schedulerName">Scheduler 名称</param>
    /// <returns>IScheduler 实例，如果不存在则返回 null</returns>
    IScheduler? GetScheduler(string schedulerName);

    /// <summary>
    /// Scheduler 数量
    /// </summary>
    int Count { get; }
}
