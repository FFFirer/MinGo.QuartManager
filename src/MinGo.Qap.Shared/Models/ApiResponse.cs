namespace MinGo.Qap.Shared.Models;

/// <summary>
/// API 统一响应包装
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// 响应数据
    /// </summary>
    public T? Data { get; set; }
    
    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// 错误代码
    /// </summary>
    public string? ErrorCode { get; set; }
    
    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data
        };
    }

    public static ApiResponse<T> Fail(string errorMessage, string? errorCode = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode
        };
    }
}

/// <summary>
/// 分页响应
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (Total + PageSize - 1) / PageSize;
}
