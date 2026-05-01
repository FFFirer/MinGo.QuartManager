using Microsoft.AspNetCore.Mvc;

namespace MinGo.Qap.Platform.Controllers.Old;

/// <summary>
/// 废弃的 Cluster 端点 - 301 永久重定向到新的 Agent/Scheduler 端点
/// </summary>
[ApiController]
[Route("api/clusters")]
public class OldClustersRedirectController : ControllerBase
{
    /// <summary>
    /// GET /api/clusters → 301 到 GET /api/agents
    /// </summary>
    [HttpGet]
    public IActionResult GetClusters()
    {
        return RedirectPermanent("/api/agents");
    }

    /// <summary>
    /// GET /api/clusters/{clusterId} → 301 到 GET /api/agents/{agentId}
    /// </summary>
    [HttpGet("{clusterId}")]
    public IActionResult GetCluster(string clusterId)
    {
        return RedirectPermanent($"/api/agents/{clusterId}");
    }

    /// <summary>
    /// POST /api/clusters → 301 到 POST /api/agents
    /// </summary>
    [HttpPost]
    public IActionResult CreateCluster()
    {
        return RedirectPermanent("/api/agents");
    }

    /// <summary>
    /// DELETE /api/clusters/{clusterId} → 301 到 DELETE /api/agents/{agentId}
    /// </summary>
    [HttpDelete("{clusterId}")]
    public IActionResult DeleteCluster(string clusterId)
    {
        return RedirectPermanent($"/api/agents/{clusterId}");
    }

    /// <summary>
    /// POST /api/clusters/{clusterId}/heartbeat → 301 到 POST /api/agents/{agentId}/heartbeat
    /// </summary>
    [HttpPost("{clusterId}/heartbeat")]
    public IActionResult ClusterHeartbeat(string clusterId)
    {
        return RedirectPermanent($"/api/agents/{clusterId}/heartbeat");
    }

    /// <summary>
    /// GET /api/clusters/{clusterId}/agents → 301 到 GET /api/agents
    /// </summary>
    [HttpGet("{clusterId}/agents")]
    public IActionResult GetClusterAgents(string clusterId)
    {
        return RedirectPermanent("/api/agents");
    }

    /// <summary>
    /// POST /api/clusters/{clusterId}/agents → 301 到 POST /api/agents
    /// </summary>
    [HttpPost("{clusterId}/agents")]
    public IActionResult CreateClusterAgent(string clusterId)
    {
        return RedirectPermanent("/api/agents");
    }

    /// <summary>
    /// POST /api/clusters/{clusterId}/jobs/* → 301 到 POST /api/schedulers/{name}/jobs/*
    /// </summary>
    [HttpPost("{clusterId}/jobs")]
    public IActionResult ClusterCreateJob(string clusterId)
    {
        return RedirectPermanent($"/api/schedulers/{clusterId}/jobs");
    }

    /// <summary>
    /// GET /api/clusters/{clusterId}/jobs → 301 到 GET /api/schedulers/{name}/jobs
    /// </summary>
    [HttpGet("{clusterId}/jobs")]
    public IActionResult ClusterGetJobs(string clusterId)
    {
        return RedirectPermanent($"/api/schedulers/{clusterId}/jobs");
    }

    /// <summary>
    /// GET /api/clusters/{clusterId}/jobs/{jobKey} → 301
    /// </summary>
    [HttpGet("{clusterId}/jobs/{jobKey}")]
    public IActionResult ClusterGetJob(string clusterId, string jobKey)
    {
        return RedirectPermanent($"/api/schedulers/{clusterId}/jobs/{jobKey}");
    }

    /// <summary>
    /// PUT /api/clusters/{clusterId}/jobs/{jobKey} → 301
    /// </summary>
    [HttpPut("{clusterId}/jobs/{jobKey}")]
    public IActionResult ClusterUpdateJob(string clusterId, string jobKey)
    {
        return RedirectPermanent($"/api/schedulers/{clusterId}/jobs/{jobKey}");
    }

    /// <summary>
    /// DELETE /api/clusters/{clusterId}/jobs/{jobKey} → 301
    /// </summary>
    [HttpDelete("{clusterId}/jobs/{jobKey}")]
    public IActionResult ClusterDeleteJob(string clusterId, string jobKey)
    {
        return RedirectPermanent($"/api/schedulers/{clusterId}/jobs/{jobKey}");
    }

    /// <summary>
    /// POST /api/clusters/{clusterId}/jobs/{jobKey}/trigger → 301
    /// </summary>
    [HttpPost("{clusterId}/jobs/{jobKey}/trigger")]
    public IActionResult ClusterTriggerJob(string clusterId, string jobKey)
    {
        return RedirectPermanent($"/api/schedulers/{clusterId}/jobs/{jobKey}/trigger");
    }

    /// <summary>
    /// POST /api/clusters/{clusterId}/jobs/{jobKey}/pause → 301
    /// </summary>
    [HttpPost("{clusterId}/jobs/{jobKey}/pause")]
    public IActionResult ClusterPauseJob(string clusterId, string jobKey)
    {
        return RedirectPermanent($"/api/schedulers/{clusterId}/jobs/{jobKey}/pause");
    }

    /// <summary>
    /// POST /api/clusters/{clusterId}/jobs/{jobKey}/resume → 301
    /// </summary>
    [HttpPost("{clusterId}/jobs/{jobKey}/resume")]
    public IActionResult ClusterResumeJob(string clusterId, string jobKey)
    {
        return RedirectPermanent($"/api/schedulers/{clusterId}/jobs/{jobKey}/resume");
    }

    /// <summary>
    /// GET /api/clusters/{clusterId}/manifest → 301 到 GET /api/schedulers/{name}/manifest
    /// </summary>
    [HttpGet("{clusterId}/manifest")]
    public IActionResult ClusterGetManifest(string clusterId)
    {
        return RedirectPermanent($"/api/schedulers/{clusterId}/manifest");
    }

    /// <summary>
    /// POST /api/clusters/{clusterId}/manifest → 301
    /// </summary>
    [HttpPost("{clusterId}/manifest")]
    public IActionResult ClusterPostManifest(string clusterId)
    {
        return RedirectPermanent($"/api/schedulers/{clusterId}/manifest");
    }
}
