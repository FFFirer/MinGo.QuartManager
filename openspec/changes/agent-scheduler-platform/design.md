# Design: Agent-Scheduler 平台架构重构

## Context

### 当前状态

**源码结构**：
```
src/
  MinGo.Qap.Agent/          — Agent 类库（嵌入宿主程序）
    Services/
      HostedAgentService.cs       生命周期管理
      AgentRegistrationService.cs  注册（含 clusterId）
      QuartzService.cs             单 IScheduler 封装
      HeartbeatService.cs          心跳（已 Obsolete）
      JobDiscoveryService.cs       Job 发现
      JobRegistry.cs               Job 类型注册表
      JobConverter.cs              Job/Trigger 转换
      AgentUrlResolver.cs          URL 解析
      HealthCheckService.cs        健康检查
      LogCollectionService.cs      日志收集
    Configuration/
      AgentConfig.cs              配置（含 ClusterId）
  MinGo.Qap.Platform/      — Platform Web API
    Controllers/
      ClustersController.cs       Cluster CRUD
      AgentInstancesController.cs  Agent 实例管理
      JobsController.cs            Job 操作
    Services/
      ClusterService.cs            Cluster 管理
      AgentInstanceService.cs      Agent 实例管理
      AgentProxyService.cs         Agent 请求代理
      JobService.cs                Job 操作转发
    Data/
      Entities/Cluster.cs          Cluster 实体
      Entities/AgentInstance.cs    Agent 实例实体
      PlatformDbContext.cs         DbContext（含 Clusters/AgentInstances/JobDefinitions）
  MinGo.Qap.Shared/         — 共享 DTO
    Models/
      ClusterDtos.cs              Cluster DTO
      AgentInstanceDto.cs          Agent DTO
      AgentRegistrationResponse.cs 注册响应
      AgentHeartbeatRequest.cs     心跳 DTO
      JobManifestDtos.cs           Job 清单 DTO
    Enums/
      ClusterStatus.cs
      AgentStatus.cs
  MinGo.Qap.UI/             — React 前端
    pages/
      ClustersPage.tsx
      ClusterDashboardPage.tsx
      ClusterDetailPage.tsx
      AgentInstancesPage.tsx
      JobsPage.tsx
      JobDetailPage.tsx
    components/
      ClusterTabs.tsx
      CreateClusterModal.tsx
```

**关键问题**：
1. Agent 注册必须携带 `ClusterId`，每集群手动创建
2. IScheduler 以单例注册，Agent 只能操作一个 Scheduler
3. AgentId 每次启动重新生成（`PostConfigureAgentConfigOptions` 第15行）
4. Platform 无 Scheduler 运行时信息

### 命名约束

- **IAgentSchedulerAccessor**：Agent 侧获取 IScheduler 的接口。不可使用 `ISchedulerRepository`，该名称与 Quartz.NET 内部接口冲突。

### 时间类型强制约定

#### 原则

所有时间字段**必须**满足以下三点：

1. **代码类型**：使用 `DateTimeOffset`，禁止使用 `DateTime`
2. **写入时**：调用处始终传入 `DateTimeOffset.UtcNow`，确保偏移量为 +00:00
3. **存储格式**：数据库列类型为 `timestamptz`（PostgreSQL 时区感知时间戳）

#### 强制机制

**EF Core 全局配置（Platform 侧）**：

```csharp
// PlatformDbContext.cs
protected override void ConfigureConventions(ModelConfigurationBuilder builder)
{
    // 所有 DateTimeOffset 属性映射到 timestamptz
    builder.Properties<DateTimeOffset>()
        .HaveColumnType("timestamptz");
}

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Value Converter：写入时强制转换为 UTC
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        foreach (var property in entityType.GetProperties()
                     .Where(p => p.ClrType == typeof(DateTimeOffset)))
        {
            property.SetValueConverter(
                new ValueConverter<DateTimeOffset, DateTimeOffset>(
                    v => v.ToUniversalTime(),    // 写入：转 UTC
                    v => v.ToUniversalTime()));   // 读取：统一返回 UTC
        }
    }
}
```

