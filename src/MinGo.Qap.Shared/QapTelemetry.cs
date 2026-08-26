using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace MinGo.Qap.Shared;

/// <summary>
/// 集中定义 MinGo QAP 的 OpenTelemetry ActivitySource 和 Meter。
/// Platform 和 Agent 共享此定义，确保 OTel 管道可通过统一名称订阅。
/// </summary>
public static class QapTelemetry
{
    /// <summary>
    /// 统一的 ActivitySource 名称，用于创建分布式追踪 Span。
    /// </summary>
    public const string SourceName = "MinGo.Qap";

    /// <summary>
    /// 统一的 Meter 名称，用于创建自定义 Metrics。
    /// </summary>
    public const string MeterName = "MinGo.Qap";

    /// <summary>
    /// 全局 ActivitySource，用于 Platform 和 Agent 创建自定义 Trace Span。
    /// </summary>
    public static readonly ActivitySource ActivitySource = new(SourceName, "1.0.0");

    /// <summary>
    /// 全局 Meter，用于 Platform 和 Agent 创建自定义 Metrics。
    /// </summary>
    public static readonly Meter Meter = new(MeterName, "1.0.0");

    // =========================================================================
    // Platform Metrics
    // =========================================================================

    // --- Counters ---

    /// <summary>Agent 注册总数 (tags: type=new/reconnect)</summary>
    public static readonly Counter<long> AgentRegistrations =
        Meter.CreateCounter<long>("qap.agent.registrations", description: "Total agent registrations");

    /// <summary>心跳处理总数 (tags: agent.id)</summary>
    public static readonly Counter<long> AgentHeartbeats =
        Meter.CreateCounter<long>("qap.agent.heartbeats", description: "Total heartbeats processed");

    /// <summary>代理转发请求总数 (tags: scheduler.name, http.method)</summary>
    public static readonly Counter<long> ProxyRequests =
        Meter.CreateCounter<long>("qap.proxy.requests", description: "Total proxy forwarding requests");

    /// <summary>代理转发失败总数 (tags: scheduler.name, error.code)</summary>
    public static readonly Counter<long> ProxyErrors =
        Meter.CreateCounter<long>("qap.proxy.errors", description: "Total proxy forwarding failures");

    /// <summary>Job 声明总数 (tags: scheduler.name, status=synced/failed)</summary>
    public static readonly Counter<long> JobsDeclared =
        Meter.CreateCounter<long>("qap.jobs.declared", description: "Total job declarations");

    /// <summary>批量操作总数 (tags: action, scheduler.name)</summary>
    public static readonly Counter<long> BatchOperations =
        Meter.CreateCounter<long>("qap.jobs.batch_operations", description: "Total batch operations");

    /// <summary>接收的执行日志条数 (tags: agent.id)</summary>
    public static readonly Counter<long> LogsReceived =
        Meter.CreateCounter<long>("qap.logs.received", description: "Total execution logs received");

    /// <summary>Manifest 缓存命中 (tags: scheduler.name)</summary>
    public static readonly Counter<long> CacheHits =
        Meter.CreateCounter<long>("qap.cache.hits", description: "Manifest cache hits");

    /// <summary>Manifest 缓存未命中 (tags: scheduler.name)</summary>
    public static readonly Counter<long> CacheMisses =
        Meter.CreateCounter<long>("qap.cache.misses", description: "Manifest cache misses");

    /// <summary>Manifest 缓存失效 (tags: reason)</summary>
    public static readonly Counter<long> CacheInvalidations =
        Meter.CreateCounter<long>("qap.cache.invalidations", description: "Manifest cache invalidations");

    // --- Histograms ---

    /// <summary>代理转发延迟 (unit: ms, tags: scheduler.name, http.method)</summary>
    public static readonly Histogram<double> ProxyDuration =
        Meter.CreateHistogram<double>("qap.proxy.duration", "ms", "Proxy forwarding latency");

    /// <summary>Job 声明端到端延迟 (unit: ms, tags: scheduler.name)</summary>
    public static readonly Histogram<double> JobDeclareDuration =
        Meter.CreateHistogram<double>("qap.job.declare.duration", "ms", "Job declaration end-to-end latency");

