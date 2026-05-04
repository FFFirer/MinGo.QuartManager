using System.Collections.Concurrent;
using Quartz;

namespace MinGo.Qap.Agent.Services;

/// <summary>
/// 延迟发现的 Scheduler Accessor
/// 用于解决 Agent 启动时 Scheduler 尚未就绪的问题
/// </summary>
public class DeferredSchedulerAccessor : IAgentSchedulerAccessor
{
    private readonly IServiceProvider _serviceProvider;
    private IReadOnlyDictionary<string, IScheduler>? _cachedSchedulers;
    private int _discoveryAttempts;
    private readonly ConcurrentDictionary<string, IScheduler> _emptyDict = new();
    private readonly ILogger _logger;

    // 重试配置
    private const int MaxAttempts = 10;
    private const int RetryIntervalMs = 500;

    public DeferredSchedulerAccessor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = serviceProvider.GetService<ILogger<DeferredSchedulerAccessor>>() ?? throw new InvalidOperationException("Logger not available");
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IScheduler> GetAll()
    {
        // 如果已缓存，直接返回
        if (_cachedSchedulers != null)
            return _cachedSchedulers;

        // 尝试发现 Scheduler
        var schedulers = TryDiscoverSchedulers();
        if (schedulers.Count > 0)
        {
            _cachedSchedulers = schedulers;
            return _cachedSchedulers;
        }

        // 返回空字典，下次调用会重试
        return _emptyDict;
    }

    /// <inheritdoc />
    public IScheduler? GetScheduler(string schedulerName)
    {
        var all = GetAll();
        all.TryGetValue(schedulerName, out var scheduler);
        return scheduler;
    }

    /// <inheritdoc />
    public int Count => GetAll().Count;

    /// <summary>
    /// 强制刷新 Scheduler 列表
    /// </summary>
    public void Refresh()
    {
        _cachedSchedulers = null;
        _discoveryAttempts = 0;
    }

    private IReadOnlyDictionary<string, IScheduler> TryDiscoverSchedulers()
    {
        // 如果超过最大重试次数，返回空
        if (_discoveryAttempts >= MaxAttempts)
            return _emptyDict;

        _discoveryAttempts++;

        try
        {
            var schedulerFactory = _serviceProvider.GetService<ISchedulerFactory>();
            var schedulers = schedulerFactory?.GetAllSchedulers().GetAwaiter().GetResult();
            if (schedulers != null && schedulers.Count > 0)
            {
                Dictionary<string, IScheduler> schedulerIndex = schedulers.ToDictionary(x => x.SchedulerName, x => x);
                return schedulerIndex;
            }
            // // 尝试获取多个 Scheduler
            // var schedulers = _serviceProvider.GetServices<IScheduler>()?.ToList();
            // if (schedulers is { Count: > 0 })
            // {
            //     return schedulers.ToDictionary(s => s.SchedulerName, s => s);
            // }

            // // 尝试获取单个 Scheduler
            // var singleScheduler = _serviceProvider.GetService<IScheduler>();
            // if (singleScheduler != null)
            // {
            //     return new Dictionary<string, IScheduler>
            //     {
            //         [singleScheduler.SchedulerName] = singleScheduler
            //     };
            // }
        }
        catch (Exception ex)
        {
            // 发现失败，将在下次调用时重试
            _logger.LogWarning(ex, "Attempt {Attempt} to discover schedulers failed", _discoveryAttempts);
        }

        // 如果需要重试，短暂等待
        if (_discoveryAttempts < MaxAttempts)
        {
            Thread.Sleep(RetryIntervalMs);
            return TryDiscoverSchedulers();
        }

        return _emptyDict;
    }
}
