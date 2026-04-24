using MinGo.Qap.Agent.Configuration;
using MinGo.Qap.Agent.Quartz;
using MinGo.Qap.Agent.Services;
using MinGo.Qap.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

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

        // Register core services
        services.AddSingleton<IJobRegistry, JobRegistry>();
        services.AddSingleton<IJobConverter, JobConverter>();
        services.AddSingleton<IAgentRegistrationService, AgentRegistrationService>();

        // Register JobManifest from configuration
        services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<AgentConfig>();
            var manifest = new JobManifestDto
            {
                ClusterId = config.Agent.ClusterId,
                Jobs = new List<JobTypeInfoDto>()
            };

            foreach (var jobType in config.Quartz.JobTypes)
            {
                manifest.Jobs.Add(new JobTypeInfoDto
                {
                    Key = jobType.Split('.').Last(),
                    Description = jobType,
                    Parameters = new List<ParameterInfoDto>()
                });
            }

            return manifest;
        });

        return services;
    }

    /// <summary>
    /// Adds the Quartz hosted service to start the scheduler.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddQuartzHostedService(this IServiceCollection services)
    {
        services.AddSingleton<IScheduler>(sp =>
        {
            var configSection = sp.GetRequiredService<IConfiguration>().GetSection("quartz");
            var logger = sp.GetRequiredService<ILogger<SchedulerInitializer>>();
            var initializer = new SchedulerInitializer(configSection, logger);
            return initializer.InitializeAsync().GetAwaiter().GetResult();
        });

        return services;
    }
}