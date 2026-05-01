namespace MinGo.Qap.Shared.Models;

/// <summary>
/// Scheduler 信息上报请求
/// </summary>
public class SchedulerReportRequest
{
    /// <summary>
    /// Scheduler 列表
    /// </summary>
    public List<SchedulerInfoDto> Schedulers { get; set; } = new();
}

/// <summary>
/// Scheduler 信息 DTO
/// </summary>
public class SchedulerInfoDto
{
    /// <summary>
    /// Scheduler 名称
    /// </summary>
    public string SchedulerName { get; set; } = string.Empty;

    /// <summary>
    /// Scheduler 实例 ID
    /// </summary>
    public string? SchedulerInstanceId { get; set; }

    /// <summary>
    /// 状态: running, standby
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 是否集群模式
    /// </summary>
    public bool IsClustered { get; set; }

    /// <summary>
    /// JobStore 类型
    /// </summary>
    public string? JobStoreType { get; set; }

    /// <summary>
    /// 线程池类型
    /// </summary>
    public string? ThreadPoolType { get; set; }

    /// <summary>
    /// 线程池大小
    /// </summary>
    public int ThreadPoolSize { get; set; }

    /// <summary>
    /// 运行开始时间（UTC）
    /// </summary>
    public DateTimeOffset? RunningSince { get; set; }

    /// <summary>
    /// Quartz 版本
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// 已执行 Job 数量
    /// </summary>
    public int NumberOfJobsExecuted { get; set; }

    /// <summary>
    /// Job 数量统计
    /// </summary>
    public JobCountsDto? JobCounts { get; set; }

    /// <summary>
    /// 扩展属性
    /// </summary>
    public Dictionary<string, string>? Properties { get; set; }
}

/// <summary>
/// Scheduler 简要信息（列表展示用）
/// </summary>
public class SchedulerSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string SchedulerName { get; set; } = string.Empty;
    public string? SchedulerInstanceId { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsClustered { get; set; }
    public DateTimeOffset? RunningSince { get; set; }
    public DateTimeOffset LastReportedAt { get; set; }
    public int AgentCount { get; set; }
}

/// <summary>
/// Agent 关联的 Scheduler DTO
/// </summary>
public class AgentSchedulerDto
{
    public string SchedulerInfoId { get; set; } = string.Empty;
    public string SchedulerName { get; set; } = string.Empty;
    public string? SchedulerInstanceId { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsClustered { get; set; }
    public DateTimeOffset? RunningSince { get; set; }
    public DateTimeOffset ReportedAt { get; set; }
}

/// <summary>
/// Scheduler 详情响应
/// </summary>
public class SchedulerDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string SchedulerName { get; set; } = string.Empty;
    public string? SchedulerInstanceId { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsClustered { get; set; }
    public string? JobStoreType { get; set; }
    public string? ThreadPoolType { get; set; }
    public int ThreadPoolSize { get; set; }
    public DateTimeOffset? RunningSince { get; set; }
    public string? Version { get; set; }
    public int NumberOfJobsExecuted { get; set; }
    public JobCountsDto? JobCounts { get; set; }
    public Dictionary<string, string>? Properties { get; set; }
    public DateTimeOffset FirstReportedAt { get; set; }
    public DateTimeOffset LastReportedAt { get; set; }
    public List<SchedulerAgentDto> Agents { get; set; } = new();
}

/// <summary>
/// 关联到 Scheduler 的 Agent DTO
/// </summary>
public class SchedulerAgentDto
{
    public string AgentId { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public string AgentUrl { get; set; } = string.Empty;
    public string AgentStatus { get; set; } = string.Empty;
    public DateTimeOffset ReportedAt { get; set; }
}

/// <summary>
/// Agent 身份信息
/// </summary>
public class AgentIdentity
{
    /// <summary>
    /// Agent ID（由 Platform 分配）
    /// </summary>
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// 注册时间（UTC）
    /// </summary>
    public DateTimeOffset RegisteredAt { get; set; }

    /// <summary>
    /// 最后更新时间（UTC）
    /// </summary>
    public DateTimeOffset LastUpdatedAt { get; set; }
}

/// <summary>
/// Agent 详情响应
/// </summary>
public class AgentDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? AgentVersion { get; set; }
    public DateTimeOffset? LastHeartbeat { get; set; }
    public DateTimeOffset? LastReportedAt { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<AgentSchedulerDto> Schedulers { get; set; } = new();
}
