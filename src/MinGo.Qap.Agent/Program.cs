using MinGo.Qap.Agent.Configuration;
using MinGo.Qap.Agent.Quartz;
using MinGo.Qap.Agent.Services;
using MinGo.Qap.Shared.Models;
using Quartz;

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
