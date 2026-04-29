using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using YamlDotNet.RepresentationModel;

namespace MinGo.Qap.Agent.Configuration;

/// <summary>
/// Parses YAML streams into flat key-value dictionaries compatible with <see cref="IConfiguration"/>.
/// </summary>
internal static class YamlConfigurationFileParser
{
    private static readonly HashSet<string> NullValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "null",
        "~",
    };

    /// <summary>
    /// Parses a YAML stream and returns a flat dictionary of configuration keys and values.
    /// </summary>
    /// <param name="stream">The YAML input stream.</param>
    /// <returns>A case-insensitive dictionary of configuration key-value pairs.</returns>
    public static IDictionary<string, string?> Parse(Stream stream)
    {
        var data = new SortedDictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
        var yaml = new YamlStream();
        yaml.Load(reader);

        if (yaml.Documents.Count > 0 && yaml.Documents[0].RootNode is YamlMappingNode rootMapping)
        {
            VisitMappingNode(rootMapping, string.Empty, data);
        }

        return data;
    }

    private static void VisitMappingNode(YamlMappingNode node, string prefix, IDictionary<string, string?> data)
    {
        foreach (var child in node.Children)
        {
            var key = child.Key.ToString();
            var fullPath = string.IsNullOrEmpty(prefix)
                ? key
                : $"{prefix}{ConfigurationPath.KeyDelimiter}{key}";

            VisitYamlNode(child.Value, fullPath, data);
        }
    }

    private static void VisitSequenceNode(YamlSequenceNode node, string prefix, IDictionary<string, string?> data)
    {
        var index = 0;
        foreach (var child in node.Children)
        {
            var indexedPath = $"{prefix}{ConfigurationPath.KeyDelimiter}{index}";
            VisitYamlNode(child, indexedPath, data);
            index++;
        }
    }

    private static void VisitYamlNode(YamlNode node, string path, IDictionary<string, string?> data)
    {
        switch (node)
        {
            case YamlScalarNode scalar:
                data[path] = IsNullValue(scalar) ? null : scalar.Value;
                break;

            case YamlMappingNode mapping:
                VisitMappingNode(mapping, path, data);
                break;

            case YamlSequenceNode sequence:
                VisitSequenceNode(sequence, path, data);
                break;
        }
    }

    private static bool IsNullValue(YamlScalarNode scalar)
    {
        // YamlDotNet represents YAML null literals as the string value of the scalar node.
        // YAML spec null values: null, Null, NULL, ~
        return scalar.Value is null || NullValues.Contains(scalar.Value);
    }
}
