using MinGo.Qap.Agent;
using MinGo.Sample.Agent.Jobs;
using Quartz;
using Quartz.Impl;
using Quartz.Simpl;
using Serilog;
using System.Collections.Specialized;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add controllers
builder.Services.AddControllers();
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

builder.Services.AddMinGoAgent(builder.Configuration);
builder.Services.AddSingleton<IScheduler>(sp =>
{
    var factory = new StdSchedulerFactory(properties);
    var scheduler = factory.GetScheduler().GetAwaiter().GetResult();
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

app.UseAuthorization();
app.MapControllers();

// Map health check endpoint
app.MapHealthChecks("/health");

// Get scheduler and schedule HelloJob manually
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