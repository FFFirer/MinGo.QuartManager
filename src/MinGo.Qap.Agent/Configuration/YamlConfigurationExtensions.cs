using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace MinGo.Qap.Agent.Configuration;

/// <summary>
/// Extension methods for adding YAML configuration support.
/// </summary>
public static class YamlConfigurationExtensions
{
    /// <summary>
    /// Adds a YAML configuration source for the specified file.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="path">Path relative to the base path stored in <paramref name="builder"/> of the file to be read.</param>
    /// <param name="optional">Whether the file is optional.</param>
    /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddYamlFile(
        this IConfigurationBuilder builder,
        string path,
        bool optional = false,
        bool reloadOnChange = false)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return builder.AddYamlFile(provider: null, path, optional, reloadOnChange);
    }

    /// <summary>
    /// Adds a YAML configuration source for the specified file using an <see cref="IFileProvider"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="provider">The <see cref="IFileProvider"/> to use to access the file.</param>
    /// <param name="path">Path relative to the base path stored in <paramref name="provider"/> of the file to be read.</param>
    /// <param name="optional">Whether the file is optional.</param>
    /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddYamlFile(
        this IConfigurationBuilder builder,
        IFileProvider? provider,
        string path,
        bool optional = false,
        bool reloadOnChange = false)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return builder.Add(new YamlConfigurationSource
        {
            FileProvider = provider,
            Path = path,
            Optional = optional,
            ReloadOnChange = reloadOnChange,
        });
    }

    /// <summary>
    /// Adds a YAML configuration source for the specified <see cref="YamlConfigurationSource"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="source">The <see cref="YamlConfigurationSource"/> to add.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder Add(
        this IConfigurationBuilder builder,
        YamlConfigurationSource source)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(source);

        return builder.Add((IConfigurationSource)source);
    }
}
