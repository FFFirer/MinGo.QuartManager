#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MinGo.Qap.Agent;
using MinGo.Qap.Agent.Configuration;
using MinGo.Qap.Agent.Services;
using MinGo.Qap.Shared.Models;
using Quartz;
using Quartz.Impl;
using System.Collections.Specialized;
using Xunit;

namespace MinGo.QuartzManager.Agent.Tests;

/// <summary>
/// Integration tests for Agent services using real RAMJobStore scheduler.
/// </summary>
public class AgentServiceIntegrationTests
{
    private ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        // Register AgentConfig via Options pattern for test
        services.Configure<AgentConfig>(cfg =>
        {
            cfg.Agent = new AgentSettings { ClusterId = "test-cluster", Port = 8080 };
            cfg.Platform = new PlatformSettings { Url = "http://localhost:9999", ApiToken = "test-token" };
            cfg.Quartz = new QuartzSettings();
        });
        services.AddLogging(b => b.AddConsole());
        services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());
        services.AddSingleton<IJobRegistry>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<JobRegistry>>();
            var manifest = new JobManifestDto
            {
                Jobs = new List<JobTypeInfoDto>
                {
                            new JobTypeInfoDto
                            {
                                Key = "HelloJob",
                                JobTypeFullName = typeof(HelloJob).FullName,
                                Description = "Test job",
                                Parameters = new List<ParameterInfoDto>
                                {
                                    new ParameterInfoDto { Name = "name", Type = "string", Required = true }
                                }
                            }
                }
            };
            return new JobRegistry(logger, manifest);
        });
        services.AddSingleton<IJobConverter, JobConverter>();
        services.AddSingleton<AgentUrlResolver>();
        services.AddSingleton<IAgentRegistrationService, DummyRegistrationService>();
        services.AddSingleton<ILogCollectionService, DummyLogCollectionService>();
        services.AddSingleton<IScheduler>(_ =>
        {
            var props = new NameValueCollection
            {
                ["quartz.scheduler.instanceName"] = "TestScheduler",
                ["quartz.scheduler.instanceId"] = "AUTO",
                ["quartz.jobStore.type"] = "Quartz.Simpl.RAMJobStore, Quartz",
                ["quartz.threadPool.threadCount"] = "1"
            };
            var factory = new StdSchedulerFactory(props);
            var scheduler = factory.GetScheduler().GetAwaiter().GetResult();
            scheduler.Start().GetAwaiter().GetResult();
            return scheduler;
        });
        services.AddSingleton<IQuartzService, QuartzService>();

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task GetManifest_Should_Return_JobManifest_With_Parameters()
    {
        using var sp = CreateServiceProvider();
        var registry = sp.GetRequiredService<IJobRegistry>();

        var manifest = registry.GetManifest();

        Assert.NotNull(manifest);
        Assert.NotEmpty(manifest.Jobs);

        var jobWithParams = manifest.Jobs.FirstOrDefault(j => j.Parameters?.Any() == true);
        Assert.NotNull(jobWithParams);
    }

    [Fact]
    public async Task GetSchedulerState_Should_Return_Scheduler_Info()
    {
        using var sp = CreateServiceProvider();
        var quartz = sp.GetRequiredService<IQuartzService>();

        var state = await quartz.GetSchedulerStateAsync();

        Assert.NotNull(state);
        Assert.False(string.IsNullOrEmpty(state.Name));
    }

    [Fact]
    public async Task GetJobs_Should_Return_Paginated_Job_List()
    {
        using var sp = CreateServiceProvider();
        var quartz = sp.GetRequiredService<IQuartzService>();

        var jobs = await quartz.GetJobsAsync(new JobQuery { Page = 1, PageSize = 10 });

        Assert.NotNull(jobs);
    }

    [Fact]
    public async Task CreateJob_Should_Return_JobDetail()
    {
        using var sp = CreateServiceProvider();
        var quartz = sp.GetRequiredService<IQuartzService>();

        var request = new CreateJobRequest
        {
            JobKey = "TestJob",
            JobType = "HelloJob",
            Schedule = new ScheduleDto
            {
                Type = "interval",
                IntervalSeconds = 60
            }
        };

        var job = await quartz.CreateJobAsync(request);

        Assert.NotNull(job);
        Assert.Equal("TestJob", job.JobKey);
    }

    [Fact]
    public async Task GetJobDetail_Should_Return_Null_For_NonExistent_Job()
    {
        using var sp = CreateServiceProvider();
        var quartz = sp.GetRequiredService<IQuartzService>();

        var job = await quartz.GetJobAsync("NonExistentJob");

        Assert.Null(job);
    }

    [Fact]
    public async Task TriggerJob_Should_Throw_For_NonExistent_Job()
    {
        using var sp = CreateServiceProvider();
        var quartz = sp.GetRequiredService<IQuartzService>();

        await Assert.ThrowsAsync<Quartz.JobPersistenceException>(() => quartz.TriggerJobAsync("NonExistentJob"));
    }

    [Fact]
    public async Task PauseJob_Should_NotThrow_For_NonExistent_Job()
    {
        using var sp = CreateServiceProvider();
        var quartz = sp.GetRequiredService<IQuartzService>();

        await quartz.PauseJobAsync("NonExistentJob");
    }

    [Fact]
    public async Task ResumeJob_Should_NotThrow_For_NonExistent_Job()
    {
        using var sp = CreateServiceProvider();
        var quartz = sp.GetRequiredService<IQuartzService>();

        await quartz.ResumeJobAsync("NonExistentJob");
    }

    [Fact]
    public async Task DeleteJob_Should_Throw_For_NonExistent_Job()
    {
        using var sp = CreateServiceProvider();
        var quartz = sp.GetRequiredService<IQuartzService>();

        await Assert.ThrowsAsync<ArgumentException>(() => quartz.DeleteJobAsync("NonExistentJob"));
    }

    [Fact]
    public async Task UpdateJob_Should_Throw_For_NonExistent_Job()
    {
        using var sp = CreateServiceProvider();
        var quartz = sp.GetRequiredService<IQuartzService>();

        await Assert.ThrowsAsync<ArgumentException>(() => quartz.UpdateJobAsync("NonExistentJob", new UpdateJobRequest()));
    }
}

// Dummy services for integration tests

public class DummyRegistrationService : IAgentRegistrationService
{
    public Task<AgentRegistrationResponse> RegisterAsync(CancellationToken ct = default)
        => Task.FromResult(new AgentRegistrationResponse { AgentId = "test", PlatformApiBaseUrl = "http://test" });

    public AgentRegistrationInfo? GetRegistrationInfo()
        => new AgentRegistrationInfo { AgentId = "test", PlatformApiBaseUrl = "http://test" };

    public Task<bool> DeregisterAsync(CancellationToken ct = default)
        => Task.FromResult(true);
}

public class DummyLogCollectionService : ILogCollectionService
{
    public void Start() { }
    public Task StopAsync() => Task.CompletedTask;
    public void RecordJobStarted(string jobKey) { }
    public void RecordJobCompleted(string jobKey, bool success, string? errorMessage = null, string? stackTrace = null, long? durationMs = null) { }
    public Task FlushPendingLogsAsync() => Task.CompletedTask;
}
