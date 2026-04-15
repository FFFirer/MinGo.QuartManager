using Microsoft.EntityFrameworkCore;
using MinGo.Qap.Platform.Data;
using MinGo.Qap.Platform.Data.Entities;
using MinGo.Qap.Shared.Enums;
using MinGo.Qap.Shared.Models;
using System.Security.Cryptography;
using System.Text;

namespace MinGo.Qap.Platform.Services;

/// <summary>
/// Cluster 服务接口
/// </summary>
public interface IClusterService
{
    Task<ClusterDto> CreateAsync(CreateClusterRequest request);
    Task<ClusterDto?> GetAsync(string clusterId);
    Task<List<ClusterSummaryDto>> GetAllAsync(string? env = null, string? status = null);
    Task UpdateHeartbeatAsync(string clusterId, HeartbeatDto heartbeat);
    Task DeleteAsync(string clusterId);
    Task<string> RotateTokenAsync(string clusterId);
}

/// <summary>
/// Cluster 服务实现
/// </summary>
public class ClusterService : IClusterService
{
    private readonly PlatformDbContext _dbContext;
    private readonly ILogger<ClusterService> _logger;

    public ClusterService(PlatformDbContext dbContext, ILogger<ClusterService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ClusterDto> CreateAsync(CreateClusterRequest request)
    {
        var clusterId = $"cls-{Guid.NewGuid().ToString()[..8]}";
        var token = GenerateToken();
        var tokenHash = HashToken(token);

        var cluster = new Cluster
        {
            Id = clusterId,
            Name = request.Name,
            Env = request.Env,
            AgentUrl = request.AgentUrl,
            Status = ClusterStatus.Pending,
            TokenHash = tokenHash,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Clusters.Add(cluster);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Cluster created: {ClusterId}", clusterId);

        // 返回包含明文的 Token（只显示一次）
        return new ClusterDto
        {
            Id = cluster.Id,
            Name = cluster.Name,
            Env = cluster.Env,
            AgentUrl = cluster.AgentUrl,
            Status = cluster.Status.ToString(),
            CreatedAt = cluster.CreatedAt,
            // Token 实际应该在响应中单独返回，这里简化
        };
    }

    public async Task<ClusterDto?> GetAsync(string clusterId)
    {
        var cluster = await _dbContext.Clusters
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clusterId);

        if (cluster == null) return null;

        return MapToDto(cluster);
    }

    public async Task<List<ClusterSummaryDto>> GetAllAsync(string? env = null, string? status = null)
    {
        var query = _dbContext.Clusters.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(env))
        {
            query = query.Where(c => c.Env == env);
        }

        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<ClusterStatus>(status, true, out var statusEnum))
            {
                query = query.Where(c => c.Status == statusEnum);
            }
        }

        var clusters = await query.ToListAsync();

        // 统计每个 Cluster 的 Job 数量
        var clusterIds = clusters.Select(c => c.Id).ToList();
        var jobCounts = await _dbContext.JobDefinitions
            .Where(j => clusterIds.Contains(j.ClusterId))
            .GroupBy(j => j.ClusterId)
            .Select(g => new { ClusterId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClusterId, x => x.Count);

        return clusters.Select(c => new ClusterSummaryDto
        {
            Id = c.Id,
            Name = c.Name,
            Env = c.Env,
            Status = c.Status.ToString(),
            LastHeartbeat = c.LastHeartbeat,
            JobCount = jobCounts.GetValueOrDefault(c.Id, 0)
        }).ToList();
    }

    public async Task UpdateHeartbeatAsync(string clusterId, HeartbeatDto heartbeat)
    {
        var cluster = await _dbContext.Clusters.FindAsync(clusterId);
        if (cluster == null)
        {
            _logger.LogWarning("Heartbeat received for unknown cluster: {ClusterId}", clusterId);
            return;
        }

        // 更新心跳时间和状态
        cluster.LastHeartbeat = DateTime.UtcNow;
        
        // 如果之前是 Pending/Offline，更新为 Online
        if (cluster.Status == ClusterStatus.Pending || cluster.Status == ClusterStatus.Offline)
        {
            cluster.Status = ClusterStatus.Online;
            _logger.LogInformation("Cluster {ClusterId} is now Online", clusterId);
        }

        cluster.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(string clusterId)
    {
        var cluster = await _dbContext.Clusters.FindAsync(clusterId);
        if (cluster == null)
        {
            throw new ArgumentException($"Cluster not found: {clusterId}");
        }

        // 软删除
        cluster.DeletedAt = DateTime.UtcNow;
        cluster.Status = ClusterStatus.Deleted;
        cluster.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Cluster deleted: {ClusterId}", clusterId);
    }

    public async Task<string> RotateTokenAsync(string clusterId)
    {
        var cluster = await _dbContext.Clusters.FindAsync(clusterId);
        if (cluster == null)
        {
            throw new ArgumentException($"Cluster not found: {clusterId}");
        }

        var newToken = GenerateToken();
        cluster.TokenHash = HashToken(newToken);
        cluster.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Token rotated for cluster: {ClusterId}", clusterId);

        return newToken;
    }

    /// <summary>
    /// 计算 Cluster 状态（基于心跳超时）
    /// </summary>
    public async Task UpdateClusterStatusesAsync()
    {
        var warningThreshold = TimeSpan.FromSeconds(60);
        var offlineThreshold = TimeSpan.FromSeconds(90);
        var now = DateTime.UtcNow;

        var clusters = await _dbContext.Clusters
            .Where(c => c.Status != ClusterStatus.Deleted && c.Status != ClusterStatus.Pending)
            .ToListAsync();

        foreach (var cluster in clusters)
        {
            if (!cluster.LastHeartbeat.HasValue) continue;

            var elapsed = now - cluster.LastHeartbeat.Value;
            var newStatus = cluster.Status;

            if (elapsed > offlineThreshold)
            {
                newStatus = ClusterStatus.Offline;
            }
            else if (elapsed > warningThreshold)
            {
                newStatus = ClusterStatus.Warning;
            }
            else
            {
                newStatus = ClusterStatus.Online;
            }

            if (newStatus != cluster.Status)
            {
                _logger.LogInformation(
                    "Cluster {ClusterId} status changed: {OldStatus} -> {NewStatus}",
                    cluster.Id, cluster.Status, newStatus);
                
                cluster.Status = newStatus;
                cluster.UpdatedAt = now;
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    #region Helper Methods

    private ClusterDto MapToDto(Cluster cluster)
    {
        return new ClusterDto
        {
            Id = cluster.Id,
            Name = cluster.Name,
            Env = cluster.Env,
            AgentUrl = cluster.AgentUrl,
            Status = cluster.Status.ToString(),
            LastHeartbeat = cluster.LastHeartbeat,
            CreatedAt = cluster.CreatedAt
        };
    }

    private string GenerateToken()
    {
        // 生成随机 Token: qap_tok_{guid}
        return $"qap_tok_{Guid.NewGuid():N}";
    }

    private string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hash);
    }

    #endregion
}