**Agent 侧代码约定**：

```csharp
// 正例：所有新时间值统一 Udage
var now = DateTimeOffset.UtcNow;

// 反例：禁止使用的写法
var bad1 = DateTimeOffset.Now;           // 本地时间，偏移量不确定
var bad2 = DateTime.UtcNow;              // DateTime 类型，时区信息丢失
var bad3 = DateTime.Now;                 // DateTime 本地时间，全面错误
```

**关键检查点**：
| 位置 | 字段 | 确保方式 |
|------|------|----------|
| Agent 上报 DTO | `RunningSince` | Agent 读取 Quartz 元数据后统一转 `UtcDateTime` |
| Agent 注册请求 | `RegisteredAt` | `DateTimeOffset.UtcNow` |
| Platform 实体 | `CreatedAt` / `UpdatedAt` | EF SaveChanges 拦截器自动填充 `DateTimeOffset.UtcNow` |
| 心跳 | `LastHeartbeat` | Agent 发心跳时传 `DateTimeOffset.UtcNow` |
| Scheduler 上报 | `LastReportedAt` | Platform 收请求时记录 `DateTimeOffset.UtcNow` |

#### 迁移现有 DateTime 字段

当前代码中大量使用 `DateTime`，迁移策略：
- 新实体全部使用 `DateTimeOffset`
- 旧实体在重构时逐步替换
- 过渡期保留旧字段但标记 `[Obsolete]`
- 最终统一为 `DateTimeOffset`

## Goals / Non-Goals

**Goals:**
1. 移除 Cluster 概念，简化为 Agent + Scheduler 双实体
2. Agent 身份持久化，重启可被 Platform 识别
3. Agent 注册后主动上报所有 Scheduler 运行时信息
4. 多 Scheduler 发现与路由支持
5. UI 以 Agent/Scheduler 为核心重构
6. 时间类型统一为 DateTimeOffset + UTC

**Non-Goals:**
- 不改变 Quartz 作业调度逻辑本身
- 不引入消息队列（保留 HTTP REST 通信）
- 不改变 Agent 端 Job 发现与注册机制
- 不涉及权限系统（RBAC 作为后续规划）

## Decisions

### 1. 接口定义：IAgentSchedulerAccessor

```csharp
// Agent/Services/IAgentSchedulerAccessor.cs
namespace MinGo.Qap.Agent.Services;

/// <summary>
/// Agent 端访问宿主程序中所有 IScheduler 的接口。
/// 命名避免与 Quartz.NET 内部的 ISchedulerRepository 冲突。
/// </summary>
public interface IAgentSchedulerAccessor
{
    /// <summary>获取所有已注册的 Scheduler</summary>
    IReadOnlyDictionary<string, IScheduler> GetAll();

    /// <summary>按名称获取 Scheduler</summary>
    IScheduler? GetScheduler(string schedulerName);

    /// <summary>Scheduler 数量</summary>
    int Count { get; }
}
```

**Rationale**: 选择 `IAgentSchedulerAccessor` 而非 `ISchedulerRepository`，因为：
- Quartz.NET 在 `Quartz` 命名空间下存在 `ISchedulerRepository` 接口
- `Accessor` 语义清晰——只读访问，不承担注册职责
- Agent 前缀避免与任何框架类型混淆

### 2. 默认实现策略

```csharp
// Agent/AgentExtensions.cs — 新增自动检测
private static void RegisterSchedulerAccessor(IServiceCollection services)
{
    services.AddSingleton<IAgentSchedulerAccessor>(sp =>
    {
        // 优先级 1：宿主显式注册了 IAgentSchedulerAccessor
        var explicitAccessor = sp.GetService<IAgentSchedulerAccessor>();
        if (explicitAccessor != null) return explicitAccessor;

        // 优先级 2：宿主有 IScheduler 集合（多 Scheduler 场景）
        var schedulers = sp.GetServices<IScheduler>()?.ToList();
        if (schedulers is { Count: > 0 })
        {
            return new AgentSchedulerAccessor(
                schedulers.ToDictionary(s => s.SchedulerName, s => s));
        }

        // 优先级 3：单 IScheduler（当前标准模式）
        var singleScheduler = sp.GetService<IScheduler>();
        if (singleScheduler != null)
        {
            return new AgentSchedulerAccessor(
                new Dictionary<string, IScheduler>
                {
                    [singleScheduler.SchedulerName] = singleScheduler
                });
        }

        // 优先级 4：延迟发现（宿主 Scheduler 初始化可能晚于 Agent）
        return new DeferredSchedulerAccessor(sp);
    });
}
```

