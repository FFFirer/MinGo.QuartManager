using Microsoft.Extensions.Options;

namespace MinGo.Qap.Agent.Configuration;

/// <summary>
/// Validates AgentConfig to ensure required fields are set.
/// </summary>
public class ValidateAgentConfigOptions : IValidateOptions<AgentConfig>
{
    public ValidateOptionsResult Validate(string? name, AgentConfig config)
    {
        var errors = new List<string>();

        // Validate ClusterId
        if (string.IsNullOrWhiteSpace(config.Agent.ClusterId))
        {
            errors.Add("Agent.ClusterId is required. Set it in config.yaml, appsettings.json, or via QAP_AGENT_CLUSTER_ID environment variable.");
        }

        // Validate Platform URL
        if (string.IsNullOrWhiteSpace(config.Platform.Url))
        {
            errors.Add("Platform.Url is required. Set it in config.yaml, appsettings.json, or via environment variable.");
        }
        else if (!Uri.TryCreate(config.Platform.Url, UriKind.Absolute, out _))
        {
            errors.Add($"Platform.Url is not a valid URL: {config.Platform.Url}");
        }

        // Validate Port
        if (config.Agent.Port < 1 || config.Agent.Port > 65535)
        {
            errors.Add($"Agent.Port must be between 1 and 65535. Current value: {config.Agent.Port}");
        }

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
