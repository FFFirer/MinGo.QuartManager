using System.Diagnostics;
using System.Text.Json;
using MinGo.Qap.Agent.Configuration;
using MinGo.Qap.Shared.Models;

namespace MinGo.Qap.Agent.Services;

/// <summary>
/// Agent 生命周期管理服务：自动注册、定时心跳、优雅关闭
/// </summary>
public class HostedAgentService : BackgroundService
{
    private readonly IAgentRegistrationService _registrationService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<HostedAgentService> _logger;

    private AgentRegistrationInfo? _registrationInfo;
    private TimeSpan _heartbeatInterval = TimeSpan.FromSeconds(30);
    private int _consecutiveHeartbeatFailures;
    private bool _isRegistered;

    private const int MaxConsecutiveFailuresBeforeReRegister = 3;

    public HostedAgentService(
        IAgentRegistrationService registrationService,
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<HostedAgentService> logger)
    {
        _registrationService = registrationService;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("HostedAgentService started");

        // Phase 1: Register with retry
        await RegisterWithRetryAsync(stoppingToken);

        // Phase 2: Heartbeat loop (only if registered)
        while (!stoppingToken.IsCancellationRequested && _isRegistered)
        {
            await SendHeartbeatAsync(stoppingToken);
            await Task.Delay(_heartbeatInterval, stoppingToken);
        }

        _logger.LogInformation("HostedAgentService stopped (IsRegistered: {IsRegistered})", _isRegistered);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("HostedAgentService stopping...");

        // Phase 3: Deregister gracefully
        await DeregisterAsync();

        await base.StopAsync(cancellationToken);
    }

    #region Registration

    private async Task RegisterWithRetryAsync(CancellationToken cancellationToken)
    {
        var config = _configuration.Get<AgentConfig>();
        var maxAttempts = config?.Agent.RegistrationMaxAttempts ?? 5;
        var retryDelay = TimeSpan.FromSeconds(config?.Agent.RegistrationRetryDelaySeconds ?? 5);

        // Read default interval from config as fallback
        if (config?.Agent.HeartbeatIntervalSeconds > 0)
        {
            _heartbeatInterval = TimeSpan.FromSeconds(config.Agent.HeartbeatIntervalSeconds);
        }

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Registration cancelled during retry");
                return;
            }

            try
            {
                _logger.LogInformation("Registering agent with platform (attempt {Attempt}/{MaxAttempts})",
                    attempt, maxAttempts);

                var response = await _registrationService.RegisterAsync(cancellationToken);

                // Store registration info
                _registrationInfo = _registrationService.GetRegistrationInfo();
                _isRegistered = true;
                _consecutiveHeartbeatFailures = 0;

                // Update heartbeat interval from registration response
                if (response.HeartbeatIntervalSeconds > 0)
                {
                    _heartbeatInterval = TimeSpan.FromSeconds(response.HeartbeatIntervalSeconds);
                }

                _logger.LogInformation(
                    "Agent registered successfully: {AgentId}, Heartbeat interval: {Interval}s",
                    response.AgentId, _heartbeatInterval.TotalSeconds);

                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Registration cancelled");
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Registration failed: Invalid API token. No further retries.");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Registration failed (attempt {Attempt}/{MaxAttempts})", attempt, maxAttempts);