**Rationale**: 多优先级确保：
- 持多种接入模式，现有应用无需改代码
- 通过 `GetServices<IScheduler>()` 支持多个 Scheduler 的 DI 注册
- 延迟发现解决 Agent 启动时 Scheduler 尚未就绪的问题

### 3. 身份持久化

```csharp
// Shared/Models/AgentIdentity.cs
public class AgentIdentity
{
    public string AgentId { get; set; } = string.Empty;
    public DateTimeOffset RegisteredAt { get; set; }
    public DateTimeOffset LastUpdatedAt { get; set; }
}

// Agent/Services/AgentIdentityFileStore.cs
public class AgentIdentityFileStore : IAgentIdentityStore
{
    private static readonly string FilePath =
        Path.Combine(AppContext.BaseDirectory, "agent-identity.json");

    public AgentIdentity? Load()
    {
        if (!File.Exists(FilePath)) return null;
        var json = File.ReadAllText(FilePath);
        return JsonSerializer.Deserialize<AgentIdentity>(json);
    }

    public void Save(AgentIdentity identity)
    {
        // 原子写入：写临时文件 → rename
        var tmp = FilePath + ".tmp";
        var json = JsonSerializer.Serialize(identity);
        File.WriteAllText(tmp, json);
        File.Move(tmp, FilePath, overwrite: true);
    }

    public void Clear()
    {
        if (File.Exists(FilePath)) File.Delete(FilePath);
    }
}
```

**Rationale**: 文件持久化方案无需数据库或外部依赖，适合内网环境。

### 4. 实体模型（DateTimeOffset + UTC）

```csharp
// Platform/Data/Entities/Agent.cs
public class Agent
{
    public string Id { get; set; } = string.Empty;       // agt-xxx
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;       // Agent HTTP 端点
    public string Status { get; set; } = "Pending";        // Online/Warning/Offline
    public string? AgentVersion { get; set; }
    public string? TokenHash { get; set; }

    // ⚠ 所有时间字段：DateTimeOffset + timestamptz + 只接受 UtcNow
    public DateTimeOffset? LastHeartbeat { get; set; }    // ← 仅从 Agent 心跳接收 UtcNow
    public DateTimeOffset? LastReportedAt { get; set; }  // ← Platform 记录：DateTimeOffset.UtcNow
    public DateTimeOffset StartedAt { get; set; }         // ← DateTimeOffset.UtcNow
    public DateTimeOffset CreatedAt { get; set; }         // ← DateTimeOffset.UtcNow
    public DateTimeOffset UpdatedAt { get; set; }          // ← DateTimeOffset.UtcNow
    public DateTimeOffset? DeletedAt { get; set; }         // ← DateTimeOffset.UtcNow

    // Navigation
    public List<AgentScheduler> AgentSchedulers { get; set; } = new();
}

// Platform/Data/Entities/SchedulerInfo.cs
public class SchedulerInfo
{
    public string Id { get; set; } = string.Empty;         // sch-xxx
    public string SchedulerName { get; set; } = string.Empty;
    public string? SchedulerInstanceId { get; set; }
    public string Status { get; set; } = "unknown";        // running/standby
    public bool IsClustered { get; set; }
    public string? JobStoreType { get; set; }
    public string? ThreadPoolType { get; set; }
    public int ThreadPoolSize { get; set; }
    public DateTimeOffset? RunningSince { get; set; }      // ← Agent 上报时转 Utc
    public string? Version { get; set; }
    public int NumberOfJobsExecuted { get; set; }
    public string? PropertiesJson { get; set; }            // 扩展属性

    public DateTimeOffset FirstReportedAt { get; set; }   // ← DateTimeOffset.UtcNow
    public DateTimeOffset LastReportedAt { get; set; }     // ← DateTimeOffset.UtcNow

    public List<AgentScheduler> AgentSchedulers { get; set; } = new();
}

// Platform/Data/Entities/AgentScheduler.cs（多对多关联）
public class AgentScheduler
{
    public string AgentId { get; set; } = string.Empty;
    public string SchedulerInfoId { get; set; } = string.Empty;
    public DateTimeOffset ReportedAt { get; set; }         // ← DateTimeOffset.UtcNow

    public Agent Agent { get; set; } = null!;
    public SchedulerInfo SchedulerInfo { get; set; } = null!;
}
```

