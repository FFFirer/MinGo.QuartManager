using Microsoft.AspNetCore.Mvc;
using MinGo.Qap.Platform.Services;
using MinGo.Qap.Shared.Attributes;
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
    private readonly IExecutionLogService _executionLogService;
    private readonly ILogger<AgentsController> _logger;

    public AgentsController(
        AgentService agentService,
        SchedulerService schedulerService,
        IExecutionLogService executionLogService,
        ILogger<AgentsController> logger)
    {
        _agentService = agentService;
        _schedulerService = schedulerService;
        _executionLogService = executionLogService;
        _logger = logger;
    }

    /// <summary>
    /// 注册 Agent（首次注册或重连）
    /// </summary>
    [HttpPost]
    [SwaggerHeader("X-Agent-Token", "Agent 身份认证 Token。由 Agent 配置中的 platform.apiToken 提供。")]
    public async Task<ActionResult<ApiResponse<RegisterAgentResponse>>> Register(
        [FromBody] RegisterAgentRequest request)
    {
        try
        {
            // 验证 Token
            var authHeader = Request.Headers["X-Agent-Token"].FirstOrDefault();
            // if (string.IsNullOrEmpty(authHeader))
            // {
            //     return Unauthorized(ApiResponse<RegisterAgentResponse>.Fail("Missing X-Agent-Token header"));
            // }

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
    /// 获取 Agent 列表（带分页）
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<AgentSummaryDto>>>> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var paged = await _agentService.GetPagedAsync(page, pageSize);
        return Ok(ApiResponse<PagedResponse<AgentSummaryDto>>.Ok(paged));
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
    [SwaggerHeader("X-Agent-Token", "Agent 身份认证 Token。由 Agent 配置中的 platform.apiToken 提供。")]
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

    /// <summary>
    /// 接收 Agent 上报的执行日志
    /// </summary>
    [HttpPost("{agentId}/logs")]
    public async Task<ActionResult<ApiResponse<object>>> ReceiveLogs(
        string agentId,
        [FromBody] List<ExecutionLogDto> logs)
    {
        try
        {
            var count = await _executionLogService.ReceiveLogsAsync(agentId, logs);
            return Ok(ApiResponse<object>.Ok(new { received = count }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to receive logs from agent {AgentId}", agentId);
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
