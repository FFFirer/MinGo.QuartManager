using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using MinGo.Qap.Shared;
using MinGo.Qap.Shared.Models;

namespace MinGo.Qap.Platform.Caching;

/// <summary>
/// Manifest 缓存服务。
/// 使用 ConcurrentDictionary 保证线程安全，支持可配置 TTL 过期。
/// </summary>
public class ManifestCacheService : IManifestCacheService
{
    private readonly ConcurrentDictionary<string, ManifestCacheEntry> _cache = new();
    private readonly TimeSpan _cacheTtl;
    private readonly ILogger<ManifestCacheService> _logger;

    public ManifestCacheService(IOptions<ManifestCacheOptions> options, ILogger<ManifestCacheService> logger)
    {
        _cacheTtl = TimeSpan.FromSeconds(options.Value.TtlSeconds);
        _logger = logger;
    }

    public bool TryGet(string schedulerName, out JobManifestDto? manifest)
    {
        if (_cache.TryGetValue(schedulerName, out var entry))
        {
            if (DateTimeOffset.UtcNow - entry.CachedAt < _cacheTtl)
            {
                manifest = entry.Manifest;
                QapTelemetry.CacheHits.Add(1,
                    new KeyValuePair<string, object?>("scheduler.name", schedulerName));
                return true;
            }

            // 缓存过期，移除
            _cache.TryRemove(schedulerName, out _);
        }

        QapTelemetry.CacheMisses.Add(1,
            new KeyValuePair<string, object?>("scheduler.name", schedulerName));
        manifest = null;
        return false;
    }

    public void Set(string schedulerName, JobManifestDto manifest)
    {
        _cache[schedulerName] = new ManifestCacheEntry(manifest, DateTimeOffset.UtcNow);
    }

    public void Invalidate(string schedulerName)
    {
        if (!string.IsNullOrWhiteSpace(schedulerName))
        {
            _cache.TryRemove(schedulerName, out _);
            QapTelemetry.CacheInvalidations.Add(1,
                new KeyValuePair<string, object?>("reason", "explicit"));
            _logger.LogDebug("Manifest cache invalidated for scheduler {SchedulerName}", schedulerName);
        }
    }

    public void InvalidateForSchedulers(IEnumerable<string> schedulerNames)
    {
        if (schedulerNames == null) return;

        var count = 0;
        foreach (var name in schedulerNames)
        {
            if (!string.IsNullOrWhiteSpace(name) && _cache.TryRemove(name, out _))
            {
                count++;
            }
        }

        if (count > 0)
        {
            QapTelemetry.CacheInvalidations.Add(count,
                new KeyValuePair<string, object?>("reason", "scheduler_report"));
            _logger.LogDebug("Manifest cache invalidated for {Count} schedulers", count);
        }
    }

    /// <inheritdoc />
    public int Count => _cache.Count;

    private record ManifestCacheEntry(JobManifestDto Manifest, DateTimeOffset CachedAt);
}
