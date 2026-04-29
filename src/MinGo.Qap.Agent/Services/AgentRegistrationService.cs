using Microsoft.Extensions.Options;
using MinGo.Qap.Agent.Configuration;
using MinGo.Qap.Shared.Models;
using System.Net.Http.Json;

namespace MinGo.Qap.Agent.Services;

/// <summary>
/// Agent 注册服务接口
/// </summary>
public interface IAgentRegistrationService
{
    /// <summary>
    /// 注册 Agent 实例到平台
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>注册响应</returns>
    Task<AgentRegistrationResponse> RegisterAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取当前注册信息（如果已注册）
    /// </summary>
    /// <returns>注册信息，如果未注册则为 null</returns>
    AgentRegistrationInfo? GetRegistrationInfo();
    
    /// <summary>
    /// 注销 Agent 实例
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>注销是否成功</returns>
    Task<bool> DeregisterAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Agent 注册信息
/// </summary>
public class AgentRegistrationInfo
{
    /// <summary>
    /// Agent 实例 ID
    /// </summary>
    public string AgentId { get; set; } = string.Empty;
    
    /// <summary>
    /// Quartz 实例 ID
    /// </summary>
    public string? QuartzInstanceId { get; set; }
    
    /// <summary>
    /// 所属集群 ID
    /// </summary>
    public string ClusterId { get; set; } = string.Empty;
    
    /// <summary>
    /// 注册时间
    /// </summary>
    public DateTime RegisteredAt { get; set; }
    
    /// <summary>
    /// 平台 API 基础 URL
    /// </summary>
    public string PlatformApiBaseUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// 心跳间隔（秒）
    /// </summary>
    public int HeartbeatIntervalSeconds { get; set; }
    
    /// <summary>
    /// 警告阈值（秒）
    /// </summary>
    public int WarningThresholdSeconds { get; set; }
    
    /// <summary>
    /// 离线阈值（秒）
    /// </summary>
    public int OfflineThresholdSeconds { get; set; }
}

/// <summary>
/// Agent 注册服务实现
/// </summary>
public class AgentRegistrationService : IAgentRegistrationService
{
    private readonly AgentConfig _config;
    private readonly ILogger<AgentRegistrationService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AgentUrlResolver _urlResolver;
    private AgentRegistrationInfo? _registrationInfo;
    
    public AgentRegistrationService(
        IOptions<AgentConfig> options,
        ILogger<AgentRegistrationService> logger,
        IHttpClientFactory httpClientFactory,
        AgentUrlResolver urlResolver)
    {
        _config = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _urlResolver = urlResolver;
    }
    
