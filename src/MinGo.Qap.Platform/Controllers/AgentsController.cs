using Microsoft.AspNetCore.Mvc;
using MinGo.Qap.Platform.Services;
using MinGo.Qap.Shared.Models;

namespace MinGo.Qap.Platform.Controllers;

/// <summary>
/// Agent 管理控制器
/// </summary>
[ApiController]
[Route("api/agents")]
public class AgentsController : ControllerBase
{
    private readonly AgentService _agentService;
    private readonly SchedulerService _schedulerService;
    private readonly ILogger<AgentsController> _logger;

    public AgentsController(
        AgentService agentService,
        SchedulerService schedulerService,
        ILogger<AgentsController> logger)
    {
        _agentService = agentService;
        _schedulerService = schedulerService;
        _logger = logger;
    }

    /// <summary>
    /// 注册 Agent（首次注册或重连）
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<RegisterAgentResponse>>> Register(
        [FromBody] RegisterAgentRequest request)
    {
        try
        {
            // 验证 Token
            var authHeader = Request.Headers["X-Agent-Token"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader))
            {
                return Unauthorized(ApiResponse<RegisterAgentResponse>.Fail("Missing X-Agent-Token header"));
            }

            var response = await _agentService.RegisterAsync(request, authHeader);
            return Ok(ApiResponse<RegisterAgentResponse>.Ok(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent registration failed");
            return BadRequest(ApiResponse<RegisterAgentResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 获取 Agent 列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AgentSummaryDto>>>> GetList()
    {
        var agents = await _agentService.GetAllAsync();
        return Ok(ApiResponse<List<AgentSummaryDto>>.Ok(agents));
    }

    /// <summary>
    /// 获取 Agent 详情
    /// </summary>
    [HttpGet("{agentId}")]
    public async Task<ActionResult<ApiResponse<AgentDetailDto>>> Get(string agentId)
    {
        var agent = await _agentService.GetAsync(agentId);
        if (agent == null)
        {
            return NotFound(ApiResponse<AgentDetailDto>.Fail("Agent not found"));
        }
        return Ok(ApiResponse<AgentDetailDto>.Ok(agent));
    }

    /// <summary>
    /// 删除 Agent（软删除）
    /// </summary>
    [HttpDelete("{agentId}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(string agentId)
    {
        // 验证 Token
        var authHeader = Request.Headers["X-Agent-Token"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && !await _agentService.ValidateTokenAsync(agentId, authHeader))
        {
            return Unauthorized(ApiResponse<object>.Fail("Invalid token"));
        }

        var success = await _agentService.DeleteAsync(agentId);
        if (!success)
        {
            return NotFound(ApiResponse<object>.Fail("Agent not found"));
        }
        return Ok(ApiResponse<object>.Ok(new { }));
    }

    /// <summary>
    /// Agent 心跳
    /// </summary>
    [HttpPost("{agentId}/heartbeat")]
    public async Task<ActionResult<ApiResponse<AgentHeartbeatResponseV2>>> Heartbeat(
        string agentId,
        [FromBody] AgentHeartbeatRequestV2 request)
    {
        var success = await _agentService.UpdateHeartbeatAsync(agentId);
        if (!success)
        {
            return NotFound(ApiResponse<AgentHeartbeatResponseV2>.Fail("Agent not found"));
        }

        return Ok(ApiResponse<AgentHeartbeatResponseV2>.Ok(new AgentHeartbeatResponseV2
        {
            ServerTime = DateTimeOffset.UtcNow
        }));
    }

    /// <summary>
    /// 上报 Scheduler 信息
    /// </summary>
    [HttpPost("{agentId}/schedulers")]
    public async Task<ActionResult<ApiResponse<object>>> ReportSchedulers(
        string agentId,
        [FromBody] SchedulerReportRequest request)
    {
        try
        {
            await _schedulerService.ReportSchedulersAsync(agentId, request);
            return Ok(ApiResponse<object>.Ok(new { }));
        }
        catch (ArgumentException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to report schedulers for agent {AgentId}", agentId);
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 查询 Agent 关联的 Schedulers
    /// </summary>
    [HttpGet("{agentId}/schedulers")]
    public async Task<ActionResult<ApiResponse<List<AgentSchedulerDto>>>> GetSchedulers(string agentId)
    {
        var schedulers = await _schedulerService.GetSchedulersByAgentAsync(agentId);
        return Ok(ApiResponse<List<AgentSchedulerDto>>.Ok(schedulers));
    }
}
