using Microsoft.AspNetCore.Http;
using MinGo.Qap.Platform.Data;
using MinGo.Qap.Shared.Enums;
using MinGo.Qap.Shared.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace MinGo.Qap.Platform.Services;

/// <summary>
/// Agent 代理服务接口
/// </summary>
public interface IAgentProxyService
{
    Task<T?> GetAsync<T>(string clusterId, string path);
    Task<T?> PostAsync<T>(string clusterId, string path, object body);
    Task<T?> PutAsync<T>(string clusterId, string path, object body);
    Task DeleteAsync(string clusterId, string path);
    Task<bool> IsHealthyAsync(string clusterId);
}

/// <summary>
/// Agent 代理服务实现
/// </summary>
public class AgentProxyService : IAgentProxyService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PlatformDbContext _dbContext;
    private readonly ILogger<AgentProxyService> _logger;
    private readonly IAgentInstanceService _agentInstanceService;
    private readonly IAgentSelectionStrategy _selectionStrategy;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    private const string AgentInstanceIdHeader = "X-Agent-Instance-Id";

    public AgentProxyService(
        IHttpClientFactory httpClientFactory,
        PlatformDbContext dbContext,
        ILogger<AgentProxyService> logger,
        IAgentInstanceService agentInstanceService,
        IAgentSelectionStrategy selectionStrategy,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _dbContext = dbContext;
        _logger = logger;
        _agentInstanceService = agentInstanceService;
        _selectionStrategy = selectionStrategy;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<T?> GetAsync<T>(string clusterId, string path)
    {
        var agentInstance = await GetAgentInstanceAsync(clusterId);
        if (agentInstance == null)
        {
            throw new AgentException($"No healthy agent instances available for cluster {clusterId}", "NO_HEALTHY_INSTANCES");
        }
        
        var client = CreateClient(agentInstance.Url);
        var response = await client.GetAsync($"/api/{path}");
        return await HandleResponse<T>(response, clusterId, path);
    }

    public async Task<T?> PostAsync<T>(string clusterId, string path, object body)
    {
        var agentInstance = await GetAgentInstanceAsync(clusterId);
        if (agentInstance == null)
        {
            throw new AgentException($"No healthy agent instances available for cluster {clusterId}", "NO_HEALTHY_INSTANCES");
        }
        
        var client = CreateClient(agentInstance.Url);
        var response = await client.PostAsJsonAsync($"/api/{path}", body);
        return await HandleResponse<T>(response, clusterId, path);
    }

    public async Task<T?> PutAsync<T>(string clusterId, string path, object body)
    {
        var agentInstance = await GetAgentInstanceAsync(clusterId);
        if (agentInstance == null)
        {
            throw new AgentException($"No healthy agent instances available for cluster {clusterId}", "NO_HEALTHY_INSTANCES");
        }
        
        var client = CreateClient(agentInstance.Url);
        var response = await client.PutAsJsonAsync($"/api/{path}", body);
        return await HandleResponse<T>(response, clusterId, path);
    }

    public async Task DeleteAsync(string clusterId, string path)
    {
        var agentInstance = await GetAgentInstanceAsync(clusterId);
        if (agentInstance == null)
        {
            throw new AgentException($"No healthy agent instances available for cluster {clusterId}", "NO_HEALTHY_INSTANCES");
        }
        
        var client = CreateClient(agentInstance.Url);
        var response = await client.DeleteAsync($"/api/{path}");
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "Agent request failed: {ClusterId} {Path} - {StatusCode}: {Error}",
                clusterId, path, response.StatusCode, error);
            
            throw new AgentException(
                $"Agent request failed: {response.StatusCode}",
                response.StatusCode.ToString());
        }
    }

    public async Task<bool> IsHealthyAsync(string clusterId)
    {
        try
        {
            var agentInstance = await GetAgentInstanceAsync(clusterId);
            if (agentInstance == null)
            {
                return false;
            }
            
            var client = CreateClient(agentInstance.Url, shortTimeout: true);
            var response = await client.GetAsync("/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    #region Helper Methods

    private async Task<AgentInstanceDto?> GetAgentInstanceAsync(string clusterId)
    {
        // 检查是否通过请求头指定了实例 ID
        var instanceId = GetRequestedInstanceId();
        if (!string.IsNullOrEmpty(instanceId))
        {
            var instance = await _agentInstanceService.GetInstanceAsync(instanceId);
            if (instance != null && instance.ClusterId == clusterId && instance.Status == AgentStatus.Online)
            {
                _logger.LogDebug("Using requested agent instance {InstanceId} for cluster {ClusterId}", instanceId, clusterId);
                return instance;
            }
            else
            {
                _logger.LogWarning("Requested agent instance {InstanceId} not found, not online, or not part of cluster {ClusterId}", instanceId, clusterId);
            }
        }
        
        // 获取集群的健康实例
        var healthyInstances = await _agentInstanceService.GetHealthyInstancesAsync(clusterId);
        if (healthyInstances.Count == 0)
        {
            _logger.LogWarning("No healthy agent instances available for cluster {ClusterId}", clusterId);
            return null;
        }
        
        // 使用选择策略选择实例
        var selectedInstance = _selectionStrategy.SelectInstance(clusterId, healthyInstances);
        if (selectedInstance != null)
        {
            _logger.LogDebug("Selected agent instance {InstanceId} ({Url}) for cluster {ClusterId} using {Strategy} strategy", 
                selectedInstance.Id, selectedInstance.Url, clusterId, _selectionStrategy.Name);
        }
        
        return selectedInstance;
    }
    
    private string? GetRequestedInstanceId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Request?.Headers != null && 
            httpContext.Request.Headers.TryGetValue(AgentInstanceIdHeader, out var instanceIdHeader))
        {
            return instanceIdHeader.ToString();
        }
        
        return null;
    }

    private HttpClient CreateClient(string agentUrl, bool shortTimeout = false)
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(agentUrl);
        
        if (shortTimeout)
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        }
        else
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        }

        return client;
    }

    private async Task<T?> HandleResponse<T>(HttpResponseMessage response, string clusterId, string path)
    {
        if (response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return default;
            }

            return await response.Content.ReadFromJsonAsync<T>();
        }

        var error = await response.Content.ReadAsStringAsync();
        _logger.LogError(
            "Agent request failed: {ClusterId} {Path} - {StatusCode}: {Error}",
            clusterId, path, response.StatusCode, error);

        throw new AgentException(
            $"Agent request failed: {response.StatusCode}",
            response.StatusCode.ToString());
    }

    #endregion
}

/// <summary>
/// Agent 请求异常
/// </summary>
public class AgentException : Exception
{
    public string ErrorCode { get; }

    public AgentException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }
}
