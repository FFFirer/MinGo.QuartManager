using Microsoft.Extensions.Configuration;

namespace MinGo.Qap.Agent.Configuration;

/// <summary>
/// A YAML-based <see cref="FileConfigurationProvider"/> implementation.
/// </summary>
public class YamlConfigurationProvider : FileConfigurationProvider
{
    /// <summary>
    /// Initializes a new instance with the specified source.
    /// </summary>
    /// <param name="source">The source settings.</param>
    public YamlConfigurationProvider(YamlConfigurationSource source) : base(source)
    {
    }

    /// <inheritdoc />
    public override void Load(Stream stream)
    {
        Data = YamlConfigurationFileParser.Parse(stream);
    }
}
