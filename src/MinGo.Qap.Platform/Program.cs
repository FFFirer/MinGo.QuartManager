using Microsoft.EntityFrameworkCore;
using MinGo.Qap.Platform.Caching;
using MinGo.Qap.Platform.Data;
using MinGo.Qap.Platform.NSwag;
using MinGo.Qap.Platform.Services;
using Serilog;
using System.Net.Security;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

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
builder.Services.AddHttpContextAccessor();

// 5. Manifest 缓存（Singleton，线程安全）
builder.Services.Configure<ManifestCacheOptions>(builder.Configuration.GetSection(ManifestCacheOptions.SectionName));
builder.Services.AddSingleton<IManifestCacheService, ManifestCacheService>();

var app = builder.Build();

// 6. 配置 HTTP 管道
// Swagger disabled temporarily (package unavailable in offline NuGet cache)
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

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
            logger.LogWarning("生产环境检测到有 {Count} 个待处理的数据库迁移: {Migrations}. 请手动运行 'dotnet ef database update' 应用迁移。",
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
