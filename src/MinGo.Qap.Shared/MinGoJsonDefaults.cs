using System.Text.Json;

namespace MinGo.Qap.Shared;

/// <summary>
/// 共享的 JSON 序列化配置。
/// 使用 Web 默认配置（PropertyNamingPolicy = CamelCase, PropertyNameCaseInsensitive = true），
/// 确保 Agent 与 Platform 之间的 HTTP 通信使用一致的 CamelCase 命名策略。
/// </summary>
public static class MinGoJsonDefaults
{
    /// <summary>
    /// 共享的 JsonSerializerOptions 实例，使用 CamelCase 命名策略。
    /// </summary>
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}
