using Microsoft.AspNetCore.Mvc;
using MinGo.Qap.Platform.Services;
using MinGo.Qap.Shared.Models;

namespace MinGo.Qap.Platform.Controllers;

/// <summary>
/// Job 管理控制器
/// </summary>
[ApiController]
[Route("api/clusters/{clusterId}/jobs")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;
    private readonly ILogger<JobsController> _logger;

    public JobsController(IJobService jobService, ILogger<JobsController> logger)
    {
        _jobService = jobService;
        _logger = logger;
    }

    /// <summary>
    /// 获取 Job 列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<JobSummaryDto>>>> GetList(
        string clusterId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? group = null,
        [FromQuery] string? keyword = null)
    {
        var query = new JobQuery
        {
            Page = page,
            PageSize = pageSize,
            Status = status,
            Group = group,
            Keyword = keyword
        };

        var jobs = await _jobService.GetByClusterAsync(clusterId, query);
        return Ok(ApiResponse<List<JobSummaryDto>>.Ok(jobs));
    }

    /// <summary>
    /// 获取 Job 详情
    /// </summary>
    [HttpGet("{jobKey}")]
    public async Task<ActionResult<ApiResponse<JobDefinitionDto>>> Get(string clusterId, string jobKey)
    {
        var job = await _jobService.GetAsync(clusterId, jobKey);
        if (job == null)
        {
            return NotFound(ApiResponse<JobDefinitionDto>.Fail("Job not found"));
        }

        return Ok(ApiResponse<JobDefinitionDto>.Ok(job));
    }

    /// <summary>
    /// 创建 Job
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<JobDefinitionDto>>> Create(string clusterId, [FromBody] CreateJobRequest request)
    {
        try
        {
            var job = await _jobService.CreateAsync(clusterId, request);
            return Ok(ApiResponse<JobDefinitionDto>.Ok(job));
        }
        catch (AgentException ex)
        {
            _logger.LogError(ex, "Failed to create job");
            return StatusCode(502, ApiResponse<JobDefinitionDto>.Fail(ex.Message, ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create job");
            return BadRequest(ApiResponse<JobDefinitionDto>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 更新 Job
    /// </summary>
    [HttpPut("{jobKey}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(
        string clusterId, 
        string jobKey, 
        [FromBody] UpdateJobRequest request)
    {
        try
        {
            await _jobService.UpdateAsync(clusterId, jobKey, request);
            return Ok(ApiResponse<object>.Ok(new { }));
        }
        catch (AgentException ex)
        {
            return StatusCode(502, ApiResponse<object>.Fail(ex.Message, ex.ErrorCode));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 删除 Job
    /// </summary>
    [HttpDelete("{jobKey}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(string clusterId, string jobKey)
    {
        try
        {
            await _jobService.DeleteAsync(clusterId, jobKey);
            return Ok(ApiResponse<object>.Ok(new { }));
        }
        catch (AgentException ex)
        {
            return StatusCode(502, ApiResponse<object>.Fail(ex.Message, ex.ErrorCode));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 手动触发 Job
    /// </summary>
    [HttpPost("{jobKey}/trigger")]
    public async Task<ActionResult<ApiResponse<object>>> Trigger(string clusterId, string jobKey)
    {
        try
        {
            await _jobService.TriggerAsync(clusterId, jobKey);
            return Ok(ApiResponse<object>.Ok(new { }));
        }
        catch (AgentException ex)
        {
            return StatusCode(502, ApiResponse<object>.Fail(ex.Message, ex.ErrorCode));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 暂停 Job
    /// </summary>
    [HttpPost("{jobKey}/pause")]
    public async Task<ActionResult<ApiResponse<object>>> Pause(string clusterId, string jobKey)
    {
        try
        {
            await _jobService.PauseAsync(clusterId, jobKey);
            return Ok(ApiResponse<object>.Ok(new { }));
        }
        catch (AgentException ex)
        {
            return StatusCode(502, ApiResponse<object>.Fail(ex.Message, ex.ErrorCode));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// 恢复 Job
    /// </summary>
    [HttpPost("{jobKey}/resume")]
    public async Task<ActionResult<ApiResponse<object>>> Resume(string clusterId, string jobKey)
    {
        try
        {
            await _jobService.ResumeAsync(clusterId, jobKey);
            return Ok(ApiResponse<object>.Ok(new { }));
        }
        catch (AgentException ex)
        {
            return StatusCode(502, ApiResponse<object>.Fail(ex.Message, ex.ErrorCode));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
