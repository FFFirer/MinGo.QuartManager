namespace MinGo.Qap.Shared.Models;

/// <summary>
/// 分页查询请求
/// </summary>
public class PagedQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Job 查询请求
/// </summary>
public class JobQuery : PagedQuery
{
    public string? Status { get; set; }
    public string? Group { get; set; }
    public string? Keyword { get; set; }
}
