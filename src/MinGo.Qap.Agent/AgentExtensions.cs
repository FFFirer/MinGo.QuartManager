using MinGo.Qap.Agent.Configuration;
using MinGo.Qap.Agent.Services;
using MinGo.Qap.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MinGo.Qap.Agent;

/// <summary>
/// Extension methods for adding MinGo Agent to an ASP.NET Core application.
/// </summary>
public static class AgentExtensions
{
    /// <summary>
    /// Adds MinGo Agent services to the service collection using YAML configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="configPath">Path to the YAML configuration file.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMinGoAgent(
        this IServiceCollection services,
        IConfiguration configuration,
        string configPath = "config.yaml")
    {
        var configLoader = new ConfigLoader(configuration);
        var agentConfig = configLoader.Load(configPath);

        // Register configuration as singleton
        services.AddSingleton(agentConfig);

        // Register HTTP client for Platform communication
        services.AddHttpClient();

        // Register discovery service (needed for manifest generation)
        services.AddSingleton<IJobDiscoveryService>(sp =>
        {
            var cfg = sp.GetRequiredService<AgentConfig>();
            var logger = sp.GetRequiredService<ILogger<JobDiscoveryService>>();
            return new JobDiscoveryService(cfg, logger);
        });

        // Register JobManifest from configuration with parameter discovery
        services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<AgentConfig>();
            var discovery = sp.GetRequiredService<IJobDiscoveryService>();
            var logger = sp.GetRequiredService<ILogger<JobDiscoveryService>>();

            var manifest = new JobManifestDto
            {
                ClusterId = config.Agent.ClusterId,
                Jobs = new List<JobTypeInfoDto>()
            };

            // Use discovery service to get full parameter metadata
            var discovered = discovery.DiscoverFromConfigAsync().GetAwaiter().GetResult();
            foreach (var job in discovered)
            {
                manifest.Jobs.Add(new JobTypeInfoDto
                {
                    Key = job.JobKey,
                    JobTypeFullName = job.JobTypeFullName,
                    Description = job.Description ?? job.JobTypeFullName ?? string.Empty,
                    Parameters = job.Parameters
                });
            }

            // Fallback: add config types that couldn't be discovered (no assembly loaded)
            foreach (var jobType in config.Quartz.JobTypes)
            {
                var key = jobType.Split('.').Last();
                if (!manifest.Jobs.Any(j => j.Key == key))
                {
                    manifest.Jobs.Add(new JobTypeInfoDto
                    {
                        Key = key,
                        Description = jobType,
                        Parameters = new List<ParameterInfoDto>()
                    });
                    logger.LogWarning("Job type {JobType} not discoverable; add assembly reference to enable parameter metadata", jobType);
                }
            }

            return manifest;
        });

        // Register core services
        services.AddSingleton<IJobRegistry>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<JobRegistry>>();
            var manifest = sp.GetService<JobManifestDto>();
            return new JobRegistry(logger, manifest);
        });
        services.AddSingleton<IJobConverter, JobConverter>();
        services.AddSingleton<AgentUrlResolver>();
        services.AddSingleton<IAgentRegistrationService, AgentRegistrationService>();
        services.AddSingleton<ILogCollectionService, LogCollectionService>();

        return services;
    }

    /// <summary>
    /// 极速初始化：注册最小的 Agent 服务以便快速启动日志收集与作业发现等能力
    /// </summary>
    public static IServiceCollection UseMinGoAgent(this IServiceCollection services)
    {
        // 注册基础日志收集能力
        services.AddSingleton<ILogCollectionService, LogCollectionService>();
        // 注册 JobDiscoveryService 通过工厂注入 AgentConfig
        services.AddSingleton<IJobDiscoveryService>(sp =>
        {
            var cfg = sp.GetRequiredService<AgentConfig>();
            var logger = sp.GetRequiredService<ILogger<JobDiscoveryService>>();
            return new JobDiscoveryService(cfg, logger);
        });
        return services;
    }

}