    /// <summary>Agent 路由选择延迟 (unit: ms, tags: scheduler.name)</summary>
    public static readonly Histogram<double> AgentRouteDuration =
        Meter.CreateHistogram<double>("qap.agent.route.duration", "ms", "Agent routing selection latency");

    /// <summary>每次接收的日志批量大小 (tags: agent.id)</summary>
    public static readonly Histogram<long> LogsBatchSize =
        Meter.CreateHistogram<long>("qap.logs.batch_size", "{count}", "Number of logs per batch");

    // =========================================================================
    // Agent Metrics
    // =========================================================================

    // --- Agent Counters ---

    /// <summary>已发送心跳总数 (tags: agent.id)</summary>
    public static readonly Counter<long> HeartbeatsSent =
        Meter.CreateCounter<long>("qap.heartbeats.sent", description: "Total heartbeats sent");

    /// <summary>心跳失败总数 (tags: agent.id)</summary>
    public static readonly Counter<long> HeartbeatsFailed =
        Meter.CreateCounter<long>("qap.heartbeats.failed", description: "Total heartbeat failures");

    /// <summary>重新注册次数 (tags: agent.id)</summary>
    public static readonly Counter<long> ReRegistrations =
        Meter.CreateCounter<long>("qap.reregistrations", description: "Total re-registrations");

    /// <summary>已刷新到平台的日志总数 (tags: agent.id)</summary>
    public static readonly Counter<long> LogsFlushed =
        Meter.CreateCounter<long>("qap.logs.flushed", description: "Total execution logs flushed to platform");

    /// <summary>日志刷新失败次数 (tags: agent.id)</summary>
    public static readonly Counter<long> LogsFlushFailed =
        Meter.CreateCounter<long>("qap.logs.flush_failed", description: "Total log flush failures");

    // --- Agent Histograms ---

    /// <summary>心跳往返延迟 (unit: ms, tags: agent.id)</summary>
    public static readonly Histogram<double> HeartbeatDuration =
        Meter.CreateHistogram<double>("qap.heartbeat.duration", "ms", "Heartbeat round-trip latency");

    /// <summary>日志刷新往返延迟 (unit: ms, tags: agent.id)</summary>
    public static readonly Histogram<double> LogsFlushDuration =
        Meter.CreateHistogram<double>("qap.logs.flush.duration", "ms", "Log flush round-trip latency");

    // =========================================================================
    // Observable Gauges (需要回调，在各自组件初始化时注册)
    // =========================================================================

    // Platform gauges — 由 Platform 在启动时通过 Meter.CreateObservableGauge 注册
    // Agent gauges — 由 Agent 在启动时通过 Meter.CreateObservableGauge 注册
    // 这些无法在静态类中直接初始化，因为需要 DI 容器提供数据源

    /// <summary>
    /// 辅助方法：启动一个计时 Span，返回 IDisposable，Dispose 时自动记录耗时到指定 Histogram。
    /// </summary>
    public static IDisposable StartTimer(Histogram<double> histogram, KeyValuePair<string, object?>[] tags)
    {
        return new HistogramTimer(histogram, tags);
    }

    /// <summary>
    /// 辅助方法：启动一个计时 Span，无额外 tags。
    /// </summary>
    public static IDisposable StartTimer(Histogram<double> histogram)
    {
        return new HistogramTimer(histogram, Array.Empty<KeyValuePair<string, object?>>());
    }

    private sealed class HistogramTimer : IDisposable
    {
        private readonly Histogram<double> _histogram;
        private readonly KeyValuePair<string, object?>[] _tags;
        private readonly long _startTimestamp;

        public HistogramTimer(Histogram<double> histogram, KeyValuePair<string, object?>[] tags)
        {
            _histogram = histogram;
            _tags = tags;
            _startTimestamp = Stopwatch.GetTimestamp();
        }

        public void Dispose()
        {
            var elapsed = Stopwatch.GetElapsedTime(_startTimestamp);
            _histogram.Record(elapsed.TotalMilliseconds, _tags);
        }
    }
}
