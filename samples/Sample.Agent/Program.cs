using MinGo.Qap.Agent;
using MinGo.Qap.Agent.OpenApi;
using MinGo.Sample.Agent.Jobs;
using Quartz;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add Minimal API support
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
builder.Services.AddMinGoAgentOpenApi();

// Add health checks
builder.Services.AddHealthChecks();

// Configure Quartz with RAMJobStore using official Microsoft DI integration
builder.Services.AddQuartz(q =>
{
    q.SchedulerName = "SampleAgentScheduler";
    q.SchedulerId = "AUTO";
    q.UseDefaultThreadPool(tp => tp.MaxConcurrency = 5);
    q.UseInMemoryStore();

    // Schedule HelloJob to run every 10 seconds
    // q.ScheduleJob<HelloJob>(trigger => trigger
    //     .WithIdentity("HelloJob-trigger", "sample")
    //     .StartNow()
    //     .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever()));

    // Schedule ScheduledJob to run every 60 seconds (health check)
    // q.ScheduleJob<ScheduledJob>(trigger => trigger
    //     .WithIdentity("ScheduledJob-trigger", "sample")
    //     .StartNow()
    //     .WithSimpleSchedule(x => x.WithIntervalInSeconds(60).RepeatForever()));

    // Register ManualTriggerJob as durable job (no trigger, triggered via API)
    q.AddJob<ManualTriggerJob>(j => j
        .WithIdentity("ManualTriggerJob", "sample")
        .StoreDurably());
});

// Add Quartz hosted service for scheduler lifecycle management
builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

// Add MinGo Agent services (includes LogCollection, JobDiscovery, Registration)
// This uses the standard IConfiguration pipeline with config.yaml as YAML source.
builder.AddMinGoAgent();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    // app.UseSwagger();
    // app.UseSwaggerUI();
    app.UseOpenApi();
    app.UseSwaggerUi();
}

// Map MinGo Agent HTTP API (replaces custom Controllers)
app.MapMinGoAgentApi();

// Map health check endpoint
app.MapHealthChecks("/health");

Log.Information("Quartz Scheduler configured with RAMJobStore via AddQuartz");

app.Run();