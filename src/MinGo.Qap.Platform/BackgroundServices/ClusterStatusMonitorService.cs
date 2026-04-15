using MinGo.Qap.Platform.Services;

namespace MinGo.Qap.Platform.BackgroundServices;

/// <summary>
/// Cluster 状态监控服务
/// </summary>
public class ClusterStatusMonitorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ClusterStatusMonitorService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);

    public ClusterStatusMonitorService(
        IServiceProvider serviceProvider,
        ILogger<ClusterStatusMonitorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Cluster status monitor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var clusterService = scope.ServiceProvider.GetRequiredService<ClusterService>();
                
                // 更新所有 Cluster 的状态
                await clusterService.UpdateClusterStatusesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update cluster statuses");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
}
