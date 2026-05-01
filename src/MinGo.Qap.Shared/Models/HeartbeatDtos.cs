namespace MinGo.Qap.Shared.Models;

/// <summary>
/// Agent 心跳数据
/// </summary>
public class HeartbeatDto
{
    /// <summary>
    /// 心跳时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Agent 版本
    /// </summary>
    public string AgentVersion { get; set; } = string.Empty;
    
    /// <summary>
    /// 运行时间（秒）
    /// </summary>
    public long UptimeSeconds { get; set; }
    
    /// <summary>
    /// Scheduler 状态: running, standby, shutdown
    /// </summary>
    public string SchedulerStatus { get; set; } = string.Empty;
    
    /// <summary>
    /// Job 统计
    /// </summary>
    public JobCountsDto Jobs { get; set; } = new();
    
    /// <summary>
    /// 系统指标
    /// </summary>
    public SystemMetricsDto System { get; set; } = new();
}

/// <summary>
/// Job 数量统计
/// </summary>
public class JobCountsDto
{
    public int Total { get; set; }
    public int TotalJobs { get; set; }
    public int Normal { get; set; }
    public int RunningJobs { get; set; }
    public int Paused { get; set; }
    public int PausedJobs { get; set; }
    public int Blocked { get; set; }
    public int BlockedJobs { get; set; }
    public int Executing { get; set; }
    public int WaitingJobs { get; set; }
}

/// <summary>
/// 系统指标
/// </summary>
public class SystemMetricsDto
{
    /// <summary>
    /// 内存使用（MB）
    /// </summary>
    public long MemoryUsedMb { get; set; }
    
    /// <summary>
    /// 总内存（MB）
    /// </summary>
    public long MemoryTotalMb { get; set; }
    
    /// <summary>
    /// CPU 使用率（百分比）
    /// </summary>
    public double CpuPercent { get; set; }
}