    public async Task<AgentRegistrationResponse> RegisterAsync(CancellationToken cancellationToken = default)
    {
        var platformUrl = _config.Platform.Url;
        var clusterId = _config.Agent.ClusterId;
        var apiToken = _config.Platform.ApiToken;
        
        if (string.IsNullOrEmpty(platformUrl))
        {
            throw new InvalidOperationException("Platform URL is not configured");
        }
        
        if (string.IsNullOrEmpty(clusterId))
        {
            throw new InvalidOperationException("Cluster ID is not configured");
        }
        
        if (string.IsNullOrEmpty(apiToken))
        {
            throw new InvalidOperationException("API Token is not configured");
        }
        
        // 获取 Agent URL（从配置或推导）
        var agentUrl = GetAgentUrl(_config);
        
        // 生成 Quartz 实例 ID（如果集群模式启用）
        string? quartzInstanceId = null;
        if (_config.Agent.ClusterMode)
        {
            quartzInstanceId = GenerateQuartzInstanceId(_config, clusterId);
        }
        
        var request = new CreateAgentRequest
        {
            Name = $"agent-{Environment.MachineName.ToLowerInvariant()}",
            Url = agentUrl,
            AgentVersion = GetAgentVersion(),
            QuartzInstanceId = quartzInstanceId // 可选，平台可覆盖
        };
        
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Agent-Token", apiToken);
        
        var maxAttempts = _config.Agent.RegistrationMaxAttempts;
        var retryDelay = TimeSpan.FromSeconds(_config.Agent.RegistrationRetryDelaySeconds);
        
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                _logger.LogInformation("Registering agent with platform (attempt {Attempt}/{MaxAttempts})", attempt, maxAttempts);
                
                var response = await httpClient.PostAsJsonAsync(
                    $"{platformUrl}/api/clusters/{clusterId}/agents",
                    request,
                    cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var registrationResponse = await response.Content.ReadFromJsonAsync<AgentRegistrationResponse>(cancellationToken);
                    if (registrationResponse == null)
                    {
                        throw new InvalidOperationException("Invalid registration response from platform");
                    }
                    
                    // 存储注册信息
                    _registrationInfo = new AgentRegistrationInfo
                    {
                        AgentId = registrationResponse.AgentId,
                        QuartzInstanceId = registrationResponse.QuartzInstanceId,
                        ClusterId = registrationResponse.ClusterId,
                        RegisteredAt = DateTime.UtcNow,
                        PlatformApiBaseUrl = registrationResponse.PlatformApiBaseUrl,
                        HeartbeatIntervalSeconds = registrationResponse.HeartbeatIntervalSeconds,
                        WarningThresholdSeconds = registrationResponse.WarningThresholdSeconds,
                        OfflineThresholdSeconds = registrationResponse.OfflineThresholdSeconds
                    };
                    
                    _logger.LogInformation(
                        "Agent registered successfully: {AgentId}, QuartzInstanceId: {QuartzInstanceId}, Heartbeat interval: {HeartbeatInterval}s",
                        registrationResponse.AgentId, registrationResponse.QuartzInstanceId, registrationResponse.HeartbeatIntervalSeconds);
                    
                    return registrationResponse;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Registration failed: Invalid API token");
                    throw new UnauthorizedAccessException("Invalid API token");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning("Registration failed (attempt {Attempt}/{MaxAttempts}): {StatusCode} - {Error}", 
                        attempt, maxAttempts, response.StatusCode, error);
                    
                    if (attempt < maxAttempts)
                    {
                        _logger.LogInformation("Retrying registration in {RetryDelay}s", retryDelay.TotalSeconds);
                        await Task.Delay(retryDelay, cancellationToken);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Registration failed after {maxAttempts} attempts: {error}");
                    }
                }
            }
            catch (HttpRequestException ex) when (attempt < maxAttempts)
            {
                _logger.LogWarning(ex, "Registration network error (attempt {Attempt}/{MaxAttempts})", attempt, maxAttempts);
                _logger.LogInformation("Retrying registration in {RetryDelay}s", retryDelay.TotalSeconds);
                await Task.Delay(retryDelay, cancellationToken);
            }
            catch (TaskCanceledException ex) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Registration cancelled");
                throw new OperationCanceledException("Registration cancelled", ex, cancellationToken);
            }
        }
        
        throw new InvalidOperationException($"Registration failed after {maxAttempts} attempts");
    }
    
    public AgentRegistrationInfo? GetRegistrationInfo()
    {
        return _registrationInfo;
    }
    
    public async Task<bool> DeregisterAsync(CancellationToken cancellationToken = default)
    {
        var registrationInfo = _registrationInfo;
        if (registrationInfo == null)
        {
            _logger.LogWarning("No registration information available for deregistration");
            return false;
        }
        
        var apiToken = _config.Platform.ApiToken;
        if (string.IsNullOrEmpty(apiToken))
        {
            _logger.LogError("API Token is not configured for deregistration");
            return false;
        }
        
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("X-Agent-Token", apiToken);
            
            var response = await httpClient.DeleteAsync(
                $"{registrationInfo.PlatformApiBaseUrl}/api/agents/{registrationInfo.AgentId}",
                cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Agent deregistered successfully: {AgentId}", registrationInfo.AgentId);
                _registrationInfo = null;
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Deregistration failed: {StatusCode} - {Error}", response.StatusCode, error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deregister agent");
            return false;
        }
    }
    
    private string GetAgentUrl(AgentConfig config)
    {
        return _urlResolver.Resolve(config.Agent);
    }
    
    private string GetAgentVersion()
    {
        var assembly = System.Reflection.Assembly.GetEntryAssembly();
        var version = assembly?.GetName().Version?.ToString() ?? "1.0.0";
        return version;
    }
    
    private string GenerateQuartzInstanceId(AgentConfig config, string clusterId)
    {
        var machineName = Environment.MachineName.ToLowerInvariant().Replace(" ", "-");
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var randomSuffix = Guid.NewGuid().ToString("N").Substring(0, 4);
        
        return $"{clusterId}-{machineName}-{timestamp}-{randomSuffix}";
    }
}