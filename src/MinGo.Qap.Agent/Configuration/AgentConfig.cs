namespace MinGo.Qap.Agent.Configuration;

/// <summary>
/// Agent 配置根节点
/// </summary>
public class AgentConfig
{
    /// <summary>
    /// Agent 基本配置
    /// </summary>
    public AgentSettings Agent { get; set; } = new();
    
    /// <summary>
    /// Platform 连接配置
    /// </summary>
    public PlatformSettings Platform { get; set; } = new();
    
    /// <summary>
    /// Quartz 调度器配置
    /// </summary>
    public QuartzSettings Quartz { get; set; } = new();
    
    /// <summary>
    /// 日志配置
    /// </summary>
    public LoggingSettings? Logging { get; set; }
}

/// <summary>
/// Agent 基本设置
/// </summary>
public class AgentSettings
{
    /// <summary>
    /// Agent 实例 ID（可选，默认自动生成）
    /// </summary>
    public string? Id { get; set; }
    
    /// <summary>
    /// 所属 Cluster ID（必填）
    /// </summary>
    public string ClusterId { get; set; } = string.Empty;
    
    /// <summary>
    /// HTTP 监听端口（默认 8080）
    /// </summary>
    public int Port { get; set; } = 8080;
    
    /// <summary>
    /// 心跳间隔（秒，默认 30）
    /// </summary>
    public int HeartbeatIntervalSeconds { get; set; } = 30;
    
    /// <summary>
    /// 注册最大尝试次数（默认 5）
    /// </summary>
    public int RegistrationMaxAttempts { get; set; } = 5;
    
    /// <summary>
    /// 注册重试延迟（秒，默认 5）
    /// </summary>
    public int RegistrationRetryDelaySeconds { get; set; } = 5;
    
    /// <summary>
    /// 是否启用集群模式（默认 false）
    /// </summary>
    public bool ClusterMode { get; set; } = false;
}

/// <summary>
/// Platform 连接设置
/// </summary>
public class PlatformSettings
{
    /// <summary>
    /// Platform API URL（必填）
    /// </summary>
    public string Url { get; set; } = string.Empty;
    
    /// <summary>
    /// API Token（用于认证）
    /// </summary>
    public string ApiToken { get; set; } = string.Empty;
}

/// <summary>
/// Quartz 调度器设置
/// </summary>
public class QuartzSettings
{
    /// <summary>
    /// Job 程序集路径（目录或文件）
    /// </summary>
    public string AssemblyPath { get; set; } = string.Empty;
    
    /// <summary>
    /// 预定义的 Job 类型列表（完整类名）
    /// </summary>
    public List<string> JobTypes { get; set; } = new();
    
    /// <summary>
    /// Quartz 属性配置（直接映射到 quartz.properties）
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new();
}

/// <summary>
/// 日志设置
/// </summary>
public class LoggingSettings
{
    /// <summary>
    /// 日志级别: Debug, Information, Warning, Error
    /// </summary>
    public string Level { get; set; } = "Information";
    
    /// <summary>
    /// 日志输出路径（可选，默认控制台）
    /// </summary>
    public string? OutputPath { get; set; }
}
