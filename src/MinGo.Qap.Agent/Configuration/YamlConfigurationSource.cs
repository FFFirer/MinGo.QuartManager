using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace MinGo.Qap.Agent.Configuration;

/// <summary>
/// Represents a YAML file as an <see cref="IConfigurationSource"/>.
/// </summary>
public class YamlConfigurationSource : FileConfigurationSource
{
    /// <inheritdoc />
    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        EnsureDefaults(builder);
        return new YamlConfigurationProvider(this);
    }
}
