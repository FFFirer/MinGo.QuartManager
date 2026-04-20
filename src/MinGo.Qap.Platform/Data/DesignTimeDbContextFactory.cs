using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MinGo.Qap.Platform.Data;

/// <summary>
/// 设计时 DbContext 工厂，支持 dotnet ef CLI 命令
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PlatformDbContext>
{
    public PlatformDbContext CreateDbContext(string[] args)
    {
        // 构建与运行时一致的配置
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: false)
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables()
            .Build();

        // 使用 GetConnectionString 与运行时保持一致
        var connectionString = configuration.GetConnectionString("PlatformDb");

        var optionsBuilder = new DbContextOptionsBuilder<PlatformDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new PlatformDbContext(optionsBuilder.Options);
    }
}
