using Microsoft.AspNetCore.Mvc;
using MinGo.Qap.Platform.Services;
using MinGo.Qap.Shared.Models;

namespace MinGo.Qap.Platform.Controllers;

/// <summary>
/// Agent 实例管理控制器
/// </summary>
[ApiController]
[Route("api/clusters/{clusterId}/agents")]
public class AgentInstancesController : ControllerBase
{
    private readonly IAgentInstanceService _agentInstanceService;
    private readonly ILogger<AgentInstancesController> _logger;

    public AgentInstancesController(
        IAgentInstanceService agentInstanceService,
        ILogger<AgentInstancesController> logger)
    {
        _agentInstanceService = agentInstanceService;
        _logger = logger;
    }

    /// <summary>
    /// 注册新的 Agent 实例
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<AgentRegistrationResponse>>> Register(
        string clusterId,
        [FromBody] CreateAgentRequest request,
        [FromHeader(Name = "X-Agent-Token")] string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest(ApiResponse<AgentRegistrationResponse>.Fail("Missing X-Agent-Token header"));
        }

        try
        {
            var response = await _agentInstanceService.RegisterAgentAsync(clusterId, request, token);
            return Ok(ApiResponse<AgentRegistrationResponse>.Ok(response));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<AgentRegistrationResponse>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(ApiResponse<AgentRegistrationResponse>.Fail("Invalid token"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register agent instance for cluster {ClusterId}", clusterId);
            return StatusCode(500, ApiResponse<AgentRegistrationResponse>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// 获取集群的所有 Agent 实例
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AgentInstanceDto>>>> GetInstances(
        string clusterId,
        [FromQuery] bool includeDeleted = false)
    {
        try
        {
            var instances = await _agentInstanceService.GetInstancesByClusterAsync(clusterId, includeDeleted);
            return Ok(ApiResponse<List<AgentInstanceDto>>.Ok(instances));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get agent instances for cluster {ClusterId}", clusterId);
            return StatusCode(500, ApiResponse<List<AgentInstanceDto>>.Fail("Internal server error"));
        }
    }
}

/// <summary>
/// Agent 实例独立操作控制器（心跳、实例管理）
/// </summary>
[ApiController]
[Route("api/agents/{agentId}")]
public class AgentInstanceOperationsController : ControllerBase
{
    private readonly IAgentInstanceService _agentInstanceService;
    private readonly ILogger<AgentInstanceOperationsController> _logger;

    public AgentInstanceOperationsController(
        IAgentInstanceService agentInstanceService,
        ILogger<AgentInstanceOperationsController> logger)
    {
        _agentInstanceService = agentInstanceService;
        _logger = logger;
    }

    /// <summary>
    /// 更新 Agent 实例心跳
    /// </summary>
    [HttpPost("heartbeat")]
    public async Task<ActionResult<ApiResponse<AgentHeartbeatResponse>>> Heartbeat(
        string agentId,
        [FromBody] AgentHeartbeatRequest request)
    {
        try
        {
            var response = await _agentInstanceService.UpdateHeartbeatAsync(agentId, request);
            return Ok(ApiResponse<AgentHeartbeatResponse>.Ok(response));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<AgentHeartbeatResponse>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update heartbeat for agent {AgentId}", agentId);
            return StatusCode(500, ApiResponse<AgentHeartbeatResponse>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// 获取 Agent 实例信息
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<AgentInstanceDto>>> GetInstance(string agentId)
    {
        try
        {
            var instance = await _agentInstanceService.GetInstanceAsync(agentId);
            if (instance == null)
            {
                return NotFound(ApiResponse<AgentInstanceDto>.Fail("Agent instance not found"));
            }
            return Ok(ApiResponse<AgentInstanceDto>.Ok(instance));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get agent instance {AgentId}", agentId);
            return StatusCode(500, ApiResponse<AgentInstanceDto>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// 删除 Agent 实例（软删除）
    /// </summary>
    [HttpDelete]
    public async Task<ActionResult<ApiResponse<object>>> DeleteInstance(string agentId)
    {
        try
        {
            var success = await _agentInstanceService.DeleteInstanceAsync(agentId);
            if (!success)
            {
                return NotFound(ApiResponse<object>.Fail("Agent instance not found"));
            }
            return Ok(ApiResponse<object>.Ok(new { success = true }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete agent instance {AgentId}", agentId);
            return StatusCode(500, ApiResponse<object>.Fail("Internal server error"));
        }
    }
}