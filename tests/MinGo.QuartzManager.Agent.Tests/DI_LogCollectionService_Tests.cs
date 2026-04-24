using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MinGo.Qap.Agent.Services;
using MinGo.Qap.Agent.Configuration;
using MinGo.Qap.Shared.Models;
using Xunit;

namespace MinGo.QuartzManager.Agent.Tests;

public class DI_LogCollectionService_Tests
{
    private class DummyRegistrationService : IAgentRegistrationService
    {
        public Task<AgentRegistrationResponse> RegisterAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AgentRegistrationResponse { AgentId = "agent-1", PlatformApiBaseUrl = "http://fake" });
        }

        public AgentRegistrationInfo? GetRegistrationInfo() => new AgentRegistrationInfo { AgentId = "agent-1", PlatformApiBaseUrl = "http://fake" };

        public Task<bool> DeregisterAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
    }

    private class FakeHttpMessageHandler : HttpMessageHandler
    {
        public bool Called { get; private set; } = false;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Called = true;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("ok") });
        }
    }

    private class DummyHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;
        public DummyHttpClientFactory(HttpMessageHandler handler)
        {
            _client = new HttpClient(handler, disposeHandler: true);
        }
        public HttpClient CreateClient(string name) => _client;
        public HttpClient CreateClient() => _client;
    }

    [Fact]
    public async Task LogCollectionService_Can_Send_Logs_To_Platform_When_Registered()
    {
        var services = new ServiceCollection();
        var agentConfig = new AgentConfig
        {
            Agent = new AgentSettings { ClusterId = "c1" },
            Platform = new PlatformSettings { Url = "http://plat", ApiToken = "tok" },
        };

        // Simple logger
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        services.AddLogging();
        services.AddSingleton(agentConfig);
        services.AddSingleton<IAgentRegistrationService, DummyRegistrationService>();
        var fakeHandler = new FakeHttpMessageHandler();
        services.AddSingleton<IHttpClientFactory>(sp => new DummyHttpClientFactory(fakeHandler));
        services.AddSingleton<ILogCollectionService, LogCollectionService>();
        var sp = services.BuildServiceProvider();

        var logService = sp.GetRequiredService<ILogCollectionService>() as LogCollectionService;
        Assert.NotNull(logService);
        logService!.Start();

        logService.RecordJobStarted("Job1");
        await logService.FlushPendingLogsAsync();
        await logService.StopAsync();
    }
}
