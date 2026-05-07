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
                TotalJobs = 0, // Would need job aggregation
                TotalAgents = agents.Count,
                OnlineAgents = agents.Count(a => a.Status == "Online"),
                WarningAgents = agents.Count(a => a.Status == "Warning"),
                OfflineAgents = agents.Count(a => a.Status == "Offline"),
                JobStatus = new JobStatusDistribution
                {
                    Active = 0,
                    Paused = 0,
                    Blocked = 0,
                    Executing = 0
                },
                LastUpdated = DateTimeOffset.UtcNow
            };

            return Ok(ApiResponse<DashboardDto>.Ok(dashboard));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get platform dashboard");
            return BadRequest(ApiResponse<DashboardDto>.Fail(ex.Message));
        }
    }
}
