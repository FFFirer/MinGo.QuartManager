using Microsoft.EntityFrameworkCore;
using MinGo.Qap.Platform.Data;
using MinGo.Qap.Platform.Data.Entities;
using MinGo.Qap.Shared.Enums;
using MinGo.Qap.Shared.Models;
using System.Security.Cryptography;
using System.Text;

namespace MinGo.Qap.Platform.Services;

/// <summary>
/// Agent 实例服务实现
/// </summary>
public class AgentInstanceService : IAgentInstanceService
{
    private readonly PlatformDbContext _dbContext;
    private readonly ILogger<AgentInstanceService> _logger;
    private readonly IConfiguration _configuration;
    
    // 配置常量
    private const int DefaultHeartbeatIntervalSeconds = 30;
    private const int DefaultWarningThresholdSeconds = 30;
    private const int DefaultOfflineThresholdSeconds = 60;

    public AgentInstanceService(
        PlatformDbContext dbContext, 
        ILogger<AgentInstanceService> logger,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<AgentRegistrationResponse> RegisterAgentAsync(
        string clusterId, 
        CreateAgentRequest request, 
        string token)
    {
        // 验证集群存在
        var cluster = await _dbContext.Clusters
            .FirstOrDefaultAsync(c => c.Id == clusterId && c.DeletedAt == null);
        if (cluster == null)
        {
            throw new ArgumentException($"Cluster '{clusterId}' not found or deleted");
        }
        
        // 验证令牌
        if (!await ValidateClusterTokenAsync(clusterId, token))
        {
            throw new UnauthorizedAccessException("Invalid or expired token");
        }
        
        // 检查是否已存在相同 URL 的实例
        var existingInstance = await _dbContext.AgentInstances
            .FirstOrDefaultAsync(ai => ai.ClusterId == clusterId && ai.Url == request.Url && ai.DeletedAt == null);
        if (existingInstance != null)
        {
            // 如果已存在且已删除，恢复它
            if (existingInstance.DeletedAt != null)
            {
                existingInstance.DeletedAt = null;
                existingInstance.Status = AgentStatus.Pending;
                existingInstance.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                
                return new AgentRegistrationResponse
                {
                    AgentId = existingInstance.Id,
                    QuartzInstanceId = existingInstance.QuartzInstanceId ?? GenerateQuartzInstanceId(clusterId),
                    ClusterId = clusterId,
                    PlatformApiBaseUrl = GetPlatformApiBaseUrl(),
                    HeartbeatIntervalSeconds = DefaultHeartbeatIntervalSeconds,
                    WarningThresholdSeconds = DefaultWarningThresholdSeconds,
                    OfflineThresholdSeconds = DefaultOfflineThresholdSeconds
                };
            }
            
            throw new InvalidOperationException($"Agent instance with URL '{request.Url}' already exists for cluster '{clusterId}'");
        }
        
        // 创建新的 Agent 实例
        var agentId = GenerateAgentId();
        var quartzInstanceId = request.QuartzInstanceId ?? GenerateQuartzInstanceId(clusterId);
        var tokenHash = HashToken(token);
        
        var agentInstance = new AgentInstance
        {
            Id = agentId,
            ClusterId = clusterId,
            Name = request.Name,
            Url = request.Url,
            Status = AgentStatus.Pending,
            QuartzInstanceId = quartzInstanceId,
            TokenHash = tokenHash,
            AgentVersion = request.AgentVersion,
            StartedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        await _dbContext.AgentInstances.AddAsync(agentInstance);
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Agent instance registered: {AgentId} for cluster {ClusterId}", agentId, clusterId);
        
        return new AgentRegistrationResponse
        {
            AgentId = agentId,
            QuartzInstanceId = quartzInstanceId,
            ClusterId = clusterId,
            PlatformApiBaseUrl = GetPlatformApiBaseUrl(),
            HeartbeatIntervalSeconds = DefaultHeartbeatIntervalSeconds,
            WarningThresholdSeconds = DefaultWarningThresholdSeconds,
            OfflineThresholdSeconds = DefaultOfflineThresholdSeconds
        };
    }

    public async Task<AgentHeartbeatResponse> UpdateHeartbeatAsync(string agentId, AgentHeartbeatRequest request)
    {
        var agentInstance = await _dbContext.AgentInstances
            .FirstOrDefaultAsync(ai => ai.Id == agentId && ai.DeletedAt == null);
        if (agentInstance == null)
        {
            return new AgentHeartbeatResponse
            {
                Success = false,
                Message = $"Agent instance '{agentId}' not found or deleted"
            };
        }
        
        // 更新心跳时间和状态
        agentInstance.LastHeartbeat = DateTime.UtcNow;
        agentInstance.QuartzInstanceId = request.QuartzInstanceId ?? agentInstance.QuartzInstanceId;
        agentInstance.AgentVersion = request.AgentVersion ?? agentInstance.AgentVersion;
        agentInstance.UpdatedAt = DateTime.UtcNow;
        
        // 计算并更新状态
        var newStatus = CalculateInstanceStatus(agentInstance.LastHeartbeat);
        if (agentInstance.Status != newStatus)
        {
            agentInstance.Status = newStatus;
            _logger.LogInformation("Agent instance {AgentId} status changed from {OldStatus} to {NewStatus}", 
                agentId, agentInstance.Status, newStatus);
        }
        
        await _dbContext.SaveChangesAsync();
        
        _logger.LogDebug("Heartbeat received for agent {AgentId}", agentId);
        
        return new AgentHeartbeatResponse
        {
            Success = true,
            NextHeartbeatIntervalSeconds = DefaultHeartbeatIntervalSeconds
        };
    }

    public async Task<AgentInstanceDto?> GetInstanceAsync(string agentId)
    {
        var agentInstance = await _dbContext.AgentInstances
            .Include(ai => ai.Cluster)
            .FirstOrDefaultAsync(ai => ai.Id == agentId && ai.DeletedAt == null);
        
        if (agentInstance == null)
        {
            return null;
        }
        
        return MapToDto(agentInstance);
    }

    public async Task<List<AgentInstanceDto>> GetInstancesByClusterAsync(string clusterId, bool includeDeleted = false)
    {
        var query = _dbContext.AgentInstances
            .Include(ai => ai.Cluster)
            .Where(ai => ai.ClusterId == clusterId);
        
        if (!includeDeleted)
        {
            query = query.Where(ai => ai.DeletedAt == null);
        }
        
        var instances = await query.ToListAsync();
        return instances.Select(MapToDto).ToList();
    }

    public async Task<List<AgentInstanceDto>> GetHealthyInstancesAsync(string clusterId)
    {
        var instances = await _dbContext.AgentInstances
            .Include(ai => ai.Cluster)
            .Where(ai => ai.ClusterId == clusterId && 
                         ai.DeletedAt == null && 
                         (ai.Status == AgentStatus.Online || ai.Status == AgentStatus.Warning))
            .ToListAsync();
        
        return instances.Select(MapToDto).ToList();
    }

    public async Task<AgentInstanceDto?> UpdateInstanceAsync(string agentId, CreateAgentRequest request)
    {
        var agentInstance = await _dbContext.AgentInstances
            .FirstOrDefaultAsync(ai => ai.Id == agentId && ai.DeletedAt == null);
        if (agentInstance == null)
        {
            return null;
        }
        
        // 更新字段
        agentInstance.Name = request.Name ?? agentInstance.Name;
        agentInstance.Url = request.Url ?? agentInstance.Url;
        agentInstance.AgentVersion = request.AgentVersion ?? agentInstance.AgentVersion;
        agentInstance.QuartzInstanceId = request.QuartzInstanceId ?? agentInstance.QuartzInstanceId;
        agentInstance.UpdatedAt = DateTime.UtcNow;
        
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Agent instance updated: {AgentId}", agentId);
        
        return MapToDto(agentInstance);
    }

    public async Task<bool> DeleteInstanceAsync(string agentId)
    {
        var agentInstance = await _dbContext.AgentInstances
            .FirstOrDefaultAsync(ai => ai.Id == agentId && ai.DeletedAt == null);
        if (agentInstance == null)
        {
            return false;
        }
        
        // 软删除
        agentInstance.DeletedAt = DateTime.UtcNow;
        agentInstance.Status = AgentStatus.Deleted;
        agentInstance.UpdatedAt = DateTime.UtcNow;
        
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Agent instance deleted: {AgentId}", agentId);
        return true;
    }

    public async Task<bool> ValidateTokenAsync(string agentId, string token)
    {
        var agentInstance = await _dbContext.AgentInstances
            .FirstOrDefaultAsync(ai => ai.Id == agentId && ai.DeletedAt == null);
        if (agentInstance == null)
        {
            return false;
        }
        
        var tokenHash = HashToken(token);
        return agentInstance.TokenHash == tokenHash;
    }

    public AgentStatus CalculateInstanceStatus(DateTime? lastHeartbeat)
    {
        if (!lastHeartbeat.HasValue)
        {
            return AgentStatus.Pending;
        }
        
        var now = DateTime.UtcNow;
        var secondsSinceHeartbeat = (now - lastHeartbeat.Value).TotalSeconds;
        
        if (secondsSinceHeartbeat <= DefaultWarningThresholdSeconds)
        {
            return AgentStatus.Online;
        }
        else if (secondsSinceHeartbeat <= DefaultOfflineThresholdSeconds)
        {
            return AgentStatus.Warning;
        }
        else
        {
            return AgentStatus.Offline;
        }
    }

    public async Task<AgentStatus> UpdateInstanceStatusAsync(string agentId, DateTime? lastHeartbeat)
    {
        var agentInstance = await _dbContext.AgentInstances
            .FirstOrDefaultAsync(ai => ai.Id == agentId && ai.DeletedAt == null);
        if (agentInstance == null)
        {
            return AgentStatus.Deleted;
        }
        
        var newStatus = CalculateInstanceStatus(lastHeartbeat);
        if (agentInstance.Status != newStatus)
        {
            agentInstance.Status = newStatus;
            agentInstance.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
        
        return newStatus;
    }

    public async Task<int> UpdateClusterInstanceStatusesAsync(string clusterId)
    {
        var instances = await _dbContext.AgentInstances
            .Where(ai => ai.ClusterId == clusterId && ai.DeletedAt == null)
            .ToListAsync();
        
        var updatedCount = 0;
        var now = DateTime.UtcNow;
        
        foreach (var instance in instances)
        {
            var newStatus = CalculateInstanceStatus(instance.LastHeartbeat);
            if (instance.Status != newStatus)
            {
                instance.Status = newStatus;
                instance.UpdatedAt = now;
                updatedCount++;
            }
        }
        
        if (updatedCount > 0)
        {
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Updated status for {UpdatedCount} agent instances in cluster {ClusterId}", updatedCount, clusterId);
        }
        
        return updatedCount;
    }

    public async Task<InstanceSummaryDto> GetInstanceSummaryAsync(string clusterId)
    {
        var instances = await _dbContext.AgentInstances
            .Where(ai => ai.ClusterId == clusterId && ai.DeletedAt == null)
            .ToListAsync();
        
        return new InstanceSummaryDto
        {
            OnlineCount = instances.Count(ai => ai.Status == AgentStatus.Online),
            WarningCount = instances.Count(ai => ai.Status == AgentStatus.Warning),
            OfflineCount = instances.Count(ai => ai.Status == AgentStatus.Offline),
            PendingCount = instances.Count(ai => ai.Status == AgentStatus.Pending),
            TotalCount = instances.Count
        };
    }

    #region 私有辅助方法

    private async Task<bool> ValidateClusterTokenAsync(string clusterId, string token)
    {
        var cluster = await _dbContext.Clusters
            .FirstOrDefaultAsync(c => c.Id == clusterId && c.DeletedAt == null);
        if (cluster == null)
        {
            return false;
        }
        
        var tokenHash = HashToken(token);
        return cluster.TokenHash == tokenHash;
    }

    private string GenerateAgentId()
    {
        return $"agt-{Guid.NewGuid().ToString()[..12]}";
    }

    private string GenerateQuartzInstanceId(string clusterId)
    {
        var machineName = Environment.MachineName;
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        return $"{clusterId}-{machineName}-{timestamp}";
    }

    private string GetPlatformApiBaseUrl()
    {
        return _configuration["Platform:ApiBaseUrl"] ?? "http://localhost:5000";
    }

    private string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private AgentInstanceDto MapToDto(AgentInstance agentInstance)
    {
        return new AgentInstanceDto
        {
            Id = agentInstance.Id,
            ClusterId = agentInstance.ClusterId,
            Name = agentInstance.Name,
            Url = agentInstance.Url,
            Status = agentInstance.Status,
            LastHeartbeat = agentInstance.LastHeartbeat,
            QuartzInstanceId = agentInstance.QuartzInstanceId,
            AgentVersion = agentInstance.AgentVersion,
            StartedAt = agentInstance.StartedAt,
            CreatedAt = agentInstance.CreatedAt,
            UpdatedAt = agentInstance.UpdatedAt
        };
    }

    #endregion
}