using Microsoft.AspNetCore.Mvc;
using MinGo.Qap.Platform.Caching;
using MinGo.Qap.Platform.Services;
using MinGo.Qap.Shared.Attributes;
using MinGo.Qap.Shared.Models;

namespace MinGo.Qap.Platform.Controllers;

/// <summary>
/// Job Manifest 控制器
/// </summary>
[ApiController]
[Route("api/schedulers/{schedulerName}/manifest")]
public class ManifestController : ControllerBase
{
    private readonly IManifestCacheService _cache;
    private readonly IAgentProxyService _agentProxy;
    private readonly ILogger<ManifestController> _logger;

    public ManifestController(
        IManifestCacheService cache,
        IAgentProxyService agentProxy,
        ILogger<ManifestController> logger)
    {
        _cache = cache;
        _agentProxy = agentProxy;
        _logger = logger;
    }

    /// <summary>
    /// 上报 Job Manifest
    /// </summary>
    [HttpPost]
    [SwaggerHeader("X-Scheduler-Name", "Scheduler 名称。Platform 转发请求到 Agent 时指定目标 Scheduler。")]
    public IActionResult Post(string schedulerName, [FromBody] JobManifestDto manifest)
    {
        if (string.IsNullOrWhiteSpace(schedulerName))
        {
            return BadRequest("SchedulerName is required");
        }

        if (manifest == null || manifest.Jobs == null)
        {
            return BadRequest("Invalid manifest data");
        }

        // 存储到内存缓存
        _cache.Set(schedulerName, manifest);

        _logger.LogInformation("Manifest received for scheduler {SchedulerName} with {JobCount} job types",
            schedulerName, manifest.Jobs.Count);

        return Ok();
    }

    /// <summary>
    /// 获取 Job Manifest
    /// </summary>
    [HttpGet]
    [SwaggerHeader("X-Scheduler-Name", "Scheduler 名称。Platform 转发请求到 Agent 时指定目标 Scheduler。")]
    public async Task<IActionResult> Get(string schedulerName)
    {
        if (string.IsNullOrWhiteSpace(schedulerName))
        {
            return BadRequest("SchedulerName is required");
        }

        if (_cache.TryGet(schedulerName, out var cachedManifest))
        {
            _logger.LogDebug("Manifest cache hit for scheduler {SchedulerName}", schedulerName);
            return Ok(ApiResponse<JobManifestDto>.Ok(cachedManifest));
        }

        // 缓存未命中或已过期，从 Agent 实时获取
        try
        {
            _logger.LogInformation("Manifest cache miss for scheduler {SchedulerName}, forwarding to agent", schedulerName);
            var agentManifest = await _agentProxy.GetAsync<JobManifestDto>(schedulerName, "manifest");

            if (agentManifest != null)
            {
                _cache.Set(schedulerName, agentManifest);
                _logger.LogInformation("Manifest cached for scheduler {SchedulerName} with {JobCount} job types",
                    schedulerName, agentManifest.Jobs.Count);
                return Ok(ApiResponse<JobManifestDto>.Ok(agentManifest));
            }
        }
        catch (AgentException ex)
        {
            _logger.LogWarning(ex, "Failed to fetch manifest from agent for scheduler {SchedulerName}", schedulerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching manifest from agent for scheduler {SchedulerName}", schedulerName);
        }

        // Agent 不可用时返回空的 manifest
        return Ok(ApiResponse<JobManifestDto>.Ok(new JobManifestDto
        {
            Jobs = new List<JobTypeInfoDto>()
        }));
    }
}