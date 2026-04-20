using Microsoft.EntityFrameworkCore;
using MinGo.Qap.Platform.BackgroundServices;
using MinGo.Qap.Platform.Data;
using MinGo.Qap.Platform.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// 1. 添加 Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. 添加数据库
var connectionString = builder.Configuration.GetConnectionString("PlatformDb") 
    ?? "Host=localhost;Database=MinGoQap;Username=postgres;Password=postgres";

builder.Services.AddDbContext<PlatformDbContext>(options =>
    options.UseNpgsql(connectionString));

// 3. 添加 HTTP Client
builder.Services.AddHttpClient();

// 4. 添加服务
builder.Services.AddScoped<IClusterService, ClusterService>();
builder.Services.AddScoped<IAgentProxyService, AgentProxyService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IAgentInstanceService, AgentInstanceService>();
builder.Services.AddSingleton<IAgentSelectionStrategy, RandomSelectionStrategy>();
builder.Services.AddHttpContextAccessor();

// 5. 添加状态检查后台服务（可选）
builder.Services.AddHostedService<ClusterStatusMonitorService>();

var app = builder.Build();

// 6. 配置 HTTP 管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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
