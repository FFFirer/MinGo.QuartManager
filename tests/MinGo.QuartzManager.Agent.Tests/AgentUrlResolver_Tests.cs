#nullable enable
using System;
using Microsoft.Extensions.Logging;
using MinGo.Qap.Agent.Configuration;
using MinGo.Qap.Agent.Services;
using Xunit;

namespace MinGo.QuartzManager.Agent.Tests;

public class AgentUrlResolver_Tests : IDisposable
{
    private readonly AgentUrlResolver _resolver;
    private readonly ILogger<AgentUrlResolver> _logger;

    // Store original env var values to restore after tests
    private readonly string? _originalAgentUrl;
    private readonly string? _originalPodIp;
    private readonly string? _originalK8sHost;
    private readonly string? _originalHostname;

    public AgentUrlResolver_Tests()
    {
        _logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<AgentUrlResolver>();
        _resolver = new AgentUrlResolver(_logger);

        _originalAgentUrl = Environment.GetEnvironmentVariable("AGENT_URL");
        _originalPodIp = Environment.GetEnvironmentVariable("POD_IP");
        _originalK8sHost = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST");
        _originalHostname = Environment.GetEnvironmentVariable("HOSTNAME");
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("AGENT_URL", _originalAgentUrl);
        Environment.SetEnvironmentVariable("POD_IP", _originalPodIp);
        Environment.SetEnvironmentVariable("KUBERNETES_SERVICE_HOST", _originalK8sHost);
        Environment.SetEnvironmentVariable("HOSTNAME", _originalHostname);
    }

    private static AgentSettings CreateSettings(string externalUrl = "", int port = 8080, string networkInterface = "")
    {
        return new AgentSettings
        {
            ExternalUrl = externalUrl,
            Port = port,
            NetworkInterface = networkInterface
        };
    }

    [Fact]
    public void Resolve_Should_Use_ExternalUrl_When_Configured()
    {
        var settings = CreateSettings(externalUrl: "http://explicit:8080", port: 5000);

        var url = _resolver.Resolve(settings);

        Assert.Equal("http://explicit:8080", url);
    }

    [Fact]
    public void Resolve_Should_Use_AGENT_URL_Environment_Variable()
    {
        Environment.SetEnvironmentVariable("AGENT_URL", "http://env-var:9000");
        var settings = CreateSettings(port: 5000);

        var url = _resolver.Resolve(settings);

        Assert.Equal("http://env-var:9000", url);
    }

    [Fact]
    public void Resolve_Should_Use_POD_IP_For_Kubernetes()
    {
        Environment.SetEnvironmentVariable("POD_IP", "10.0.0.5");
        var settings = CreateSettings(port: 8080);

        var url = _resolver.Resolve(settings);

        Assert.Equal("http://10.0.0.5:8080", url);
    }

    [Fact]
    public void Resolve_Should_Use_K8S_HOSTNAME_When_Kubernetes_Service_Host_Exists()
    {
        Environment.SetEnvironmentVariable("KUBERNETES_SERVICE_HOST", "10.96.0.1");
        Environment.SetEnvironmentVariable("HOSTNAME", "my-pod-abc123");
        var settings = CreateSettings(port: 8080);

        var url = _resolver.Resolve(settings);

        Assert.Equal("http://my-pod-abc123:8080", url);
    }

    [Fact]
    public void Resolve_POD_IP_Should_Take_Priority_Over_K8S_Service_Host()
    {
        Environment.SetEnvironmentVariable("POD_IP", "10.0.0.5");
        Environment.SetEnvironmentVariable("KUBERNETES_SERVICE_HOST", "10.96.0.1");
        Environment.SetEnvironmentVariable("HOSTNAME", "my-pod-abc123");
        var settings = CreateSettings(port: 8080);

        var url = _resolver.Resolve(settings);

        Assert.Equal("http://10.0.0.5:8080", url);
    }

    [Fact]
    public void Resolve_ExternalUrl_Should_Take_Priority_Over_Environment_Variable()
    {
        Environment.SetEnvironmentVariable("AGENT_URL", "http://env:9000");
        var settings = CreateSettings(externalUrl: "http://explicit:8080", port: 5000);

        var url = _resolver.Resolve(settings);

        Assert.Equal("http://explicit:8080", url);
    }

    [Fact]
    public void Resolve_Environment_Variable_Should_Take_Priority_Over_Kubernetes()
    {
        Environment.SetEnvironmentVariable("AGENT_URL", "http://env:9000");
        Environment.SetEnvironmentVariable("POD_IP", "10.0.0.5");
        var settings = CreateSettings(port: 8080);

        var url = _resolver.Resolve(settings);

        Assert.Equal("http://env:9000", url);
    }

    [Fact]
    public void Resolve_Should_Fallback_To_Local_MachineName_When_No_Other_Source()
    {
        Environment.SetEnvironmentVariable("AGENT_URL", null);
        Environment.SetEnvironmentVariable("POD_IP", null);
        Environment.SetEnvironmentVariable("KUBERNETES_SERVICE_HOST", null);

        var settings = CreateSettings(port: 5000);

        var url = _resolver.Resolve(settings);

        Assert.StartsWith("http://", url);
        Assert.Contains(":5000", url);
    }

    [Fact]
    public void Resolve_Should_Return_Local_IP_When_NetworkInterface_Specified_But_Invalid()
    {
        Environment.SetEnvironmentVariable("AGENT_URL", null);
        Environment.SetEnvironmentVariable("POD_IP", null);
        Environment.SetEnvironmentVariable("KUBERNETES_SERVICE_HOST", null);

        var settings = CreateSettings(port: 5000, networkInterface: "nonexistent-eth");

        var url = _resolver.Resolve(settings);

        // Should still return a valid http URL (fallback to local IP or machine name)
        Assert.StartsWith("http://", url);
        Assert.Contains(":5000", url);
    }
}
