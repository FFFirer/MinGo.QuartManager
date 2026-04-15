using MinGo.Qap.Platform.Data;
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

    public AgentProxyService(
        IHttpClientFactory httpClientFactory,
        PlatformDbContext dbContext,
        ILogger<AgentProxyService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string clusterId, string path)
    {
        var agentUrl = await GetAgentUrlAsync(clusterId);
        var client = CreateClient(agentUrl);
        
        var response = await client.GetAsync($"/api/{path}");
        return await HandleResponse<T>(response, clusterId, path);
    }

    public async Task<T?> PostAsync<T>(string clusterId, string path, object body)
    {
        var agentUrl = await GetAgentUrlAsync(clusterId);
        var client = CreateClient(agentUrl);
        
        var response = await client.PostAsJsonAsync($"/api/{path}", body);
        return await HandleResponse<T>(response, clusterId, path);
    }

    public async Task<T?> PutAsync<T>(string clusterId, string path, object body)
    {
        var agentUrl = await GetAgentUrlAsync(clusterId);
        var client = CreateClient(agentUrl);
        
        var response = await client.PutAsJsonAsync($"/api/{path}", body);
        return await HandleResponse<T>(response, clusterId, path);
    }

    public async Task DeleteAsync(string clusterId, string path)
    {
        var agentUrl = await GetAgentUrlAsync(clusterId);
        var client = CreateClient(agentUrl);
        
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
            var agentUrl = await GetAgentUrlAsync(clusterId);
            var client = CreateClient(agentUrl, shortTimeout: true);
            
            var response = await client.GetAsync("/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    #region Helper Methods

    private async Task<string> GetAgentUrlAsync(string clusterId)
    {
        var cluster = await _dbContext.Clusters.FindAsync(clusterId);
        if (cluster == null)
        {
            throw new ArgumentException($"Cluster not found: {clusterId}");
        }

        return cluster.AgentUrl;
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
