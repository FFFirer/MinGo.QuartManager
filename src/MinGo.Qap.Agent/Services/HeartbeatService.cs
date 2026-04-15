using MinGo.Qap.Shared.Models;
using System.Diagnostics;

namespace MinGo.Qap.Agent.Services;

/// <summary>
/// 心跳服务
/// </summary>
public class HeartbeatService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _config;
    private readonly ILogger<HeartbeatService> _logger;
    private readonly TimeSpan _interval;

    public HeartbeatService(
        IServiceProvider serviceProvider,
        IConfiguration config,
        ILogger<HeartbeatService> logger)
    {
        _serviceProvider = serviceProvider;
        _config = config;
        _logger = logger;
        
        // 从配置读取心跳间隔，默认 30 秒
        var intervalSeconds = _config.GetValue<int?>("agent:heartbeatIntervalSeconds") ?? 30;
        _interval = TimeSpan.FromSeconds(intervalSeconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Heartbeat service started. Interval: {Interval}s", _interval.TotalSeconds);

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
        
        var quartzService = scope.ServiceProvider.GetRequiredService<IQuartzService>();
        var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var platformUrl = config["platform:url"] ?? "http://localhost:5000";
        var clusterId = config["agent:clusterId"] ?? throw new InvalidOperationException("ClusterId not configured");

        // 收集心跳数据
        var schedulerState = await quartzService.GetSchedulerStateAsync();
        var heartbeat = BuildHeartbeat(schedulerState);

        // 发送心跳
        var response = await httpClient.PostAsJsonAsync(
            $"{platformUrl}/api/clusters/{clusterId}/heartbeat",
            heartbeat,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogDebug("Heartbeat sent successfully");
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Heartbeat failed: {StatusCode} - {Error}", response.StatusCode, error);
        }
    }

    private HeartbeatDto BuildHeartbeat(SchedulerStateDto schedulerState)
    {
        var process = Process.GetCurrentProcess();
        
        return new HeartbeatDto
        {
            Timestamp = DateTime.UtcNow,
            AgentVersion = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
            UptimeSeconds = (long)(DateTime.UtcNow - process.StartTime.ToUniversalTime()).TotalSeconds,
            SchedulerStatus = schedulerState.Status,
            Jobs = schedulerState.JobCounts,
            System = new SystemMetricsDto
            {
                MemoryUsedMb = process.WorkingSet64 / 1024 / 1024,
                MemoryTotalMb = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1024 / 1024,
                CpuPercent = 0 // V1 简化，不实现 CPU 监控
            }
        };
    }
}