**EF Core 配置——双重保证 UTC 落库**：

```csharp
// PlatformDbContext.cs
protected override void ConfigureConventions(ModelConfigurationBuilder builder)
{
    // 第一层保证：列类型强制 timestamptz
    builder.Properties<DateTimeOffset>()
        .HaveColumnType("timestamptz");
}

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // 第二层保证：Value Converter 在读写两端都归一化到 UTC
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        foreach (var property in entityType.GetProperties()
                     .Where(p => p.ClrType == typeof(DateTimeOffset)))
        {
            property.SetValueConverter(
                new ValueConverter<DateTimeOffset, DateTimeOffset>(
                    v => v.ToUniversalTime(),    // 写入 → 强制 UTC
                    v => v.ToUniversalTime()));   // 读取 → 统一返回 UTC
        }
    }
}
```

**SaveChanges 拦截器——自动填充审计时间**：

```csharp
// Platform/Data/UtcAuditInterceptor.cs
public class UtcAuditInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var entries = eventData.Context!.ChangeTracker
            .Entries()
            .Where(e => e.State == EntityState.Added ||
                        e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            // 查找所有 DateTimeOffset 属性，确保其值为 Utc
            foreach (var property in entry.Properties)
            {
                if (property.CurrentValue is DateTimeOffset dto)
                {
                    property.CurrentValue = dto.ToUniversalTime();
                }
            }

            // CreatedAt / UpdatedAt 自动赋值
            if (entry.State == EntityState.Added)
            {
                if (entry.Property("CreatedAt")?.CurrentValue == null)
                    entry.Property("CreatedAt").CurrentValue = DateTimeOffset.UtcNow;
            }
            if (entry.State == EntityState.Added ||
                entry.State == EntityState.Modified)
            {
                if (entry.Property("UpdatedAt") != null)
                    entry.Property("UpdatedAt").CurrentValue = DateTimeOffset.UtcNow;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
```

**DI 注册**：

```csharp
// Program.cs
builder.Services.AddDbContext<PlatformDbContext>(options =>
    options.UseNpgsql(connectionString)
           .AddInterceptors(new UtcAuditInterceptor()));
```

### 5. Scheduler 信息上报 DTO

```csharp
// Shared/Models/SchedulerInfoDtos.cs
public class SchedulerReportRequest
{
    public List<SchedulerInfoDto> Schedulers { get; set; } = new();
}

public class SchedulerInfoDto
{
    public string SchedulerName { get; set; } = string.Empty;
    public string? SchedulerInstanceId { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsClustered { get; set; }
    public string? JobStoreType { get; set; }
    public string? ThreadPoolType { get; set; }
    public int ThreadPoolSize { get; set; }
    public DateTimeOffset? RunningSince { get; set; }
    public string? Version { get; set; }
    public int NumberOfJobsExecuted { get; set; }
    public JobCountsDto? JobCounts { get; set; }
    public Dictionary<string, string>? Properties { get; set; }
}
```

### 6. 注册协议

```csharp
// Shared/Models/AgentRegistrationDtos.cs
public class RegisterAgentRequest
{
    public string? AgentId { get; set; }       // null=首次, 有值=重连
    public string? Name { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? AgentVersion { get; set; }
}

public class RegisterAgentResponse
{
    public string AgentId { get; set; } = string.Empty;
    public int HeartbeatIntervalSeconds { get; set; } = 30;
    public int WarningThresholdSeconds { get; set; } = 30;
    public int OfflineThresholdSeconds { get; set; } = 60;
}
```

