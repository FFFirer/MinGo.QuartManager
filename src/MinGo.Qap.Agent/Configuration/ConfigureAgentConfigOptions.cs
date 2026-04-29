using Microsoft.Extensions.Options;

namespace MinGo.Qap.Agent.Configuration;

/// <summary>
/// Configures AgentConfig by binding from ASP.NET Core IConfiguration sections.
/// </summary>
public class ConfigureAgentConfigOptions : IConfigureOptions<AgentConfig>
{
    private readonly IConfiguration _configuration;

    public ConfigureAgentConfigOptions(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(AgentConfig config)
    {
        // Bind from configuration sections
        if (_configuration.GetSection("agent").Exists())
        {
            _configuration.GetSection("agent").Bind(config.Agent);
        }

        if (_configuration.GetSection("platform").Exists())
        {
            _configuration.GetSection("platform").Bind(config.Platform);
        }

        if (_configuration.GetSection("quartz").Exists())
        {
            _configuration.GetSection("quartz").Bind(config.Quartz);
        }

        // Logging is optional; provide default
        config.Logging = _configuration.GetSection("logging").Get<LoggingSettings>() ?? new LoggingSettings();
    }
}
