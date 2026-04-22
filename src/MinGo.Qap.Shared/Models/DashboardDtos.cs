namespace MinGo.Qap.Shared.Models;

public class DashboardDto
{
    public int TotalClusters { get; set; }
    public int TotalJobs { get; set; }
    public int TotalAgents { get; set; }
    public int OnlineAgents { get; set; }
    public int WarningAgents { get; set; }
    public int OfflineAgents { get; set; }
    public List<ClusterSummaryItem> Clusters { get; set; } = new();
    public List<UpcomingJobDto> UpcomingJobs { get; set; } = new();
    public JobStatusDistribution JobStatus { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class ClusterSummaryItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Env { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int JobCount { get; set; }
    public int AgentCount { get; set; }
    public int OnlineAgentCount { get; set; }
    public string? LastHeartbeat { get; set; }
}

public class UpcomingJobDto
{
    public string JobKey { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public string ClusterId { get; set; } = string.Empty;
    public string ClusterName { get; set; } = string.Empty;
    public string ScheduleDescription { get; set; } = string.Empty;
    public DateTime NextFireTime { get; set; }
}

public class JobStatusDistribution
{
    public int Active { get; set; }
    public int Paused { get; set; }
    public int Blocked { get; set; }
    public int Executing { get; set; }
}

public class ClusterDashboardDto
{
    public string ClusterId { get; set; } = string.Empty;
    public string ClusterName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Env { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public JobSummary JobSummary { get; set; } = new();
    public AgentSummary AgentSummary { get; set; } = new();
    public List<AgentInstanceDto> RecentAgents { get; set; } = new();
    public List<UpcomingJobDto> UpcomingJobs { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class JobSummary
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Paused { get; set; }
    public int Blocked { get; set; }
    public int Executing { get; set; }
}

public class AgentSummary
{
    public int Total { get; set; }
    public int Online { get; set; }
    public int Warning { get; set; }
    public int Offline { get; set; }
}

public class CalendarDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public List<CalendarJobDto> Jobs { get; set; } = new();
}

public class CalendarJobDto
{
    public string JobKey { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public string ScheduleType { get; set; } = string.Empty;
    public string? CronExpression { get; set; }
    public string ScheduleDescription { get; set; } = string.Empty;
    public List<DateTime> FireTimes { get; set; } = new();
}