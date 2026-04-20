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
builder.Configuration["agent:registrationMaxAttempts"] = agentConfig.Agent.RegistrationMaxAttempts.ToString();
builder.Configuration["agent:registrationRetryDelaySeconds"] = agentConfig.Agent.RegistrationRetryDelaySeconds.ToString();
builder.Configuration["agent:clusterMode"] = agentConfig.Agent.ClusterMode.ToString();
builder.Configuration["platform:url"] = agentConfig.Platform.Url;
builder.Configuration["platform:apiToken"] = agentConfig.Platform.ApiToken;

// 处理集群模式配置
var quartzProperties = new Dictionary<string, string>(agentConfig.Quartz.Properties);

// 如果启用了集群模式，确保 Quartz 配置支持集群
if (agentConfig.Agent.ClusterMode)
{
    // 生成 Quartz 实例 ID（集群中必须唯一）
    // 格式: {clusterId}-{hostname}-{timestamp}
    if (!quartzProperties.ContainsKey("quartz.scheduler.instanceId") || 
        quartzProperties["quartz.scheduler.instanceId"] == "AUTO")
    {
        var hostname = Environment.MachineName.ToLowerInvariant();
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var clusterId = agentConfig.Agent.ClusterId.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-")
            .Replace(".", "-");
        var instanceId = $"{clusterId}-{hostname}-{timestamp}";
        quartzProperties["quartz.scheduler.instanceId"] = instanceId;
    }
    
    // 确保 jobStore.type 是 AdoJobStore（如果不是，则设置为默认值）
    if (!quartzProperties.ContainsKey("quartz.jobStore.type") || 
        !quartzProperties["quartz.jobStore.type"].Contains("AdoJobStore"))
    {
        quartzProperties["quartz.jobStore.type"] = "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz";
        quartzProperties["quartz.jobStore.driverDelegateType"] = "Quartz.Impl.AdoJobStore.StdAdoDelegate, Quartz";
        quartzProperties["quartz.jobStore.tablePrefix"] = "QRTZ_";
        quartzProperties["quartz.jobStore.useProperties"] = "false";
        quartzProperties["quartz.jobStore.misfireThreshold"] = "60000";
    }
    
    // 启用集群
    quartzProperties["quartz.jobStore.clustered"] = "true";
    
    // 设置集群检查间隔（如果未设置）
    if (!quartzProperties.ContainsKey("quartz.jobStore.clusterCheckinInterval"))
    {
        quartzProperties["quartz.jobStore.clusterCheckinInterval"] = "20000";
    }
    
    // 如果未设置数据源，设置默认数据源配置（用户仍需提供连接字符串）
    if (!quartzProperties.ContainsKey("quartz.dataSource.default.provider"))
    {
        quartzProperties["quartz.dataSource.default.provider"] = "Npgsql";
    }
    
    if (!quartzProperties.ContainsKey("quartz.dataSource.default.connectionString"))
    {
        // 连接字符串需要由用户提供，这里只设置一个示例占位符
        // 实际值应该来自环境变量或配置文件
        quartzProperties["quartz.dataSource.default.connectionString"] = "Host=postgres;Port=5432;Database=quartz;Username=postgres;Password=${POSTGRES_PASSWORD}";
    }
}

// 添加 Quartz 配置到 builder.Configuration
foreach (var prop in quartzProperties)
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
builder.Services.AddSingleton<IAgentRegistrationService, AgentRegistrationService>();

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
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    // 注销 Agent 实例
    try
    {
        var registrationService = scope.ServiceProvider.GetRequiredService<IAgentRegistrationService>();
        var success = registrationService.DeregisterAsync().GetAwaiter().GetResult();
        if (success)
        {
            logger.LogInformation("Agent deregistered successfully");
        }
        else
        {
            logger.LogWarning("Agent deregistration failed or not registered");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during agent deregistration");
    }
    
    // 关闭 Quartz 调度器
    var scheduler = scope.ServiceProvider.GetRequiredService<IScheduler>();
    if (!scheduler.IsShutdown)
    {
        scheduler.Shutdown(waitForJobsToComplete: true).Wait();
    }
});

// 应用程序启动时注册 Agent
app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        using var scope = app.Services.CreateScope();
        var registrationService = scope.ServiceProvider.GetRequiredService<IAgentRegistrationService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Registering agent with platform...");
        var registrationResponse = await registrationService.RegisterAsync();
        logger.LogInformation("Agent registered successfully. AgentId: {AgentId}", registrationResponse.AgentId);
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Failed to register agent with platform. Heartbeat service will not start.");
        // 注意：如果注册失败，心跳服务将无法工作
    }
});

app.Run();
