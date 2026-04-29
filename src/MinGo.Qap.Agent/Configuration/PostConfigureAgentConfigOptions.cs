using Microsoft.Extensions.Options;

namespace MinGo.Qap.Agent.Configuration;

/// <summary>
/// Applies default values to AgentConfig after standard configuration.
/// </summary>
public class PostConfigureAgentConfigOptions : IPostConfigureOptions<AgentConfig>
{
    public void PostConfigure(string? name, AgentConfig config)
    {
        // Generate default Agent ID if not set
        if (string.IsNullOrWhiteSpace(config.Agent.Id))
        {
            config.Agent.Id = $"agent-{Guid.NewGuid().ToString()[..8]}";
        }

        // Ensure collections are not null
        config.Quartz.JobTypes ??= [];
        config.Quartz.Properties ??= [];
        config.Logging ??= new LoggingSettings();
    }
}
