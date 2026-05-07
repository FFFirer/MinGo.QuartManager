using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MinGo.Qap.Platform.Data;

/// <summary>
/// UTC 时间审计拦截器
/// 确保所有 DateTimeOffset 字段写入时转换为 UTC
/// 自动填充 CreatedAt 和 UpdatedAt 字段
/// </summary>
public class UtcAuditInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyUtcConversion(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyUtcConversion(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ApplyUtcConversion(DbContext? context)
    {
        if (context == null)
            return;

        var entries = context.ChangeTracker
            .Entries()
            .Where(e => e.State == EntityState.Added ||
                        e.State == EntityState.Modified);

        var utcNow = DateTimeOffset.UtcNow;

        foreach (var entry in entries)
        {
            // 1. 查找所有 DateTimeOffset 属性，确保其值为 UTC
            foreach (var property in entry.Properties)
            {
                if (property.CurrentValue is DateTimeOffset dto)
                {
                    // 转换为 UTC（如果还不是 UTC）
                    if (dto.Offset != TimeSpan.Zero)
                    {
                        property.CurrentValue = dto.ToUniversalTime();
                    }
                }
            }

            // 2. 自动填充审计字段
            if (entry.State == EntityState.Added)
            {
                // CreatedAt：新增时自动填充
                var createdAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedAt");
                if (createdAtProperty?.CurrentValue == null ||
                    (createdAtProperty.CurrentValue is DateTimeOffset dto && dto == default) ||
                    (createdAtProperty.CurrentValue is DateTime dt && dt == default))
                {
                    if (createdAtProperty != null)
                        createdAtProperty.CurrentValue = utcNow;
                }
            }

            // UpdatedAt：新增或修改时自动更新
            var updatedAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
            if (updatedAtProperty != null)
            {
                updatedAtProperty.CurrentValue = utcNow;
            }
        }
    }
}
