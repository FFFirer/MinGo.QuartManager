using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MinGo.Qap.Platform.Data.Entities;

namespace MinGo.Qap.Platform.Data;

/// <summary>
/// Platform 数据库上下文
/// </summary>
public class PlatformDbContext : DbContext
{
    public PlatformDbContext(DbContextOptions<PlatformDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Agent 表
    /// </summary>
    public DbSet<Agent> Agents { get; set; } = null!;

    /// <summary>
    /// SchedulerInfo 表
    /// </summary>
    public DbSet<SchedulerInfo> SchedulerInfos { get; set; } = null!;

    /// <summary>
    /// Agent-Scheduler 关联表
    /// </summary>
    public DbSet<AgentScheduler> AgentSchedulers { get; set; } = null!;

    /// <summary>
    /// JobDefinition 表
    /// </summary>
    public DbSet<JobDefinition> JobDefinitions { get; set; } = null!;

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        base.ConfigureConventions(builder);

        // 1.3.1: 全局配置：所有 DateTimeOffset 属性映射到 timestamptz
        builder.Properties<DateTimeOffset>()
            .HaveColumnType("timestamptz");

        builder.Properties<DateTimeOffset?>()
            .HaveColumnType("timestamptz");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1.3.2: Value Converter：写入时强制转换为 UTC，读取时统一返回 UTC
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties()
                         .Where(p => p.ClrType == typeof(DateTimeOffset) ||
                                     p.ClrType == typeof(DateTimeOffset?)))
            {
                property.SetValueConverter(
                    new ValueConverter<DateTimeOffset, DateTimeOffset>(
                        v => v.ToUniversalTime(),    // 写入 → 强制 UTC
                        v => v.ToUniversalTime()));   // 读取 → 统一返回 UTC
            }
        }

        // Agent 配置
        modelBuilder.Entity<Agent>(entity =>
        {
            entity.ToTable("Agents");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(64);
            entity.Property(e => e.Name).HasMaxLength(256).IsRequired();
            entity.Property(e => e.Url).HasMaxLength(512).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(32).IsRequired();
            entity.Property(e => e.AgentVersion).HasMaxLength(64);
            entity.Property(e => e.TokenHash).HasMaxLength(256);

            // 索引
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.LastHeartbeat);

            // 查询过滤软删除
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        // SchedulerInfo 配置
        modelBuilder.Entity<SchedulerInfo>(entity =>
        {
            entity.ToTable("SchedulerInfos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(64);
            entity.Property(e => e.SchedulerName).HasMaxLength(256).IsRequired();
            entity.Property(e => e.SchedulerInstanceId).HasMaxLength(256);
            entity.Property(e => e.Status).HasMaxLength(32).IsRequired();
            entity.Property(e => e.JobStoreType).HasMaxLength(256);
            entity.Property(e => e.ThreadPoolType).HasMaxLength(256);
            entity.Property(e => e.Version).HasMaxLength(64);
            entity.Property(e => e.PropertiesJson).HasColumnType("text");

            // 联合唯一：SchedulerName + SchedulerInstanceId
            entity.HasIndex(e => new { e.SchedulerName, e.SchedulerInstanceId })
                  .IsUnique();

            // 索引
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.LastReportedAt);
        });

        // AgentScheduler 配置（多对多关联）
        modelBuilder.Entity<AgentScheduler>(entity =>
        {
            entity.ToTable("AgentSchedulers");
            entity.HasKey(e => new { e.AgentId, e.SchedulerInfoId });

            entity.Property(e => e.AgentId).HasMaxLength(64);
            entity.Property(e => e.SchedulerInfoId).HasMaxLength(64);

            // 外键配置
            entity.HasOne(e => e.Agent)
                .WithMany(a => a.AgentSchedulers)
                .HasForeignKey(e => e.AgentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SchedulerInfo)
                .WithMany(s => s.AgentSchedulers)
                .HasForeignKey(e => e.SchedulerInfoId)
                .OnDelete(DeleteBehavior.Cascade);

            // 索引
            entity.HasIndex(e => e.AgentId);
            entity.HasIndex(e => e.SchedulerInfoId);
            entity.HasIndex(e => e.ReportedAt);
        });

            // JobDefinition 配置
            modelBuilder.Entity<JobDefinition>(entity =>
            {
                entity.ToTable("JobDefinitions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(64);
                entity.Property(e => e.SchedulerName).HasMaxLength(64).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(256).IsRequired();
                entity.Property(e => e.Group).HasMaxLength(256).IsRequired();
                entity.Property(e => e.JobKey).HasMaxLength(512);
                entity.Property(e => e.JobType).HasMaxLength(256).IsRequired();
                entity.Property(e => e.Params).HasColumnType("text");
                entity.Property(e => e.Schedule).HasColumnType("text");
                entity.Property(e => e.Options).HasColumnType("text");
                entity.Property(e => e.ResultJson).HasColumnType("text");
                entity.Property(e => e.ErrorMessage).HasMaxLength(4000);

                // 索引
                entity.HasIndex(e => new { e.SchedulerName, e.Group, e.Name }).IsUnique();
                entity.HasIndex(e => new { e.SchedulerName, e.JobKey }).IsUnique();
                entity.HasIndex(e => e.Status);
            });


    }
}
