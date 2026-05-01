using Microsoft.AspNetCore.Mvc;
using MinGo.Qap.Platform.Services;
using MinGo.Qap.Shared.Models;

namespace MinGo.Qap.Platform.Controllers;

/// <summary>
/// Scheduler 管理控制器
/// </summary>
[ApiController]
[Route("api/schedulers")]
public class SchedulersController : ControllerBase
{
    private readonly SchedulerService _schedulerService;
    private readonly ILogger<SchedulersController> _logger;

    public SchedulersController(
        SchedulerService schedulerService,
        ILogger<SchedulersController> logger)
    {
        _schedulerService = schedulerService;
        _logger = logger;
    }

    /// <summary>
    /// 获取全局 Scheduler 列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SchedulerSummaryDto>>>> GetList()
    {
        var schedulers = await _schedulerService.GetAllSchedulersAsync();
        return Ok(ApiResponse<List<SchedulerSummaryDto>>.Ok(schedulers));
    }

    /// <summary>
    /// 获取 Scheduler 详情
    /// </summary>
    [HttpGet("{schedulerName}")]
    public async Task<ActionResult<ApiResponse<SchedulerDetailDto>>> Get(string schedulerName)
    {
        var scheduler = await _schedulerService.GetSchedulerAsync(schedulerName);
        if (scheduler == null)
        {
            return NotFound(ApiResponse<SchedulerDetailDto>.Fail("Scheduler not found"));
        }
        return Ok(ApiResponse<SchedulerDetailDto>.Ok(scheduler));
    }

    /// <summary>
    /// 获取 Scheduler 关联的 Agents
    /// </summary>
    [HttpGet("{schedulerName}/agents")]
    public async Task<ActionResult<ApiResponse<List<SchedulerAgentDto>>>> GetAgents(string schedulerName)
    {
        var agents = await _schedulerService.GetAgentsBySchedulerAsync(schedulerName);
        return Ok(ApiResponse<List<SchedulerAgentDto>>.Ok(agents));
    }
}
