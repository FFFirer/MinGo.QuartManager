using MinGo.Qap.Agent;
using MinGo.Sample.Agent.Jobs;
using Quartz;
using Quartz.Impl;
using Quartz.Simpl;
using Quartz.Spi;
using Serilog;
using System.Collections.Specialized;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add Minimal API support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add health checks
builder.Services.AddHealthChecks();

// Configure Quartz with RAMJobStore using simple approach
var properties = new NameValueCollection
{
    ["quartz.scheduler.instanceName"] = "SampleAgentScheduler",
    ["quartz.scheduler.instanceId"] = "AUTO",
    ["quartz.jobStore.type"] = typeof(RAMJobStore).FullName,
    ["quartz.threadPool.threadCount"] = "5"
};

// Add MinGo Agent services (includes LogCollection, JobDiscovery, Registration)
builder.Services.AddMinGoAgent(builder.Configuration);

// Register sample jobs for DI resolution
builder.Services.AddTransient<HelloJob>();
builder.Services.AddTransient<ScheduledJob>();
builder.Services.AddTransient<ManualTriggerJob>();

builder.Services.AddSingleton<IScheduler>(sp =>
{
    var factory = new StdSchedulerFactory(properties);
    var scheduler = factory.GetScheduler().GetAwaiter().GetResult();
    scheduler.JobFactory = new DIJobFactory(sp);
    scheduler.Start();
    Log.Information("Quartz Scheduler started with RAMJobStore");
    return scheduler;
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map MinGo Agent HTTP API (replaces custom Controllers)
app.MapMinGoAgentApi();

// Map health check endpoint
app.MapHealthChecks("/health");

// Get scheduler and schedule sample jobs
var scheduler = app.Services.GetRequiredService<IScheduler>();

// Schedule HelloJob to run every 10 seconds
var helloJob = JobBuilder.Create<HelloJob>()
    .WithIdentity("HelloJob", "sample")
    .Build();

var helloTrigger = TriggerBuilder.Create()
    .WithIdentity("HelloJob-trigger", "sample")
    .StartNow()
    .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever())
    .Build();

await scheduler.ScheduleJob(helloJob, helloTrigger);

// Schedule ScheduledJob to run every 60 seconds (health check)
var scheduledJob = JobBuilder.Create<ScheduledJob>()
    .WithIdentity("ScheduledJob", "sample")
    .Build();

var scheduledTrigger = TriggerBuilder.Create()
    .WithIdentity("ScheduledJob-trigger", "sample")
    .StartNow()
    .WithSimpleSchedule(x => x.WithIntervalInSeconds(60).RepeatForever())
    .Build();

await scheduler.ScheduleJob(scheduledJob, scheduledTrigger);

// Register ManualTriggerJob as durable job
var manualJob = JobBuilder.Create<ManualTriggerJob>()
    .WithIdentity("ManualTriggerJob", "sample")
    .StoreDurably()
    .Build();

await scheduler.AddJob(manualJob, replace: true);

Log.Information("Sample jobs registered successfully");

app.Run();

/// <summary>
/// Simple DI-aware job factory that resolves jobs from the service provider.
/// </summary>
public class DIJobFactory : IJobFactory
{
    private readonly IServiceProvider _serviceProvider;
    public DIJobFactory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        return (IJob)_serviceProvider.GetRequiredService(bundle.JobDetail.JobType);
    }

    public void ReturnJob(IJob job) { }
}