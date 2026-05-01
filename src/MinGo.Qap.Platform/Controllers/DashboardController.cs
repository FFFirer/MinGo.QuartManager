using Microsoft.AspNetCore.Mvc;
using MinGo.Qap.Platform.Services;
using MinGo.Qap.Shared.Models;

namespace MinGo.Qap.Platform.Controllers;

[ApiController]
[Route("api")]
public class DashboardController : ControllerBase
{
    private readonly AgentService _agentService;
    private readonly SchedulerService _schedulerService;
    private readonly IJobService _jobService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        AgentService agentService,
        SchedulerService schedulerService,
        IJobService jobService,
        ILogger<DashboardController> logger)
    {
        _agentService = agentService;
        _schedulerService = schedulerService;
        _jobService = jobService;
        _logger = logger;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<DashboardDto>>> GetPlatformDashboard()
    {
        try
        {
            var agents = await _agentService.GetAllAsync();
            var schedulers = await _schedulerService.GetAllSchedulersAsync();

            var dashboard = new DashboardDto
            {
                TotalClusters = 0, // Cluster concept removed
                TotalJobs = 0, // Would need job aggregation
                TotalAgents = agents.Count,
                OnlineAgents = agents.Count(a => a.Status == "Online"),
                WarningAgents = agents.Count(a => a.Status == "Warning"),
                OfflineAgents = agents.Count(a => a.Status == "Offline"),
                Clusters = new List<ClusterSummaryItem>(),
                JobStatus = new JobStatusDistribution
                {
                    Active = 0,
                    Paused = 0,
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
        // Cluster dashboard is deprecated - return empty response
        var dashboard = new ClusterDashboardDto
        {
            ClusterId = clusterId,
            ClusterName = clusterId,
            Status = "Deprecated",
            Env = "N/A",
            CreatedAt = DateTime.MinValue,
            JobSummary = new JobSummary
            {
                Total = 0,
                Active = 0,
                Paused = 0,
                Blocked = 0,
                Executing = 0
            },
            AgentSummary = new AgentSummary
            {
                Total = 0,
                Online = 0,
                Warning = 0,
                Offline = 0
            },
            RecentAgents = new List<AgentInstanceDto>(),
            UpcomingJobs = new List<UpcomingJobDto>(),
            LastUpdated = DateTime.UtcNow
        };

        return Ok(ApiResponse<ClusterDashboardDto>.Ok(dashboard));
    }

    [HttpGet("clusters/{clusterId}/calendar")]
    public async Task<ActionResult<ApiResponse<CalendarDto>>> GetClusterCalendar(
        string clusterId,
        [FromQuery] int year,
        [FromQuery] int month)
    {
        // Cluster calendar is deprecated - return empty response
        year = year == 0 ? DateTime.Now.Year : year;
        month = month == 0 ? DateTime.Now.Month : month;

        var calendar = new CalendarDto
        {
            Year = year,
            Month = month,
            Jobs = new List<CalendarJobDto>()
        };

        return Ok(ApiResponse<CalendarDto>.Ok(calendar));
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
