using System.Text.Json;
using MinGo.Qap.Shared.Models;

namespace MinGo.Qap.Shared;

/// <summary>
/// API 响应解包异常 — 表示 API 返回了业务层错误（Success=false）。
/// </summary>
public class ApiResponseException : Exception
{
    /// <summary>
    /// 错误代码，对应 <see cref="Models.ApiResponse{T}.ErrorCode"/>
    /// </summary>
    public string? ErrorCode { get; }

    public ApiResponseException(string message, string? errorCode = null)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// 提供 <see cref="HttpContent"/> 的扩展方法，用于统一解包 <see cref="ApiResponse{T}"/> 包装的 HTTP 响应。
/// </summary>
public static class HttpContentApiResponseExtensions
{
    /// <summary>
    /// 从 HTTP 响应内容中读取并解包 <see cref="ApiResponse{T}"/>，返回 <c>.Data</c>。
    /// 如果响应不是 <c>ApiResponse&lt;T&gt;</c> 格式（比如直接返回裸 JSON），则直接按 <typeparamref name="T"/> 反序列化。
    /// 如果 <c>Success == false</c>，抛出 <see cref="ApiResponseException"/>。
    /// </summary>
    /// <typeparam name="T">目标数据类型</typeparam>
    /// <param name="content">HTTP 响应内容</param>
    /// <param name="options">JSON 序列化选项（默认使用 <see cref="MinGoJsonDefaults.Options"/>）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>解包后的 <c>.Data</c>，或直接反序列化的 <typeparamref name="T"/> 实例</returns>
    /// <exception cref="ApiResponseException">当 <c>Success == false</c> 时抛出</exception>
    public static async Task<T?> ReadFromApiResponseAsync<T>(
        this HttpContent content,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= MinGoJsonDefaults.Options;

        var raw = await content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(raw))
        {
            return default;
        }

        using var doc = JsonDocument.Parse(raw);

        // 探测是否为 ApiResponse<T> 包装格式（顶层有 "success" 和 "data" 字段）
        if (doc.RootElement.TryGetProperty("success", out var successProp) &&
            doc.RootElement.TryGetProperty("data", out var dataProp))
        {
            var success = successProp.GetBoolean();

            if (!success)
            {
                var errorMsg = doc.RootElement.TryGetProperty("errorMessage", out var emProp)
                    ? emProp.GetString()
                    : "Unknown error";
                var errorCode = doc.RootElement.TryGetProperty("errorCode", out var ecProp)
                    ? ecProp.GetString()
                    : null;

                throw new ApiResponseException(errorMsg ?? "Unknown error", errorCode);
            }

            // 成功 → 从 .data 字段反序列化 T
            return JsonSerializer.Deserialize<T>(dataProp.GetRawText(), options);
        }

        // 不是 ApiResponse 包装 → 直接按 T 反序列化（向后兼容）
        return JsonSerializer.Deserialize<T>(raw, options);
    }
}