                if (attempt < maxAttempts)
                {
                    _logger.LogInformation("Retrying registration in {Delay}s", retryDelay.TotalSeconds);
                    try
                    {
                        await Task.Delay(retryDelay, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogWarning("Registration retry cancelled");
                        return;
                    }
                }
                else
                {
                    _logger.LogError(ex, "Registration failed after {MaxAttempts} attempts. Heartbeat will be skipped.", maxAttempts);
                }
            }
        }
    }

    private async Task DeregisterAsync()
    {
        if (!_isRegistered || _registrationInfo == null)
        {
            _logger.LogDebug("No active registration to deregister");
            return;
        }

        try
        {
            _logger.LogInformation("Deregistering agent: {AgentId}", _registrationInfo.AgentId);
            var success = await _registrationService.DeregisterAsync();
            if (success)
            {
                _logger.LogInformation("Agent deregistered successfully: {AgentId}", _registrationInfo.AgentId);
            }
            else
            {
                _logger.LogWarning("Deregistration returned failure for agent: {AgentId}", _registrationInfo.AgentId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deregister agent: {AgentId}", _registrationInfo.AgentId);
        }
        finally
        {
            _isRegistered = false;
            _registrationInfo = null;
        }
    }

    #endregion

    #region Heartbeat

    private async Task SendHeartbeatAsync(CancellationToken cancellationToken)
    {
        // Refresh registration info
        _registrationInfo = _registrationService.GetRegistrationInfo();
        if (_registrationInfo == null)
        {
            _logger.LogWarning("No registration information available. Heartbeat skipped.");
            return;
        }

        // Update heartbeat interval from registration (dynamic)
        var newInterval = TimeSpan.FromSeconds(_registrationInfo.HeartbeatIntervalSeconds);
        if (_heartbeatInterval != newInterval)
        {
            _logger.LogInformation("Heartbeat interval changed from {OldInterval}s to {NewInterval}s",
                _heartbeatInterval.TotalSeconds, newInterval.TotalSeconds);
            _heartbeatInterval = newInterval;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var quartzService = scope.ServiceProvider.GetRequiredService<IQuartzService>();
            var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

            var schedulerState = await quartzService.GetSchedulerStateAsync();
            var heartbeatRequest = BuildHeartbeatRequest(schedulerState);

            var httpClient = httpClientFactory.CreateClient();
            var response = await httpClient.PostAsJsonAsync(
                $"{_registrationInfo.PlatformApiBaseUrl}/api/agents/{_registrationInfo.AgentId}/heartbeat",
                heartbeatRequest,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var heartbeatResponse = await response.Content.ReadFromJsonAsync<AgentHeartbeatResponse>(cancellationToken);
                if (heartbeatResponse?.Success == true)
                {
                    _logger.LogDebug("Heartbeat sent successfully to agent {AgentId}", _registrationInfo.AgentId);
                    _consecutiveHeartbeatFailures = 0;
                    _isRegistered = true;

                    // Dynamic interval update from heartbeat response
                    if (heartbeatResponse.NextHeartbeatIntervalSeconds > 0 &&
                        heartbeatResponse.NextHeartbeatIntervalSeconds != _heartbeatInterval.TotalSeconds)
                    {
                        _heartbeatInterval = TimeSpan.FromSeconds(heartbeatResponse.NextHeartbeatIntervalSeconds);
                        _logger.LogInformation("Heartbeat interval updated from response: {Interval}s",
                            _heartbeatInterval.TotalSeconds);
                    }
                }
                else
                {
                    var error = heartbeatResponse?.Message ?? "Unknown error";
                    _logger.LogWarning("Heartbeat response indicates failure: {Error}", error);
                    HandleHeartbeatFailure();
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                     response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Heartbeat failed with {StatusCode}: {Error}. Registration may be invalid. Triggering re-registration.",
                    response.StatusCode, error);
                _isRegistered = false;
                await TriggerReRegistrationAsync(cancellationToken);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Heartbeat failed: {StatusCode} - {Error}", response.StatusCode, error);
                HandleHeartbeatFailure(cancellationToken);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while sending heartbeat to platform");
            HandleHeartbeatFailure(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Shutdown in progress
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during heartbeat");
            HandleHeartbeatFailure(cancellationToken);
        }
    }

    private AgentHeartbeatRequest BuildHeartbeatRequest(SchedulerStateDto schedulerState)
    {
        var process = Process.GetCurrentProcess();
        var uptimeSeconds = (long)(DateTime.UtcNow - process.StartTime.ToUniversalTime()).TotalSeconds;

        var agentVersion = GetAgentVersion();

        var metrics = new
        {
            timestamp = DateTime.UtcNow,
            uptimeSeconds,
            schedulerStatus = schedulerState.Status,
            jobCounts = schedulerState.JobCounts,
            system = new
            {
                memoryUsedMb = process.WorkingSet64 / 1024 / 1024,
                memoryTotalMb = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1024 / 1024,
                cpuPercent = 0
            }
        };

        return new AgentHeartbeatRequest
        {
            AgentId = _registrationInfo?.AgentId ?? string.Empty,
            QuartzInstanceId = _registrationInfo?.QuartzInstanceId,
            AgentVersion = agentVersion,
            Status = schedulerState.Status,
            Metrics = JsonSerializer.Serialize(metrics)
        };
    }

    private static string GetAgentVersion()
    {
        var assembly = System.Reflection.Assembly.GetEntryAssembly();
        return assembly?.GetName().Version?.ToString() ?? "1.0.0";
    }

    #endregion

    #region Failure Recovery

    private void HandleHeartbeatFailure(CancellationToken cancellationToken = default)
    {
        _consecutiveHeartbeatFailures++;
        _logger.LogWarning("Heartbeat failure count: {Count}/{Max}",
            _consecutiveHeartbeatFailures, MaxConsecutiveFailuresBeforeReRegister);

        if (_consecutiveHeartbeatFailures >= MaxConsecutiveFailuresBeforeReRegister)
        {
            _logger.LogWarning("Too many consecutive heartbeat failures. Triggering re-registration.");
            _isRegistered = false;

            // Fire-and-forget re-registration (don't block the heartbeat loop)
            _ = Task.Run(async () =>
            {
                try
                {
                    await TriggerReRegistrationAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Re-registration failed after heartbeat failures");
                }
            }, cancellationToken);
        }
    }

    private async Task TriggerReRegistrationAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting re-registration...");
        _isRegistered = false;

        var config = _configuration.Get<AgentConfig>();
        var maxAttempts = config?.Agent.RegistrationMaxAttempts ?? 5;
        var retryDelay = TimeSpan.FromSeconds(config?.Agent.RegistrationRetryDelaySeconds ?? 5);

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            if (cancellationToken.IsCancellationRequested) return;

            try
            {
                var response = await _registrationService.RegisterAsync(cancellationToken);
                _registrationInfo = _registrationService.GetRegistrationInfo();
                _isRegistered = true;
                _consecutiveHeartbeatFailures = 0;

                if (response.HeartbeatIntervalSeconds > 0)
                {
                    _heartbeatInterval = TimeSpan.FromSeconds(response.HeartbeatIntervalSeconds);
                }

                _logger.LogInformation("Re-registration successful: {AgentId}", response.AgentId);
                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogError("Re-registration failed: Invalid API token. No further retries.");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Re-registration failed (attempt {Attempt}/{MaxAttempts})", attempt, maxAttempts);

                if (attempt < maxAttempts && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(retryDelay, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                }
            }
        }

        _logger.LogError("Re-registration failed after {MaxAttempts} attempts", maxAttempts);
    }

    #endregion
}
