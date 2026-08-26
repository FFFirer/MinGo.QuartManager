using Microsoft.EntityFrameworkCore;
using MinGo.Qap.Platform.Caching;
using MinGo.Qap.Platform.Data;
using MinGo.Qap.Platform.Data.Entities;
using MinGo.Qap.Platform.NSwag;
using MinGo.Qap.Platform.Services;
using MinGo.Qap.Shared;
using MinGo.Qap.Shared.Enums;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System.Net.Security;

var builder = WebApplication.CreateBuilder(args);

// =============================================================================
// 1. Serilog 日志
// =============================================================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{MachineName}/{EnvironmentName}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// =============================================================================
// 2. OpenTelemetry (Traces + Metrics + Logs)
// =============================================================================
var otelSection = builder.Configuration.GetSection("OpenTelemetry");
var serviceName = otelSection["ServiceName"] ?? "MinGo.Qap.Platform";
var serviceVersion = otelSection["ServiceVersion"] ?? "1.0.0";
var otlpEndpoint = otelSection["OtlpEndpoint"];

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r
        .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
        .AddAttributes(new Dictionary<string, object>
        {
            ["deployment.environment"] = builder.Environment.EnvironmentName
        }))
    .WithTracing(tracing =>
    {
        tracing
            .AddSource(QapTelemetry.SourceName)
            .AddAspNetCoreInstrumentation(options =>
            {
                // 过滤健康检查等噪音请求
                options.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/health");
            })
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation(options =>
            {
                options.SetDbStatementForText = true;
            });

        if (!string.IsNullOrEmpty(otlpEndpoint))
            tracing.AddOtlpExporter(opts =>
            {
                opts.Endpoint = new Uri(otlpEndpoint);
                opts.Protocol = OtlpExportProtocol.Grpc;
            });
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddMeter(QapTelemetry.MeterName)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation();

        if (!string.IsNullOrEmpty(otlpEndpoint))
            metrics.AddOtlpExporter(opts =>
            {
                opts.Endpoint = new Uri(otlpEndpoint);
                opts.Protocol = OtlpExportProtocol.Grpc;
            });
    });

// OTel Logs: 通过 ILogger 管道导出 (与 Serilog 并行)
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;

    if (!string.IsNullOrEmpty(otlpEndpoint))
        logging.AddOtlpExporter(opts =>
        {
            opts.Endpoint = new Uri(otlpEndpoint);
            opts.Protocol = OtlpExportProtocol.Grpc;
        });
});

// 1. 添加 Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen(); // Add Swashbuckle package to enable
builder.Services.AddOpenApiDocument(config =>
{
    config.OperationProcessors.Add(new SwaggerHeaderProcessor());
});

// 2. 添加数据库
var connectionString = builder.Configuration.GetConnectionString("PlatformDb") 
    ?? "Host=localhost;Database=MinGoQap;Username=postgres;Password=postgres";

builder.Services.AddDbContext<PlatformDbContext>(options =>
    options.UseNpgsql(connectionString)
           .AddInterceptors(new UtcAuditInterceptor()));

// 3. 添加命名 HTTP Client（用于转发到 Agent）
builder.Services.AddHttpClient("AgentApi", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var skipSsl = config.GetValue<bool>("AgentProxy:SkipSslVerify");

    var handler = new SocketsHttpHandler();
    if (skipSsl)
    {
        handler.SslOptions = new SslClientAuthenticationOptions
        {
            RemoteCertificateValidationCallback = (_, _, _, _) => true
        };
    }
    return handler;
});

// 4. 添加服务
builder.Services.AddScoped<AgentService>();
builder.Services.AddScoped<SchedulerService>();
builder.Services.AddScoped<SchedulerRouterService>();
builder.Services.AddScoped<IAgentProxyService, AgentProxyService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IExecutionLogService, ExecutionLogService>();
builder.Services.AddHttpContextAccessor();

// 5. Manifest 缓存（Singleton，线程安全）
builder.Services.Configure<ManifestCacheOptions>(builder.Configuration.GetSection(ManifestCacheOptions.SectionName));
builder.Services.AddSingleton<IManifestCacheService, ManifestCacheService>();

var app = builder.Build();

// 6.1. 注册 OTel Observable Gauges（需要 DI 容器查询 DB）
var manifestCache = app.Services.GetRequiredService<IManifestCacheService>();

QapTelemetry.Meter.CreateObservableGauge<int>("qap.agents.online", () =>
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
    return db.Agents.Count(a => a.DeletedAt == null && a.Status == "Online");
});

QapTelemetry.Meter.CreateObservableGauge<int>("qap.agents.warning", () =>
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
    return db.Agents.Count(a => a.DeletedAt == null && a.Status == "Warning");
});

QapTelemetry.Meter.CreateObservableGauge<int>("qap.agents.offline", () =>
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
    return db.Agents.Count(a => a.DeletedAt == null && a.Status == "Offline");
});

QapTelemetry.Meter.CreateObservableGauge<int>("qap.cache.entries", () =>
{
    return manifestCache.Count;
});

QapTelemetry.Meter.CreateObservableGauge<int>("qap.jobs.pending_sync", () =>
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
    return db.JobDefinitions.Count(j => j.Status == SyncStatus.Pending);
});

QapTelemetry.Meter.CreateObservableGauge<int>("qap.jobs.failed_sync", () =>
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
    return db.JobDefinitions.Count(j => j.Status == SyncStatus.Failed);
});

// 6. 配置 HTTP 管道
// Swagger disabled temporarily (package unavailable in offline NuGet cache)
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

// app.UseHttpsRedirection();

// 7. 静态文件托管 (wwwroot — 包含生产构建的 UI)
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();
app.MapControllers();

// 8. SPA 回退路由：非 API 路径返回 index.html，支持 React Router 前端路由
app.MapFallbackToFile("index.html");

// 7. 数据库迁移
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    if (app.Environment.IsDevelopment())
    {
        // 开发环境：自动应用迁移
        logger.LogInformation("Development environment detected. Applying pending migrations...");
        db.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully.");
    }
    else
    {
        // 生产环境：检查是否有待处理迁移并记录警告
        var pendingMigrations = db.Database.GetPendingMigrations().ToList();
        if (pendingMigrations.Any())
        {
            logger.LogWarning("生产环境检测到有 {Count} 个待处理的数据库迁移: {Migrations}. " +
                "请使用镜像内 efbundle 执行迁移：\n" +
                "  docker run --rm -e ConnectionStrings__PlatformDb=\"<连接字符串>\" <镜像名> dotnet /app/efbundle.dll",
                pendingMigrations.Count,
                string.Join(", ", pendingMigrations));
        }
        else
        {
            logger.LogInformation("数据库已是最新状态，没有待处理的迁移。");
        }
    }
}

app.Run();
