using Microsoft.AspNetCore.Mvc;
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
    private static readonly Dictionary<string, JobManifestDto> _manifestCache = new();
    private readonly IAgentProxyService _agentProxy;
    private readonly ILogger<ManifestController> _logger;

    public ManifestController(IAgentProxyService agentProxy, ILogger<ManifestController> logger)
    {
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
        _manifestCache[schedulerName] = manifest;

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

        if (_manifestCache.TryGetValue(schedulerName, out var manifest))
        {
            return Ok(manifest);
        }

        // 缓存未命中，从 Agent 实时获取
        try
        {
            _logger.LogInformation("Manifest cache miss for scheduler {SchedulerName}, forwarding to agent", schedulerName);
            var agentManifest = await _agentProxy.GetAsync<JobManifestDto>(schedulerName, "agent/manifest");

            if (agentManifest != null)
            {
                _manifestCache[schedulerName] = agentManifest;
                return Ok(agentManifest);
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
        return Ok(new JobManifestDto
        {
            Jobs = new List<JobTypeInfoDto>()
        });
    }
}