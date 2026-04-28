using MinGo.Qap.Shared.Models;
using System.Diagnostics;
using System.Text.Json;

namespace MinGo.Qap.Agent.Services;

/// <summary>
/// 心跳服务
/// </summary>
[Obsolete("Use HostedAgentService instead, which handles auto-registration, heartbeat, and graceful shutdown.")]
public class HeartbeatService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HeartbeatService> _logger;
    private TimeSpan _interval = TimeSpan.FromSeconds(30);
    private bool _hasValidRegistration = false;
    
    public HeartbeatService(
        IServiceProvider serviceProvider,
        ILogger<HeartbeatService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Heartbeat service started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendHeartbeatAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send heartbeat");
            }
            
            await Task.Delay(_interval, stoppingToken);
        }
        
        _logger.LogInformation("Heartbeat service stopped");
    }
    
    private async Task SendHeartbeatAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        
        var registrationService = scope.ServiceProvider.GetRequiredService<IAgentRegistrationService>();
        var quartzService = scope.ServiceProvider.GetRequiredService<IQuartzService>();
        var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        
        var registrationInfo = registrationService.GetRegistrationInfo();
        if (registrationInfo == null)
        {
            if (!_hasValidRegistration)
            {
                _logger.LogWarning("No registration information available. Heartbeat skipped.");
            }
            else
            {
                _logger.LogError("Registration information lost. Heartbeat skipped.");
                _hasValidRegistration = false;
            }
            return;
        }
        
        // 更新心跳间隔（如果与当前不同）
        var newInterval = TimeSpan.FromSeconds(registrationInfo.HeartbeatIntervalSeconds);
        if (_interval != newInterval)
        {
            _logger.LogInformation("Heartbeat interval changed from {OldInterval}s to {NewInterval}s", 
                _interval.TotalSeconds, newInterval.TotalSeconds);
            _interval = newInterval;
        }
        
        // 收集心跳数据
        var schedulerState = await quartzService.GetSchedulerStateAsync();
        var heartbeatRequest = BuildHeartbeatRequest(schedulerState);
        
        // 发送心跳到实例级别端点
        var platformUrl = registrationInfo.PlatformApiBaseUrl;
        var agentId = registrationInfo.AgentId;
        
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                $"{platformUrl}/api/agents/{agentId}/heartbeat",
                heartbeatRequest,
                cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var heartbeatResponse = await response.Content.ReadFromJsonAsync<AgentHeartbeatResponse>(cancellationToken);
                if (heartbeatResponse?.Success == true)
                {
                    _logger.LogDebug("Heartbeat sent successfully to agent {AgentId}", agentId);
                    _hasValidRegistration = true;
                }
                else
                {
                    var error = heartbeatResponse?.Message ?? "Unknown error";
                    _logger.LogWarning("Heartbeat response indicates failure: {Error}", error);
                    _hasValidRegistration = false;
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Heartbeat failed: {StatusCode} - {Error}", response.StatusCode, error);
                _hasValidRegistration = false;
                
                // 如果是未授权或未找到，可能需要重新注册
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
                    response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Heartbeat endpoint rejected the request. Registration may be invalid.");
                }
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while sending heartbeat to platform");
            _hasValidRegistration = false;
        }
    }
    
    private AgentHeartbeatRequest BuildHeartbeatRequest(SchedulerStateDto schedulerState)
    {
        var process = Process.GetCurrentProcess();
        var uptimeSeconds = (long)(DateTime.UtcNow - process.StartTime.ToUniversalTime()).TotalSeconds;
        
        // 构建指标 JSON
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
            AgentId = GetAgentIdFromRegistration(),
            QuartzInstanceId = GetQuartzInstanceId(),
            AgentVersion = GetAgentVersion(),
            Status = schedulerState.Status,
            Metrics = JsonSerializer.Serialize(metrics)
        };
    }
    
    private string GetAgentIdFromRegistration()
    {
        using var scope = _serviceProvider.CreateScope();
        var registrationService = scope.ServiceProvider.GetRequiredService<IAgentRegistrationService>();
        var registrationInfo = registrationService.GetRegistrationInfo();
        return registrationInfo?.AgentId ?? string.Empty;
    }
    
    private string GetAgentVersion()
    {
        var assembly = System.Reflection.Assembly.GetEntryAssembly();
        var version = assembly?.GetName().Version?.ToString() ?? "1.0.0";
        return version;
    }
    
    private string? GetQuartzInstanceId()
    {
        using var scope = _serviceProvider.CreateScope();
        var registrationService = scope.ServiceProvider.GetRequiredService<IAgentRegistrationService>();
        var registrationInfo = registrationService.GetRegistrationInfo();
        return registrationInfo?.QuartzInstanceId;
    }
}