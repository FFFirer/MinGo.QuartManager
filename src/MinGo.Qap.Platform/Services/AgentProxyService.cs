using Microsoft.AspNetCore.Http;
using MinGo.Qap.Platform.Data;
using MinGo.Qap.Shared.Models;
using System.Net.Http.Json;

namespace MinGo.Qap.Platform.Services;

/// <summary>
/// Agent 代理服务接口
/// </summary>
public interface IAgentProxyService
{
    Task<T?> GetAsync<T>(string schedulerName, string path);
    Task<T?> PostAsync<T>(string schedulerName, string path, object body);
    Task<T?> PutAsync<T>(string schedulerName, string path, object body);
    Task DeleteAsync(string schedulerName, string path);
    Task<bool> IsHealthyAsync(string schedulerName);
}

/// <summary>
/// Agent 代理服务实现
/// 通过 SchedulerRouterService 选择 Agent，并在转发时设置 X-Scheduler-Name 请求头
/// </summary>
public class AgentProxyService : IAgentProxyService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PlatformDbContext _dbContext;
    private readonly ILogger<AgentProxyService> _logger;
    private readonly SchedulerRouterService _schedulerRouterService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private const string SchedulerNameHeader = "X-Scheduler-Name";

    public AgentProxyService(
        IHttpClientFactory httpClientFactory,
        PlatformDbContext dbContext,
        ILogger<AgentProxyService> logger,
        SchedulerRouterService schedulerRouterService,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _dbContext = dbContext;
        _logger = logger;
        _schedulerRouterService = schedulerRouterService;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 根据 SchedulerName 选择一个健康的 Agent，并转发 GET 请求
    /// 转发时设置 X-Scheduler-Name 请求头，指示 Agent 使用哪个 Scheduler
    /// </summary>
    public async Task<T?> GetAsync<T>(string schedulerName, string path)
    {
        var agent = await _schedulerRouterService.PickAgentForSchedulerAsync(schedulerName);
        if (agent == null)
        {
            throw new AgentException($"No healthy agent available for scheduler '{schedulerName}'", "NO_HEALTHY_AGENT");
        }

        var client = CreateClient(agent.Url);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/{path}");
        request.Headers.Add(SchedulerNameHeader, schedulerName);

        var response = await client.SendAsync(request);
        return await HandleResponse<T>(response, schedulerName, path);
    }

    public async Task<T?> PostAsync<T>(string schedulerName, string path, object body)
    {
        var agent = await _schedulerRouterService.PickAgentForSchedulerAsync(schedulerName);
        if (agent == null)
        {
            throw new AgentException($"No healthy agent available for scheduler '{schedulerName}'", "NO_HEALTHY_AGENT");
        }

        var client = CreateClient(agent.Url);
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/{path}");
        request.Headers.Add(SchedulerNameHeader, schedulerName);
        request.Content = JsonContent.Create(body);

        var response = await client.SendAsync(request);
        return await HandleResponse<T>(response, schedulerName, path);
    }

    public async Task<T?> PutAsync<T>(string schedulerName, string path, object body)
    {
        var agent = await _schedulerRouterService.PickAgentForSchedulerAsync(schedulerName);
        if (agent == null)
        {
            throw new AgentException($"No healthy agent available for scheduler '{schedulerName}'", "NO_HEALTHY_AGENT");
        }

        var client = CreateClient(agent.Url);
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/{path}");
        request.Headers.Add(SchedulerNameHeader, schedulerName);
        request.Content = JsonContent.Create(body);

        var response = await client.SendAsync(request);
        return await HandleResponse<T>(response, schedulerName, path);
    }

    public async Task DeleteAsync(string schedulerName, string path)
    {
        var agent = await _schedulerRouterService.PickAgentForSchedulerAsync(schedulerName);
        if (agent == null)
        {
            throw new AgentException($"No healthy agent available for scheduler '{schedulerName}'", "NO_HEALTHY_AGENT");
        }

        var client = CreateClient(agent.Url);
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/{path}");
        request.Headers.Add(SchedulerNameHeader, schedulerName);

        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "Agent request failed: {SchedulerName} {Path} - {StatusCode}: {Error}",
                schedulerName, path, response.StatusCode, error);

            throw new AgentException(
                $"Agent request failed: {response.StatusCode}",
                response.StatusCode.ToString());
        }
    }

    public async Task<bool> IsHealthyAsync(string schedulerName)
    {
        try
        {
            var agent = await _schedulerRouterService.PickAgentForSchedulerAsync(schedulerName);
            if (agent == null) return false;

            var client = CreateClient(agent.Url, shortTimeout: true);
            var response = await client.GetAsync("/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    #region Helper Methods

    private HttpClient CreateClient(string agentUrl, bool shortTimeout = false)
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(agentUrl.TrimEnd('/'));

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

    private async Task<T?> HandleResponse<T>(HttpResponseMessage response, string schedulerName, string path)
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
            "Agent request failed: scheduler {SchedulerName} {Path} - {StatusCode}: {Error}",
            schedulerName, path, response.StatusCode, error);

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
