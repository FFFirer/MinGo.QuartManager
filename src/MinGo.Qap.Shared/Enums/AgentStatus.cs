namespace MinGo.Qap.Shared.Enums;

/// <summary>
/// Agent 实例状态
/// </summary>
public enum AgentStatus
{
    /// <summary>
    /// 刚注册，等待首次心跳
    /// </summary>
    Pending,
    
    /// <summary>
    /// 正常在线
    /// </summary>
    Online,
    
    /// <summary>
    /// 心跳延迟（Warning）
    /// </summary>
    Warning,
    
    /// <summary>
    /// 已离线
    /// </summary>
    Offline,
    
    /// <summary>
    /// 已删除（软删除）
    /// </summary>
    Deleted
}