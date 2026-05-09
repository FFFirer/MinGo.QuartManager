using Microsoft.AspNetCore.Mvc;
using MinGo.Qap.Platform.Services;
using MinGo.Qap.Shared.Attributes;
using MinGo.Qap.Shared.Models;

namespace MinGo.Qap.Platform.Controllers;

/// <summary>
/// Job 管理控制器
/// </summary>
[ApiController]
[Route("api/schedulers/{schedulerName}/jobs")]
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
    // [SwaggerHeader("X-Scheduler-Name", "Scheduler 名称。Platform 转发请求到 Agent 时指定目标 Scheduler。")]
    public async Task<ActionResult<ApiResponse<PagedResponse<JobSummaryDto>>>> GetList(
        string schedulerName,
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

        var result = await _jobService.GetBySchedulerAsync(schedulerName, query);
        return Ok(ApiResponse<PagedResponse<JobSummaryDto>>.Ok(result));
    }

    /// <summary>
    /// 获取 Job 详情
    /// </summary>
    [HttpGet("{name}")]
    [HttpGet("{name}/{group}")]
    [SwaggerHeader("X-Scheduler-Name", "Scheduler 名称。Platform 转发请求到 Agent 时指定目标 Scheduler。")]
    public async Task<ActionResult<ApiResponse<JobDefinitionDto>>> Get(string schedulerName, string name, string? group)
    {
        var jobKey = new JobKeyDto(name, group ?? "DEFAULT");
        var job = await _jobService.GetAsync(schedulerName, jobKey);
        if (job == null)
        {
            return NotFound(ApiResponse<JobDefinitionDto>.Fail("Job not found"));
        }

        return Ok(ApiResponse<JobDefinitionDto>.Ok(job));
    }

    /// <summary>
    /// 创建 Job（声明式）
    /// </summary>
    [HttpPost]
    [SwaggerHeader("X-Scheduler-Name", "Scheduler 名称。Platform 转发请求到 Agent 时指定目标 Scheduler。")]
    public async Task<ActionResult<ApiResponse<JobDefinitionDto>>> Create(string schedulerName, [FromBody] CreateJobRequest request)
    {
        try
        {
            var job = await _jobService.CreateAsync(schedulerName, request);
            return Ok(ApiResponse<JobDefinitionDto>.Ok(job));
        }
        catch (DeclarationConflictException ex)
        {
            return Conflict(ApiResponse<JobDefinitionDto>.Fail(ex.Message));
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
    [HttpPut("{name}")]
    [HttpPut("{name}/{group}")]
    [SwaggerHeader("X-Scheduler-Name", "Scheduler 名称。Platform 转发请求到 Agent 时指定目标 Scheduler。")]
    public async Task<ActionResult<ApiResponse<object>>> Update(
        string schedulerName,
        string name,
        string? group,
        [FromBody] UpdateJobRequest request)
    {
        try
        {
            var jobKey = new JobKeyDto(name, group ?? "DEFAULT");
            await _jobService.UpdateAsync(schedulerName, jobKey, request);
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
    [HttpDelete("{name}")]
    [HttpDelete("{name}/{group}")]
    [SwaggerHeader("X-Scheduler-Name", "Scheduler 名称。Platform 转发请求到 Agent 时指定目标 Scheduler。")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(string schedulerName, string name, string? group)
    {
        try
        {
            var jobKey = new JobKeyDto(name, group ?? "DEFAULT");
            await _jobService.DeleteAsync(schedulerName, jobKey);
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
    [HttpPost("{name}/trigger")]
    [HttpPost("{name}/{group}/trigger")]
    [SwaggerHeader("X-Scheduler-Name", "Scheduler 名称。Platform 转发请求到 Agent 时指定目标 Scheduler。")]
    public async Task<ActionResult<ApiResponse<object>>> Trigger(string schedulerName, string name, string? group)
    {
        try
        {
            var jobKey = new JobKeyDto(name, group ?? "DEFAULT");
            await _jobService.TriggerAsync(schedulerName, jobKey);
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
    [HttpPost("{name}/pause")]
    [HttpPost("{name}/{group}/pause")]
    [SwaggerHeader("X-Scheduler-Name", "Scheduler 名称。Platform 转发请求到 Agent 时指定目标 Scheduler。")]
    public async Task<ActionResult<ApiResponse<object>>> Pause(string schedulerName, string name, string? group)
    {
        try
        {
            var jobKey = new JobKeyDto(name, group ?? "DEFAULT");
            await _jobService.PauseAsync(schedulerName, jobKey);
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
    [HttpPost("{name}/resume")]
    [HttpPost("{name}/{group}/resume")]
    [SwaggerHeader("X-Scheduler-Name", "Scheduler 名称。Platform 转发请求到 Agent 时指定目标 Scheduler。")]
    public async Task<ActionResult<ApiResponse<object>>> Resume(string schedulerName, string name, string? group)
    {
        try
        {
            var jobKey = new JobKeyDto(name, group ?? "DEFAULT");
            await _jobService.ResumeAsync(schedulerName, jobKey);
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
