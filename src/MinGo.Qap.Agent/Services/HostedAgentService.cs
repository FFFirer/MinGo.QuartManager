using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Options;
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
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<AgentConfig> _options;
    private readonly ILogger<HostedAgentService> _logger;
    private readonly IAgentIdentityStore _identityStore;

    private AgentRegistrationInfo? _registrationInfo;
    private TimeSpan _heartbeatInterval = TimeSpan.FromSeconds(30);
    private int _consecutiveHeartbeatFailures;
    private bool _isRegistered;

    private const int MaxConsecutiveFailuresBeforeReRegister = 3;

    public HostedAgentService(
        IAgentRegistrationService registrationService,
        IServiceProvider serviceProvider, 
        IServiceScopeFactory scopeFactory,
        IOptions<AgentConfig> options,
        ILogger<HostedAgentService> logger,
        IAgentIdentityStore identityStore)
    {
        _registrationService = registrationService;
        _serviceProvider = serviceProvider;
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
        _identityStore = identityStore;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("HostedAgentService started");

        // Phase 1: 读取本地身份
        var identity = _identityStore.Load();
        if (identity != null)
        {
            _logger.LogInformation("Loaded existing agent identity: {AgentId}, registered at {RegisteredAt:O}",
                identity.AgentId, identity.RegisteredAt);
        }
        else
        {
            _logger.LogInformation("No existing agent identity found, will register as new agent");
        }

        // Phase 2: 注册（携带 AgentId 如果存在）
        await RegisterWithRetryAsync(stoppingToken);

        // Phase 3: 如果注册成功，持久化身份并上报 Scheduler
        if (_isRegistered && _registrationInfo != null)
        {
            // 持久化 AgentId
            var newIdentity = new AgentIdentity
            {
                AgentId = _registrationInfo.AgentId,
                RegisteredAt = DateTimeOffset.UtcNow,
                LastUpdatedAt = DateTimeOffset.UtcNow
            };
            _identityStore.Save(newIdentity);
            _logger.LogInformation("Agent identity persisted: {AgentId}", newIdentity.AgentId);

            // Phase 4: 上报 Scheduler 信息
            await ReportSchedulersAsync(stoppingToken);
        }

        // Phase 5: 心跳循环
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

        // Phase 6: 优雅注销
        await DeregisterAsync();

        await base.StopAsync(cancellationToken);
    }

    #region Registration

    private async Task RegisterWithRetryAsync(CancellationToken cancellationToken)
    {
        var config = _options.Value;
        var maxAttempts = config.Agent.RegistrationMaxAttempts;
        var retryDelay = TimeSpan.FromSeconds(config.Agent.RegistrationRetryDelaySeconds);

        // Read default interval from config as fallback
        if (config.Agent.HeartbeatIntervalSeconds > 0)
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
                _isRegistered = !string.IsNullOrWhiteSpace(_registrationInfo?.AgentId);
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

    #region Scheduler Reporting

    private async Task ReportSchedulersAsync(CancellationToken cancellationToken)
    {
        if (_registrationInfo == null)
        {
            _logger.LogWarning("Cannot report schedulers: not registered");
            return;
        }

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var reporter = scope.ServiceProvider.GetRequiredService<SchedulerReporterService>();

            var success = await reporter.ReportAsync(
                _registrationInfo.PlatformApiBaseUrl,
                _registrationInfo.AgentId,
                _registrationInfo.Token ?? string.Empty);

            if (success)
            {
                _logger.LogInformation("Schedulers reported successfully");
            }
            else
            {
                _logger.LogWarning("Failed to report schedulers");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reporting schedulers");
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
            var schedulerAccessor = scope.ServiceProvider.GetRequiredService<IAgentSchedulerAccessor>();
            var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

            // 获取所有 Scheduler 的状态摘要
            var schedulerSummaries = GetSchedulerSummaries(schedulerAccessor);

            var heartbeatRequest = new AgentHeartbeatRequestV2
            {
                AgentId = _registrationInfo.AgentId,
                Status = "Online",
                Timestamp = DateTimeOffset.UtcNow,  // 使用 UTC 时间
                SchedulerSummaries = schedulerSummaries
            };

            var httpClient = httpClientFactory.CreateClient("PlatformApi");
            var response = await httpClient.PostAsJsonAsync(
                $"{_registrationInfo.PlatformApiBaseUrl.TrimEnd('/')}/api/agents/{_registrationInfo.AgentId}/heartbeat",
                heartbeatRequest,
                MinGoJsonDefaults.Options,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var heartbeatResponse = await response.Content.ReadFromApiResponseAsync<AgentHeartbeatResponseV2>(MinGoJsonDefaults.Options, cancellationToken);
                _logger.LogDebug("Heartbeat sent successfully to agent {AgentId}", _registrationInfo.AgentId);
                _consecutiveHeartbeatFailures = 0;
                _isRegistered = true;

                // 检查是否需要重新上报 Scheduler
                if (heartbeatResponse?.ShouldReportSchedulers == true)
                {
                    _logger.LogInformation("Platform requested scheduler re-report");
                    await ReportSchedulersAsync(cancellationToken);
                }

                // Dynamic interval update from heartbeat response
                if (heartbeatResponse?.NextHeartbeatIntervalSeconds > 0 &&
                    heartbeatResponse.NextHeartbeatIntervalSeconds.Value != (int)_heartbeatInterval.TotalSeconds)
                {
                    _heartbeatInterval = TimeSpan.FromSeconds(heartbeatResponse.NextHeartbeatIntervalSeconds.Value);
                    _logger.LogInformation("Heartbeat interval updated from response: {Interval}s",
                        _heartbeatInterval.TotalSeconds);
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

    private List<SchedulerStatusSummary> GetSchedulerSummaries(IAgentSchedulerAccessor schedulerAccessor)
    {
        var summaries = new List<SchedulerStatusSummary>();

        try
        {
            var schedulers = schedulerAccessor.GetAll();
            foreach (var kvp in schedulers)
            {
                try
                {
                    var scheduler = kvp.Value;
                    var currentlyExecuting = scheduler.GetCurrentlyExecutingJobs().Result.Count;

                    summaries.Add(new SchedulerStatusSummary
                    {
                        SchedulerName = scheduler.SchedulerName,
                        Status = scheduler.IsStarted && !scheduler.InStandbyMode ? "running" : "standby",
                        JobCount = scheduler.GetJobKeys(Quartz.Impl.Matchers.GroupMatcher<Quartz.JobKey>.AnyGroup()).Result.Count,
                        RunningJobCount = currentlyExecuting
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get status for scheduler {SchedulerName}", kvp.Key);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get scheduler summaries");
        }

        return summaries;
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

        var config = _options.Value;
        var maxAttempts = config.Agent.RegistrationMaxAttempts;
        var retryDelay = TimeSpan.FromSeconds(config.Agent.RegistrationRetryDelaySeconds);

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

                // 重新上报 Scheduler
                await ReportSchedulersAsync(cancellationToken);

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
