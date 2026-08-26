using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MinGo.Qap.Shared;

namespace MinGo.Qap.Agent.Services;

/// <summary>
/// 在 Agent 启动时注册 OTel Observable Gauges。
/// 这些 Gauge 通过回调函数定期从 DI 容器获取实时数据。
/// </summary>
public class AgentTelemetryHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public AgentTelemetryHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // 注册 Agent 端 Observable Gauges
        QapTelemetry.Meter.CreateObservableGauge<int>("qap.logs.buffered", () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var logService = scope.ServiceProvider.GetService<ILogCollectionService>();
            return logService?.BufferedCount ?? 0;
        });

        QapTelemetry.Meter.CreateObservableGauge<int>("qap.schedulers.managed", () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var accessor = scope.ServiceProvider.GetService<IAgentSchedulerAccessor>();
            return accessor?.GetAll().Count ?? 0;
        });

        QapTelemetry.Meter.CreateObservableGauge<int>("qap.schedulers.running_jobs", () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var accessor = scope.ServiceProvider.GetService<IAgentSchedulerAccessor>();
            if (accessor == null) return 0;

            var total = 0;
            foreach (var scheduler in accessor.GetAll().Values)
            {
                try
                {
                    total += scheduler.GetCurrentlyExecutingJobs().Result.Count;
                }
                catch
                {
                    // Scheduler 可能尚未就绪
                }
            }
            return total;
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
