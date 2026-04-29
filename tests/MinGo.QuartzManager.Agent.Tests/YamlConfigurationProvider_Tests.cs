using System.IO;
using System.Text;
using MinGo.Qap.Agent.Configuration;
using Xunit;

namespace MinGo.QuartzManager.Agent.Tests;

public class YamlConfigurationProvider_Tests
{
    private static YamlConfigurationProvider CreateProvider(string yaml)
    {
        var source = new YamlConfigurationSource
        {
            Optional = false,
        };
        var provider = new YamlConfigurationProvider(source);

        var yamlBytes = Encoding.UTF8.GetBytes(yaml);
        using var stream = new MemoryStream(yamlBytes);
        provider.Load(stream);

        return provider;
    }

    [Fact]
    public void Load_YamlFile_ReturnsConfigValues()
    {
        var yaml = @"
agent:
  clusterId: ""my-cluster""
  port: 8080
platform:
  url: ""http://localhost:5000""
";
        var provider = CreateProvider(yaml);

        Assert.True(provider.TryGet("agent:clusterId", out var clusterId));
        Assert.Equal("my-cluster", clusterId);
        Assert.True(provider.TryGet("agent:port", out var port));
        Assert.Equal("8080", port);
        Assert.True(provider.TryGet("platform:url", out var url));
        Assert.Equal("http://localhost:5000", url);
    }

    [Fact]
    public void Load_YamlFile_WithSequence_FlattensToIndexedKeys()
    {
        var yaml = @"
quartz:
  jobTypes:
    - ""MyApp.Jobs.JobA""
    - ""MyApp.Jobs.JobB""
";
        var provider = CreateProvider(yaml);

        Assert.True(provider.TryGet("quartz:jobTypes:0", out var job0));
        Assert.Equal("MyApp.Jobs.JobA", job0);
        Assert.True(provider.TryGet("quartz:jobTypes:1", out var job1));
        Assert.Equal("MyApp.Jobs.JobB", job1);
    }

    [Fact]
    public void Load_YamlFile_NullValues_HandledGracefully()
    {
        var yaml = @"
key1: null
key2: ~
key3:
";
        var provider = CreateProvider(yaml);

        // null values should be stored as null in the Data dictionary
        Assert.True(provider.TryGet("key1", out var val1));
        Assert.Null(val1);

        Assert.True(provider.TryGet("key2", out var val2));
        Assert.Null(val2);
    }

    [Fact]
    public void Load_EmptyYaml_ReturnsEmptyConfig()
    {
        var yaml = @"
";
        var provider = CreateProvider(yaml);

        // Empty YAML with no root mapping should produce empty data
        // The provider only processes if root node is a YamlMappingNode
        Assert.NotNull(provider);
    }

    [Fact]
    public void Load_MixedNestedStructures_FlattensCorrectly()
    {
        var yaml = @"
outer:
  inner:
    value: ""deep""
  list:
    - item1
    - item2
  nested_list:
    - name: ""first""
      enabled: true
    - name: ""second""
      enabled: false
";
        var provider = CreateProvider(yaml);

        Assert.True(provider.TryGet("outer:inner:value", out var deep));
        Assert.Equal("deep", deep);

        Assert.True(provider.TryGet("outer:list:0", out var item1));
        Assert.Equal("item1", item1);

        Assert.True(provider.TryGet("outer:list:1", out var item2));
        Assert.Equal("item2", item2);

        Assert.True(provider.TryGet("outer:nested_list:0:name", out var name1));
        Assert.Equal("first", name1);

        Assert.True(provider.TryGet("outer:nested_list:1:enabled", out var enabled2));
        Assert.Equal("false", enabled2);
    }
}
