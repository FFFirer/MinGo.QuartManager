namespace MinGo.Qap.Shared.Models;

/// <summary>
/// Agent 心跳请求
/// </summary>
public class AgentHeartbeatRequest
{
    /// <summary>
    /// Agent 实例 ID
    /// </summary>
    public string AgentId { get; set; } = string.Empty;
    
    /// <summary>
    /// Quartz 实例 ID
    /// </summary>
    public string? QuartzInstanceId { get; set; }
    
    /// <summary>
    /// Agent 版本
    /// </summary>
    public string? AgentVersion { get; set; }
    
    /// <summary>
    /// 当前运行状态
    /// </summary>
    public string? Status { get; set; }
    
    /// <summary>
    /// 运行指标（JSON）
    /// </summary>
    public string? Metrics { get; set; } = "{}";
}

/// <summary>
/// Agent 心跳响应
/// </summary>
public class AgentHeartbeatResponse
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// 消息
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// 下次心跳间隔（秒）
    /// </summary>
    public int NextHeartbeatIntervalSeconds { get; set; } = 30;
    
    /// <summary>
    /// 需要执行的命令（可选）
    /// </summary>
    public string? Command { get; set; }
    
    /// <summary>
    /// 命令参数（JSON）
    /// </summary>
    public string? CommandArgs { get; set; } = "{}";
}