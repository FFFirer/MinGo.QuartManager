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
    Task<RegisterAgentResponse> RegisterAsync(CancellationToken cancellationToken = default);

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
    /// Agent 显示名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Quartz 实例 ID
    /// </summary>
    public string? QuartzInstanceId { get; set; }

    /// <summary>
    /// 注册时间
    /// </summary>
    public DateTimeOffset RegisteredAt { get; set; }

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

    /// <summary>
    /// API Token
    /// </summary>
    public string? Token { get; set; }
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
    private readonly IAgentIdentityStore _identityStore;
    private AgentRegistrationInfo? _registrationInfo;

    public AgentRegistrationService(
        IOptions<AgentConfig> options,
        ILogger<AgentRegistrationService> logger,
        IHttpClientFactory httpClientFactory,
        AgentUrlResolver urlResolver,
        IAgentIdentityStore identityStore)
    {
        _config = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _urlResolver = urlResolver;
        _identityStore = identityStore;
    }

    public async Task<RegisterAgentResponse> RegisterAsync(CancellationToken cancellationToken = default)
    {
        var platformUrl = _config.Platform.Url;
        var apiToken = _config.Platform.ApiToken;

        if (string.IsNullOrEmpty(platformUrl))
        {
            throw new InvalidOperationException("Platform URL is not configured");
        }

        if (string.IsNullOrEmpty(apiToken))
        {
            throw new InvalidOperationException("API Token is not configured");
        }

        // 获取 Agent URL（从配置或推导）
        var agentUrl = GetAgentUrl(_config);

        // 读取本地存储的 AgentIdentity（重连时使用）
        var identity = _identityStore.Load();

        var request = new RegisterAgentRequest
        {
            AgentId = identity?.AgentId,  // null=首次注册, 有值=重连
            Name = _config.Agent.Id ?? $"agent-{Environment.MachineName.ToLowerInvariant()}",
            Url = agentUrl,
            AgentVersion = GetAgentVersion(),
            StartedAt = DateTimeOffset.UtcNow  // 使用 UTC 时间
        };

        var httpClient = _httpClientFactory.CreateClient("PlatformApi");

        var maxAttempts = _config.Agent.RegistrationMaxAttempts;
        var retryDelay = TimeSpan.FromSeconds(_config.Agent.RegistrationRetryDelaySeconds);

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                _logger.LogInformation("Registering agent with platform (attempt {Attempt}/{MaxAttempts})", attempt, maxAttempts);

                // 2.1.1: 新端点 POST /api/agents
                var response = await httpClient.PostAsJsonAsync(
                    $"{platformUrl.TrimEnd('/')}/api/agents",
                    request,
                    MinGoJsonDefaults.Options,
                    cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var registrationResponse = await response.Content.ReadFromJsonAsync<RegisterAgentResponse>(MinGoJsonDefaults.Options, cancellationToken);
                    if (registrationResponse == null)
                    {
                        throw new InvalidOperationException("Invalid registration response from platform");
                    }

                    // 存储注册信息
                    _registrationInfo = new AgentRegistrationInfo
                    {
                        AgentId = registrationResponse.AgentId,
                        Name = request.Name,
                        RegisteredAt = DateTimeOffset.UtcNow,
                        PlatformApiBaseUrl = platformUrl,
                        HeartbeatIntervalSeconds = registrationResponse.HeartbeatIntervalSeconds,
                        WarningThresholdSeconds = registrationResponse.WarningThresholdSeconds,
                        OfflineThresholdSeconds = registrationResponse.OfflineThresholdSeconds,
                        Token = apiToken
                    };

                    _logger.LogInformation(
                        "Agent registered successfully: {AgentId}, Heartbeat interval: {HeartbeatInterval}s",
                        registrationResponse.AgentId, registrationResponse.HeartbeatIntervalSeconds);

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
            var httpClient = _httpClientFactory.CreateClient("PlatformApi");

            var response = await httpClient.DeleteAsync(
                $"{registrationInfo.PlatformApiBaseUrl.TrimEnd('/')}/api/agents/{registrationInfo.AgentId}",
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
}
