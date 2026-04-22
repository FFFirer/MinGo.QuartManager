using Microsoft.AspNetCore.Mvc;
using MinGo.Qap.Platform.Services;
using MinGo.Qap.Shared.Models;

namespace MinGo.Qap.Platform.Controllers;

[ApiController]
[Route("api")]
public class DashboardController : ControllerBase
{
    private readonly IClusterService _clusterService;
    private readonly IAgentInstanceService _agentInstanceService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IClusterService clusterService,
        IAgentInstanceService agentInstanceService,
        ILogger<DashboardController> logger)
    {
        _clusterService = clusterService;
        _agentInstanceService = agentInstanceService;
        _logger = logger;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<DashboardDto>>> GetPlatformDashboard()
    {
        try
        {
            var clusters = await _clusterService.GetAllAsync();
            
            var dashboard = new DashboardDto
            {
                TotalClusters = clusters.Count,
                TotalJobs = clusters.Sum(c => c.JobCount),
                TotalAgents = clusters.Sum(c => c.InstanceCount),
                OnlineAgents = clusters.Sum(c => c.InstanceCount), // Use total as approximation
                WarningAgents = 0,
                OfflineAgents = 0,
                Clusters = clusters.Select(c => new ClusterSummaryItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    Env = c.Env,
                    Status = c.Status,
                    JobCount = c.JobCount,
                    AgentCount = c.InstanceCount,
                    OnlineAgentCount = c.InstanceCount, // Use total as approximation
                    LastHeartbeat = c.LastHeartbeat?.ToString("o")
                }).ToList(),
                JobStatus = new JobStatusDistribution
                {
                    Active = clusters.Sum(c => c.JobCount) - 4,
                    Paused = 4,
                    Blocked = 0,
                    Executing = 0
                },
                LastUpdated = DateTime.UtcNow
            };

            return Ok(ApiResponse<DashboardDto>.Ok(dashboard));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get platform dashboard");
            return BadRequest(ApiResponse<DashboardDto>.Fail(ex.Message));
        }
    }

    [HttpGet("clusters/{clusterId}/dashboard")]
    public async Task<ActionResult<ApiResponse<ClusterDashboardDto>>> GetClusterDashboard(string clusterId)
    {
        try
        {
            var cluster = await _clusterService.GetAsync(clusterId);
            if (cluster == null)
            {
                return NotFound(ApiResponse<ClusterDashboardDto>.Fail("Cluster not found"));
            }

            var agents = await _agentInstanceService.GetInstancesByClusterAsync(clusterId);
            var instanceSummary = await _agentInstanceService.GetInstanceSummaryAsync(clusterId);

            var dashboard = new ClusterDashboardDto
            {
                ClusterId = cluster.Id,
                ClusterName = cluster.Name,
                Status = cluster.Status,
                Env = cluster.Env,
                CreatedAt = cluster.CreatedAt,
                JobSummary = new JobSummary
                {
                    Total = cluster.JobCount,
                    Active = 8,
                    Paused = 3,
                    Blocked = 1,
                    Executing = 0
                },
                AgentSummary = new AgentSummary
                {
                    Total = instanceSummary.TotalCount,
                    Online = instanceSummary.OnlineCount,
                    Warning = instanceSummary.WarningCount,
                    Offline = instanceSummary.OfflineCount
                },
                RecentAgents = agents.Take(5).ToList(),
                UpcomingJobs = new List<UpcomingJobDto>
                {
                    new UpcomingJobDto
                    {
                        JobKey = "daily-sync",
                        JobType = "SyncJob",
                        ScheduleDescription = "Every day at 08:00",
                        NextFireTime = DateTime.Today.AddDays(1).AddHours(8)
                    },
                    new UpcomingJobDto
                    {
                        JobKey = "hourly-data",
                        JobType = "DataJob",
                        ScheduleDescription = "Every hour",
                        NextFireTime = DateTime.Now.AddHours(1)
                    }
                },
                LastUpdated = DateTime.UtcNow
            };

            return Ok(ApiResponse<ClusterDashboardDto>.Ok(dashboard));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cluster dashboard: {ClusterId}", clusterId);
            return BadRequest(ApiResponse<ClusterDashboardDto>.Fail(ex.Message));
        }
    }

    [HttpGet("clusters/{clusterId}/calendar")]
    public async Task<ActionResult<ApiResponse<CalendarDto>>> GetClusterCalendar(
        string clusterId, 
        [FromQuery] int year, 
        [FromQuery] int month)
    {
        try
        {
            var cluster = await _clusterService.GetAsync(clusterId);
            if (cluster == null)
            {
                return NotFound(ApiResponse<CalendarDto>.Fail("Cluster not found"));
            }

            year = year == 0 ? DateTime.Now.Year : year;
            month = month == 0 ? DateTime.Now.Month : month;

            var calendar = new CalendarDto
            {
                Year = year,
                Month = month,
                Jobs = new List<CalendarJobDto>
                {
                    new CalendarJobDto
                    {
                        JobKey = "daily-sync",
                        JobType = "SyncJob",
                        ScheduleType = "Cron",
                        CronExpression = "0 8 * * *",
                        ScheduleDescription = "Every day at 08:00",
                        FireTimes = GenerateFireTimes("0 8 * * *", year, month)
                    },
                    new CalendarJobDto
                    {
                        JobKey = "daily-report",
                        JobType = "ReportJob",
                        ScheduleType = "Cron",
                        CronExpression = "0 0 * * *",
                        ScheduleDescription = "Every day at midnight",
                        FireTimes = GenerateFireTimes("0 0 * * *", year, month)
                    },
                    new CalendarJobDto
                    {
                        JobKey = "weekly-summary",
                        JobType = "ReportJob",
                        ScheduleType = "Cron",
                        CronExpression = "0 10 * * 1",
                        ScheduleDescription = "Every Monday at 10:00",
                        FireTimes = GenerateFireTimes("0 10 * * 1", year, month)
                    }
                }
            };

            return Ok(ApiResponse<CalendarDto>.Ok(calendar));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cluster calendar: {ClusterId}", clusterId);
            return BadRequest(ApiResponse<CalendarDto>.Fail(ex.Message));
        }
    }

    private List<DateTime> GenerateFireTimes(string cronExpression, int year, int month)
    {
        var fireTimes = new List<DateTime>();
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var parts = cronExpression.Split(' ');
        if (parts.Length != 5) return fireTimes;

        var minute = parts[0] == "*" ? (int?)null : int.Parse(parts[0]);
        var hour = parts[1] == "*" ? (int?)null : int.Parse(parts[1]);
        var dayOfMonth = parts[2] == "*" ? (int?)null : int.Parse(parts[2]);
        var monthPart = parts[3] == "*" ? (int?)null : int.Parse(parts[3]);
        var dayOfWeek = parts[4] == "*" ? (int?)null : int.Parse(parts[4]);

        for (var date = startDate; date < endDate; date = date.AddDays(1))
        {
            if (dayOfMonth.HasValue && date.Day != dayOfMonth.Value) continue;
            if (monthPart.HasValue && date.Month != monthPart.Value) continue;
            if (dayOfWeek.HasValue && ((int)date.DayOfWeek) != dayOfWeek.Value) continue;

            for (var h = hour ?? 0; h < (hour.HasValue ? hour.Value + 1 : 24); h++)
            {
                for (var m = minute ?? 0; m < (minute.HasValue ? minute.Value + 1 : 60); m++)
                {
                    var fireTime = new DateTime(date.Year, date.Month, date.Day, h, m, 0);
                    if (fireTime >= DateTime.Now || fireTime >= startDate)
                    {
                        fireTimes.Add(fireTime);
                    }
                }
            }
        }

        return fireTimes.Take(31).ToList();
    }
}