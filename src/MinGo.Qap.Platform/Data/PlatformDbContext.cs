using Microsoft.EntityFrameworkCore;
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
    /// Cluster 表
    /// </summary>
    public DbSet<Cluster> Clusters { get; set; } = null!;

    /// <summary>
    /// JobDefinition 表
    /// </summary>
    public DbSet<JobDefinition> JobDefinitions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cluster 配置
        modelBuilder.Entity<Cluster>(entity =>
        {
            entity.ToTable("Clusters");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(64);
            entity.Property(e => e.Name).HasMaxLength(256).IsRequired();
            entity.Property(e => e.Env).HasMaxLength(64).IsRequired();
            entity.Property(e => e.AgentUrl).HasMaxLength(512).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.TokenHash).HasMaxLength(256);
            entity.Property(e => e.Description).HasMaxLength(1024);
            
            // 索引
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Env);
            entity.HasIndex(e => e.DeletedAt);
            
            // 查询过滤软删除
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        // JobDefinition 配置
        modelBuilder.Entity<JobDefinition>(entity =>
        {
            entity.ToTable("JobDefinitions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(64);
            entity.Property(e => e.ClusterId).HasMaxLength(64).IsRequired();
            entity.Property(e => e.JobKey).HasMaxLength(512).IsRequired();
            entity.Property(e => e.JobType).HasMaxLength(256).IsRequired();
            entity.Property(e => e.Params).HasColumnType("text");
            entity.Property(e => e.Schedule).HasColumnType("text");
            entity.Property(e => e.Options).HasColumnType("text");
            entity.Property(e => e.ErrorMessage).HasMaxLength(4000);
            
            // 外键
            entity.HasOne(e => e.Cluster)
                .WithMany(c => c.JobDefinitions)
                .HasForeignKey(e => e.ClusterId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // 索引
            entity.HasIndex(e => new { e.ClusterId, e.JobKey }).IsUnique();
            entity.HasIndex(e => e.Status);
        });
    }
}
