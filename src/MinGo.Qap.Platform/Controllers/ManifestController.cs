using Microsoft.AspNetCore.Mvc;
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
    private readonly ILogger<ManifestController> _logger;

    public ManifestController(ILogger<ManifestController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 上报 Job Manifest
    /// </summary>
    [HttpPost]
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
    public IActionResult Get(string schedulerName)
    {
        if (string.IsNullOrWhiteSpace(schedulerName))
        {
            return BadRequest("SchedulerName is required");
        }

        if (_manifestCache.TryGetValue(schedulerName, out var manifest))
        {
            return Ok(manifest);
        }

        // 返回空的 manifest
        return Ok(new JobManifestDto
        {
            Jobs = new List<JobTypeInfoDto>()
        });
    }
}