using Microsoft.Extensions.Options;
using MinGo.Qap.Agent.Configuration;
using MinGo.Qap.Agent.Services;
using MinGo.Qap.Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;

namespace MinGo.Qap.Agent;

/// <summary>
/// Extension methods for adding MinGo Agent to an ASP.NET Core application.
/// </summary>
public static class AgentExtensions
{
    /// <summary>
    /// Adds MinGo Agent services using the standard ASP.NET Core configuration pipeline.
    /// Registers <c>config.yaml</c> as an optional YAML configuration source.
    /// Uses <c>IOptions&lt;AgentConfig&gt;</c> with validation and defaults.
    /// </summary>
    /// <typeparam name="T">The host application builder type.</typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static T AddMinGoAgent<T>(this T builder) where T : IHostApplicationBuilder
    {
        builder.Configuration.AddYamlFile("config.yaml", optional: true);
        
        // Register IOptions<AgentConfig> with binding, defaults, and validation
        builder.Services.ConfigureOptions<ConfigureAgentConfigOptions>();
        builder.Services.ConfigureOptions<PostConfigureAgentConfigOptions>();
        builder.Services.AddSingleton<IValidateOptions<AgentConfig>, ValidateAgentConfigOptions>();

        RegisterAgentServices(builder.Services);

        return builder;   
    }

    /// <summary>
    /// Registers the core Agent services into the service collection.
    /// </summary>
    private static void RegisterAgentServices(IServiceCollection services)
    {
        // Register HTTP client for Platform communication
        services.AddHttpClient();

        // Register discovery service (needed for manifest generation)
        services.AddSingleton<IJobDiscoveryService, JobDiscoveryService>();

        // Register JobManifest from configuration with parameter discovery
        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AgentConfig>>();
            var config = options.Value;
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

        // Register Quartz service facade (resolved lazily; IScheduler must be registered by host app)
        services.AddSingleton<IQuartzService, QuartzService>();

        // Register hosted agent lifecycle service (auto-register, heartbeat, graceful shutdown)
        services.AddHostedService<HostedAgentService>();
    }

    /// <summary>
    /// 极速初始化：注册最小的 Agent 服务以便快速启动日志收集与作业发现等能力
    /// </summary>
    public static IServiceCollection UseMinGoAgent(this IServiceCollection services)
    {
        // 注册基础日志收集能力
        services.AddSingleton<ILogCollectionService, LogCollectionService>();
        // 注册 JobDiscoveryService 通过工厂注入 IOptions<AgentConfig>
        services.AddSingleton<IJobDiscoveryService>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AgentConfig>>();
            var logger = sp.GetRequiredService<ILogger<JobDiscoveryService>>();
            return new JobDiscoveryService(options, logger);
        });
        return services;
    }

}