### 7. API 路由对照

```
当前                               目标
────                                ────
POST   /api/clusters               【删除】
GET    /api/clusters
GET    /api/clusters/{id}
DELETE /api/clusters/{id}

POST   /api/clusters/{cid}/agents   POST   /api/agents
GET    /api/clusters/{cid}/agents   GET    /api/agents
                                     GET    /api/agents/{agentId}
                                     DELETE /api/agents/{agentId}
POST   /api/agents/{id}/heartbeat    POST   /api/agents/{id}/heartbeat（不变）

                                     【新增】
                                     POST   /api/agents/{id}/schedulers（上报）
                                     GET    /api/agents/{id}/schedulers（查询）
                                     GET    /api/schedulers
                                     GET    /api/schedulers/{name}
                                     GET    /api/schedulers/{name}/agents

POST   /api/clusters/{cid}/jobs      POST   /api/schedulers/{name}/jobs
GET    /api/clusters/{cid}/jobs      GET    /api/schedulers/{name}/jobs
GET    /api/clusters/{cid}/jobs/{k}  GET    /api/schedulers/{name}/jobs/{k}
PUT    /api/clusters/{cid}/jobs/{k}  PUT    /api/schedulers/{name}/jobs/{k}
DELETE /api/clusters/{cid}/jobs/{k}  DELETE /api/schedulers/{name}/jobs/{k}
POST   /api/clusters/{cid}/jobs/{k}/trigger
                                     POST   /api/schedulers/{name}/jobs/{k}/trigger
POST   /api/clusters/{cid}/jobs/{k}/pause
                                     POST   /api/schedulers/{name}/jobs/{k}/pause
POST   /api/clusters/{cid}/jobs/{k}/resume
                                     POST   /api/schedulers/{name}/jobs/{k}/resume
```

### 8. 平台端 Agent→Scheduler 路由

```csharp
// Platform/Services/SchedulerRouterService.cs（新增）
public class SchedulerRouterService
{
    /// <summary>根据 SchedulerName 选择一个可用的 Agent</summary>
    public async Task<Agent?> PickAgentForSchedulerAsync(string schedulerName)
    {
        // 1. 查 AgentScheduler 关联表
        var agentSchedulers = await _dbContext.AgentSchedulers
            .Include(a => a.Agent)
            .Where(a => a.SchedulerInfo.SchedulerName == schedulerName)
            .ToListAsync();

        if (agentSchedulers.Count == 0) return null;

        // 2. 过滤健康的 Agent，选一个
        var healthy = agentSchedulers
            .Where(a => a.Agent.Status == "Online")
            .Select(a => a.Agent)
            .ToList();

        if (healthy.Count == 0) return null;

        // 3. 随机选择（或轮询）
        return healthy[Random.Shared.Next(healthy.Count)];
    }
}
```

### 9. UI 导航结构

```
【当前】                        【目标】
Dashboard                      Dashboard
  ├── Clusters（下拉）           ├── Agents（新）
  │   ├── 最近 Cluster           │   └── Agent 详情 → 其 Scheduler 列表
  │   └── View All Clusters      │
  ├── [无]                      ├── Schedulers（新）
  └── Settings                   │   └── Scheduler 详情 → 关联 Agents → Job 操作
                                 └── Settings
```

### 10. Agent 启动序列（更新后）

```
Host App Start
       │
       ▼
Agent HostedAgentService Start
       │
       ├── 读取 agent-identity.json
       │   ├── 有 AgentId → 注册时携带
       │   └── 无 AgentId → 首次注册
       │
       ▼
POST /api/agents（注册）
  { agentId?, name, url, agentVersion }
       │
       ▼
← { agentId, heartbeatIntervalSeconds, ... }
       │
       ▼
写入 agent-identity.json（持久化 AgentId）
       │
       ▼
IAgentSchedulerAccessor.GetAll()
  → 枚举所有 IScheduler
  → 读取运行时元数据
       │
       ▼
POST /api/agents/{agentId}/schedulers（上报）
  { schedulers: [{ schedulerName, instanceId, status, ... }] }
       │
       ▼
进入心跳循环（Heartbeat + 状态摘要）
```

