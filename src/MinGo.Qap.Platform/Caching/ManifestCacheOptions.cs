namespace MinGo.Qap.Platform.Caching;

/// <summary>
/// Manifest 缓存配置选项。
/// </summary>
public class ManifestCacheOptions
{
    /// <summary>
    /// 配置节路径
    /// </summary>
    public const string SectionName = "ManifestCache";

    /// <summary>
    /// 缓存 TTL（秒），默认 60 秒。
    /// </summary>
    public int TtlSeconds { get; set; } = 60;
}
