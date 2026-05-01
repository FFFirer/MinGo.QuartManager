using Microsoft.Extensions.Options;

namespace MinGo.Qap.Agent.Configuration;

/// <summary>
/// Applies default values to AgentConfig after standard configuration.
/// </summary>
public class PostConfigureAgentConfigOptions : IPostConfigureOptions<AgentConfig>
{
    public void PostConfigure(string? name, AgentConfig config)
    {
        // Note: AgentId is now assigned by Platform and persisted locally.
        // config.Agent.Id is used only as an optional display name.

        // Ensure collections are not null
        config.Quartz.JobTypes ??= [];
        config.Quartz.Properties ??= [];
        config.Logging ??= new LoggingSettings();
    }
}
