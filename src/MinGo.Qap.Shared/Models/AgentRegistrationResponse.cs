namespace MinGo.Qap.Shared.Models;

/// <summary>
/// Agent 注册响应
/// </summary>
public class AgentRegistrationResponse
{
    /// <summary>
    /// Agent 实例 ID
    /// </summary>
    public string AgentId { get; set; } = string.Empty;
    
    /// <summary>
    /// Quartz 实例 ID
    /// </summary>
    public string QuartzInstanceId { get; set; } = string.Empty;
    
    /// <summary>
    /// 平台 API 端点基础 URL
    /// </summary>
    public string PlatformApiBaseUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// 心跳间隔（秒）
    /// </summary>
    public int HeartbeatIntervalSeconds { get; set; } = 30;
    
    /// <summary>
    /// 警告阈值（秒）
    /// </summary>
    public int WarningThresholdSeconds { get; set; } = 30;
    
    /// <summary>
    /// 离线阈值（秒）
    /// </summary>
    public int OfflineThresholdSeconds { get; set; } = 60;
}