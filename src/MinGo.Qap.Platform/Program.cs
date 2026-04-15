using Microsoft.EntityFrameworkCore;
using MinGo.Qap.Platform.BackgroundServices;
using MinGo.Qap.Platform.Data;
using MinGo.Qap.Platform.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. 添加 Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. 添加数据库
var connectionString = builder.Configuration.GetConnectionString("PlatformDb") 
    ?? "Server=localhost;Database=MinGoQap;Trusted_Connection=True;TrustServerCertificate=True";

builder.Services.AddDbContext<PlatformDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. 添加 HTTP Client
builder.Services.AddHttpClient();

// 4. 添加服务
builder.Services.AddScoped<IClusterService, ClusterService>();
builder.Services.AddScoped<IAgentProxyService, AgentProxyService>();
builder.Services.AddScoped<IJobService, JobService>();

// 5. 添加状态检查后台服务（可选）
builder.Services.AddHostedService<ClusterStatusMonitorService>();

var app = builder.Build();

// 6. 配置 HTTP 管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// 7. 自动迁移（开发环境）
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
    db.Database.Migrate();
}

app.Run();
