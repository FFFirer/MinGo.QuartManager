using System.Diagnostics.CodeAnalysis;
using MinGo.Qap.Shared.Models;

namespace MinGo.Qap.Platform.Caching;

/// <summary>
/// Manifest 缓存服务接口。
/// 用于缓存 Agent 上报的 Job Manifest，避免重复请求 Agent。
/// 实现需保证线程安全。
/// </summary>
public interface IManifestCacheService
{
    /// <summary>
    /// 获取缓存的 Manifest。
    /// </summary>
    /// <param name="schedulerName">Scheduler 名称</param>
    /// <param name="manifest">缓存的 Manifest（如果存在且未过期）</param>
    /// <returns>缓存命中且未过期返回 true，否则返回 false</returns>
    bool TryGet(string schedulerName, [NotNullWhen(true)] out JobManifestDto? manifest);

    /// <summary>
    /// 设置 Manifest 缓存。
    /// </summary>
    void Set(string schedulerName, JobManifestDto manifest);

    /// <summary>
    /// 清除指定 Scheduler 的 Manifest 缓存（Agent 重连/重启时调用）。
    /// </summary>
    void Invalidate(string schedulerName);

    /// <summary>
    /// 批量清除多个 Scheduler 的 Manifest 缓存。
    /// </summary>
    void InvalidateForSchedulers(IEnumerable<string> schedulerNames);

    /// <summary>
    /// 获取当前缓存条目数（用于 OTel Gauge 指标）。
    /// </summary>
    int Count { get; }
}
