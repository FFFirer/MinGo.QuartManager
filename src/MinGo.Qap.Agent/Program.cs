using MinGo.Qap.Agent.Configuration;
using MinGo.Qap.Agent.Quartz;
using MinGo.Qap.Agent.Services;
using MinGo.Qap.Shared.Models;
using Quartz;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// 1. 加载配置
var configPath = builder.Configuration.GetValue<string>("ConfigPath") ?? "config.yaml";
var configLoader = new ConfigLoader(builder.Configuration);
var agentConfig = configLoader.Load(configPath);

// 将配置添加到 DI
builder.Configuration["agent:id"] = agentConfig.Agent.Id;
builder.Configuration["agent:clusterId"] = agentConfig.Agent.ClusterId;
builder.Configuration["agent:port"] = agentConfig.Agent.Port.ToString();
builder.Configuration["agent:heartbeatIntervalSeconds"] = agentConfig.Agent.HeartbeatIntervalSeconds.ToString();
builder.Configuration["platform:url"] = agentConfig.Platform.Url;

// 添加 Quartz 配置
foreach (var prop in agentConfig.Quartz.Properties)
{
    builder.Configuration[$"quartz:{prop.Key}"] = prop.Value;
}

// 2. 注册服务
// HTTP Client
builder.Services.AddHttpClient();

// Quartz Scheduler
builder.Services.AddSingleton<IScheduler>(sp =>
{
    var initializer = new SchedulerInitializer(
        sp.GetRequiredService<IConfiguration>().GetSection("quartz"),
        sp.GetRequiredService<ILogger<SchedulerInitializer>>()
    );
    return initializer.InitializeAsync().GetAwaiter().GetResult();
});

// Agent Services
builder.Services.AddSingleton<IJobRegistry, JobRegistry>();
builder.Services.AddSingleton<IJobConverter, JobConverter>();
builder.Services.AddScoped<IQuartzService, QuartzService>();
builder.Services.AddScoped<HealthCheckService>();

// Background Services
builder.Services.AddHostedService<HeartbeatService>();

// 注册 Job Manifest（从配置）
builder.Services.AddSingleton<JobManifestDto>(sp =>
{
    var manifest = new JobManifestDto
    {
        ClusterId = agentConfig.Agent.ClusterId,
        Jobs = new List<MinGo.Qap.Shared.Models.JobTypeInfoDto>()
    };

    foreach (var jobType in agentConfig.Quartz.JobTypes)
    {
        // 简化：Job 类型只记录名称，详细信息可以后续扩展
        manifest.Jobs.Add(new MinGo.Qap.Shared.Models.JobTypeInfoDto
        {
            Key = jobType.Split('.').Last(), // 使用类名作为 Key
            Description = jobType,
            Parameters = new List<MinGo.Qap.Shared.Models.ParameterInfoDto>()
        });
    }

    return manifest;
});

var app = builder.Build();

// 3. 配置 HTTP 管道
app.MapGet("/health", async (HealthCheckService healthCheck) =>
{
    var status = await healthCheck.CheckHealthAsync();
    return status.Healthy 
        ? Results.Ok(status) 
        : Results.StatusCode(503);
});

// Job Manifest 端点
app.MapGet("/api/jobs/manifest", (IJobRegistry jobRegistry) =>
{
    var manifest = jobRegistry.GetManifest();
    return Results.Ok(manifest);
});

// Jobs 端点
app.MapGet("/api/jobs", async (IQuartzService quartzService, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? group = null, [FromQuery] string? status = null, [FromQuery] string? keyword = null) =>
{
    var query = new JobQuery
    {
        Page = page,
        PageSize = pageSize,
        Group = group,
        Status = status,
        Keyword = keyword
    };
    var jobs = await quartzService.GetJobsAsync(query);
    return Results.Ok(jobs);
});

app.MapGet("/api/jobs/{jobKey}", async (IQuartzService quartzService, string jobKey) =>
{
    var job = await quartzService.GetJobAsync(jobKey);
    if (job == null) return Results.NotFound();
    return Results.Ok(job);
});

app.MapPost("/api/jobs", async (IQuartzService quartzService, CreateJobRequest request) =>
{
    try
    {
        var job = await quartzService.CreateJobAsync(request);
        return Results.Created($"/api/jobs/{job.JobKey}", job);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPut("/api/jobs/{jobKey}", async (IQuartzService quartzService, string jobKey, UpdateJobRequest request) =>
{
    try
    {
        await quartzService.UpdateJobAsync(jobKey, request);
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapDelete("/api/jobs/{jobKey}", async (IQuartzService quartzService, string jobKey) =>
{
    try
    {
        await quartzService.DeleteJobAsync(jobKey);
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/jobs/{jobKey}/trigger", async (IQuartzService quartzService, string jobKey) =>
{
    try
    {
        await quartzService.TriggerJobAsync(jobKey);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/jobs/{jobKey}/pause", async (IQuartzService quartzService, string jobKey) =>
{
    try
    {
        await quartzService.PauseJobAsync(jobKey);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/jobs/{jobKey}/resume", async (IQuartzService quartzService, string jobKey) =>
{
    try
    {
        await quartzService.ResumeJobAsync(jobKey);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 注册 Job Manifest 到 Registry
app.Lifetime.ApplicationStarted.Register(() =>
{
    using var scope = app.Services.CreateScope();
    var registry = scope.ServiceProvider.GetRequiredService<IJobRegistry>();
    var manifest = scope.ServiceProvider.GetRequiredService<JobManifestDto>();
    registry.Register(manifest);
});

// 4. 优雅关闭
app.Lifetime.ApplicationStopping.Register(() =>
{
    using var scope = app.Services.CreateScope();
    var scheduler = scope.ServiceProvider.GetRequiredService<IScheduler>();
    
    if (!scheduler.IsShutdown)
    {
        scheduler.Shutdown(waitForJobsToComplete: true).Wait();
    }
});

app.Run();