## Risks / Trade-offs

### Risk 1: Quartz.NET ISchedulerRepository 命名冲突
- **Impact**: 使用 `ISchedulerRepository` 会导致编译冲突
- **Mitigation**: 采用 `IAgentSchedulerAccessor`，与 Quartz 命名空间隔离
- **Detection**: 编译时即暴露，不会漏到运行时

### Risk 2: Agent 启动时 Scheduler 未就绪
- **Impact**: IScheduler 可能尚未创建或 Start()
- **Mitigation**: DeferredSchedulerAccessor 延迟重试，结合 IHostedService 启动顺序

### Risk 3: 多 Agent 共享同一 Scheduler（Quartz 集群）
- **Impact**: 同一 SchedulerName+InstanceId 被多个 Agent 上报
- **Mitigation**: SchedulerInfo 以 (SchedulerName, InstanceId) 联合去重，AgentScheduler 保留多对多

### Risk 4: DateTimeOffset 迁移破坏现有查询
- **Impact**: 现有使用 DateTime 的查询可能失效
- **Mitigation**: 分阶段迁移，旧字段保留 DateTime 兼容层，新代码全部使用 DateTimeOffset

### Risk 5: API 路由变更导致客户端中断
- **Impact**: 旧 Cluster 路由被删除，UI 和外部客户端不可用
- **Mitigation**: 保留临时 301 重定向，文档明确标记过渡期

## Migration Plan

### Phase 1: 接口定义与基础设施
1. 定义 `IAgentSchedulerAccessor`（注意命名避免冲突）
2. 定义 `IAgentIdentityStore` 与文件实现
3. 定义新 Shared DTO（DateTimeOffset 版本）
4. 配置 EF Core 全局 `timestamptz` 约定

### Phase 2: Agent 端改造
1. 实现 `AgentSchedulerAccessor` 多优先级检测
2. 实现 `AgentIdentityFileStore` 原子写入
3. 实现 `SchedulerReporterService` 采集+上报
4. 修改 `HostedAgentService` 加入身份读取和上报阶段
5. 修改 `AgentRegistrationService` 移除 ClusterId
6. 修改 `AgentApiExtensions` 支持 SchedulerName 路由
7. 移除 AgentConfig 中的 ClusterId

### Phase 3: Platform 端改造
1. 新建 Agent、SchedulerInfo、AgentScheduler 实体
2. 新建 AgentService、SchedulerService
3. 新建 AgentsController、SchedulersController
4. 改造 AgentProxyService 支持 schedulerName 路由
5. 改造 JobsController 路由
6. 删除 Cluster 相关代码
7. 生成 EF Migration

### Phase 4: UI 改造
1. 删除 Cluster 页面和组件
2. 新增 AgentsPage、AgentDetailPage
3. 新增 SchedulersPage、SchedulerDetailPage
4. 修改 App.tsx 路由
5. 修改 Sidebar 导航
6. 更新 API client 和 TypeScript 类型

### Phase 5: 清理与验证
1. 数据库数据迁移
2. 端到端测试
3. 旧端点 301 重定向
4. 更新现有 spec 文档

## Open Questions

1. **Q**: Quartz 集群场景下，Platform 应如何选择 Agent 执行操作？
   - **Proposed**: 任选一个健康的 Agent，Quartz 内部通过共享数据库协调

2. **Q**: Agent 的 `agent-identity.json` 文件路径是否可配置？
   - **Proposed**: 默认 `AppContext.BaseDirectory/agent-identity.json`，IOptions 可选覆盖

3. **Q**: Scheduler 信息上报是全量替换还是增量更新？
   - **Proposed**: 全量替换——Agent 上报完整列表，Platform 删除旧关联后重建

4. **Q**: 旧 Cluster 数据的数据库迁移如何处理？
   - **Proposed**: 新增 Migration 创建新表，将 AgentInstance → Agent 数据迁移，Cluster 表保留但标记废弃
