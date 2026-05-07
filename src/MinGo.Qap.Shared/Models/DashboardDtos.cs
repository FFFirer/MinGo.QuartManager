namespace MinGo.Qap.Shared.Models;

public class DashboardDto
{
    public int TotalJobs { get; set; }
    public int TotalAgents { get; set; }
    public int OnlineAgents { get; set; }
    public int WarningAgents { get; set; }
    public int OfflineAgents { get; set; }
    public List<UpcomingJobDto> UpcomingJobs { get; set; } = new();
    public JobStatusDistribution JobStatus { get; set; } = new();
    public DateTimeOffset LastUpdated { get; set; }
}

public class UpcomingJobDto
{
    public string JobKey { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public string ScheduleDescription { get; set; } = string.Empty;
    public DateTimeOffset NextFireTime { get; set; }
}

public class JobStatusDistribution
{
    public int Active { get; set; }
    public int Paused { get; set; }
    public int Blocked { get; set; }
    public int Executing { get; set; }
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