using Microsoft.AspNetCore.Mvc;
using MinGo.Qap.Platform.Services;
using MinGo.Qap.Shared.Models;

namespace MinGo.Qap.Platform.Controllers;

/// <summary>
/// Cluster 管理控制器
/// </summary>
[ApiController]
[Route("api/clusters")]
public class ClustersController : ControllerBase
{
    private readonly IClusterService _clusterService;
    private readonly ILogger<ClustersController> _logger;

    public ClustersController(IClusterService clusterService, ILogger<ClustersController> logger)
    {
        _clusterService = clusterService;
        _logger = logger;
    }

    /// <summary>
    /// 创建 Cluster
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CreateClusterResponse>>> Create([FromBody] CreateClusterRequest request)
    {
        try
        {
            var cluster = await _clusterService.CreateAsync(request);
            
            // 生成 Token（实际应该在服务层生成并返回）
            var token = $"qap_tok_{Guid.NewGuid():N}";
            
            var response = new CreateClusterResponse
            {
                Id = cluster.Id,
                Name = cluster.Name,
                Token = token,
                Status = cluster.Status,
                CreatedAt = cluster.CreatedAt
            };

            return Ok(ApiResponse<CreateClusterResponse>.Ok(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create cluster");
            return BadRequest(ApiResponse<CreateClusterResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 获取 Cluster 列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ClusterSummaryDto>>>> GetList(
        [FromQuery] string? env,
        [FromQuery] string? status)
    {
        var clusters = await _clusterService.GetAllAsync(env, status);
        return Ok(ApiResponse<List<ClusterSummaryDto>>.Ok(clusters));
    }

    /// <summary>
    /// 获取 Cluster 详情
    /// </summary>
    [HttpGet("{clusterId}")]
    public async Task<ActionResult<ApiResponse<ClusterDto>>> Get(string clusterId)
    {
        var cluster = await _clusterService.GetAsync(clusterId);
        if (cluster == null)
        {
            return NotFound(ApiResponse<ClusterDto>.Fail("Cluster not found"));
        }

        return Ok(ApiResponse<ClusterDto>.Ok(cluster));
    }

    /// <summary>
    /// 删除 Cluster
    /// </summary>
    [HttpDelete("{clusterId}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(string clusterId)
    {
        try
        {
            await _clusterService.DeleteAsync(clusterId);
            return Ok(ApiResponse<object>.Ok(new { }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 接收心跳
    /// </summary>
    [HttpPost("{clusterId}/heartbeat")]
    public async Task<ActionResult<ApiResponse<object>>> Heartbeat(string clusterId, [FromBody] HeartbeatDto heartbeat)
    {
        await _clusterService.UpdateHeartbeatAsync(clusterId, heartbeat);
        return Ok(ApiResponse<object>.Ok(new { }));
    }
}
