using Microsoft.AspNetCore.Mvc;
using MinGo.Qap.Shared.Models;

namespace MinGo.Qap.Platform.Controllers;

/// <summary>
/// Job Manifest 控制器
/// </summary>
[ApiController]
[Route("api/clusters/{clusterId}/manifest")]
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
    public IActionResult Post(string clusterId, [FromBody] JobManifestDto manifest)
    {
        if (string.IsNullOrWhiteSpace(clusterId))
        {
            return BadRequest("ClusterId is required");
        }

        if (manifest == null || manifest.Jobs == null)
        {
            return BadRequest("Invalid manifest data");
        }

        // 验证 ClusterId 匹配
        if (manifest.ClusterId != clusterId)
        {
            return BadRequest("ClusterId in manifest does not match URL");
        }

        // 存储到内存缓存
        _manifestCache[clusterId] = manifest;

        _logger.LogInformation("Manifest received for cluster {ClusterId} with {JobCount} job types",
            clusterId, manifest.Jobs.Count);

        return Ok();
    }

    /// <summary>
    /// 获取 Job Manifest
    /// </summary>
    [HttpGet]
    public IActionResult Get(string clusterId)
    {
        if (string.IsNullOrWhiteSpace(clusterId))
        {
            return BadRequest("ClusterId is required");
        }

        if (_manifestCache.TryGetValue(clusterId, out var manifest))
        {
            return Ok(manifest);
        }

        // 返回空的 manifest
        return Ok(new JobManifestDto
        {
            ClusterId = clusterId,
            Jobs = new List<JobTypeInfoDto>()
        });
    }
}