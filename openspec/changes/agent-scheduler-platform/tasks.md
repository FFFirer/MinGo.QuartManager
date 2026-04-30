# Tasks: Agent-Scheduler 平台架构重构

## Phase 1: 接口定义与基础设施

### 1.1 Shared DTO 定义

- [ ] 1.1.1 新建 `Shared/Models/SchedulerInfoDtos.cs`
  - `SchedulerInfoDto`（DateTimeOffset 字段）
  - `SchedulerReportRequest`（List<SchedulerInfoDto>）
  - Db 字段映射到 `timestamptz`
- [ ] 1.1.2 新建 `Shared/Models/AgentRegistrationDtos.cs`
  - `RegisterAgentRequest`（含可选 AgentId）
  - `RegisterAgentResponse`（DateTimeOffset）
- [ ] 1.1.3 删除 `Shared/Models/ClusterDtos.cs`
- [ ] 1.1.4 删除 `Shared/Enums/ClusterStatus.cs`
- [ ] 1.1.5 修改 `Shared/Models/JobDefinitionDto.cs`：`ClusterId` → `SchedulerName`
- [ ] 1.1.6 修改 `Shared/Models/JobManifestDto.cs`：移除 `ClusterId`

### 1.2 Agent 接口定义

- [ ] 1.2.1 新建 `Agent/Services/IAgentSchedulerAccessor.cs`
  - `GetAll()` → `IReadOnlyDictionary<string, IScheduler>`
  - `GetScheduler(string)` → `IScheduler?`
  - `Count` → `int`
  - 注意命名不冲突 Quartz.NET 的 `ISchedulerRepository`
- [ ] 1.2.2 新建 `Agent/Services/AgentSchedulerAccessor.cs`
  - 从 `IServiceProvider.GetServices<IScheduler>()` 构建字典
- [ ] 1.2.3 新建 `Agent/Services/DeferredSchedulerAccessor.cs`
  - 延迟发现：首次调用时再尝试，缓存结果
  - 重试间隔 500ms，最多 10 次
- [ ] 1.2.4 新建 `Agent/Services/IAgentIdentityStore.cs`
  - `Load()` → `AgentIdentity?`
  - `Save(AgentIdentity)`
  - `Clear()`
- [ ] 1.2.5 新建 `Agent/Services/AgentIdentityFileStore.cs`
  - 文件路径：`agent-identity.json`
  - 原子写入（临时文件 → Move）
  - JSON 序列化：System.Text.Json
- [ ] 1.2.6 新建 `Agent/Services/SchedulerReporterService.cs`
  - 采集：遍历 `IAgentSchedulerAccessor.GetAll()`，读取每个 IScheduler 元数据
  - 上报：`POST /api/agents/{agentId}/schedulers`
  - 重试：最多 3 次，指数退避

### 1.3 EF Core 基础设施（DateTimeOffset → UTC 落库）

- [ ] 1.3.1 修改 `PlatformDbContext.ConfigureConventions()`，增加全局 `timestamptz` 约定
  ```csharp
  builder.Properties<DateTimeOffset>().HaveColumnType("timestamptz");
  ```
- [ ] 1.3.2 在 `PlatformDbContext.OnModelCreating()` 中增加 Value Converter
  - 遍历所有 `DateTimeOffset` 属性
  - 写入时 `v.ToUniversalTime()`
  - 读取时 `v.ToUniversalTime()`
- [ ] 1.3.3 新建 `Platform/Data/UtcAuditInterceptor.cs`
  - 实现 `SaveChangesInterceptor`
  - `SavingChangesAsync` 中扫描所有 `DateTimeOffset` 属性，强制 `.ToUniversalTime()`
  - `Added` 状态自动填充 `CreatedAt = DateTimeOffset.UtcNow`
  - `Added/Modified` 自动更新 `UpdatedAt = DateTimeOffset.UtcNow`
- [ ] 1.3.4 修改 `Program.cs`，注册 `UtcAuditInterceptor`
  ```csharp
  .AddInterceptors(new UtcAuditInterceptor())
  ```
- [ ] 1.3.5 在所有 Agent 侧代码中检查：新 `DateTimeOffset` 值统一使用 `DateTimeOffset.UtcNow`，禁止 `DateTimeOffset.Now`

## Phase 2: Agent 端改造

### 2.1 注册流程改造

- [ ] 2.1.1 修改 `AgentRegistrationService.cs`
  - `RegisterAsync()` 签名：移除 `clusterId` 参数
  - 请求 URL：`POST /api/agents`
  - 请求体：`RegisterAgentRequest`（含从 `IAgentIdentityStore` 读取的 `AgentId`）
  - 认证头部：`X-Agent-Token` 保留
- [ ] 2.1.2 修改 `AgentRegistrationInfo`（重构）
  - 移除 `ClusterId`
  - 添加 `AgentIdentity` 引用
- [ ] 2.1.3 修改 `AgentConfig.cs`
  - `AgentSettings.ClusterId` → 标记 Obsolete 或删除
  - `PlatformSettings.ClusterId` → 删除

### 2.2 生命周期改造

- [ ] 2.2.1 修改 `HostedAgentService.ExecuteAsync()`
  ```
  流程改為：
    Phase 1: 读取身份 (IAgentIdentityStore.Load)
    Phase 2: 注册 (携带 AgentId)
    Phase 3: 写入身份 (第一次注册后持久化)
    Phase 4: 上报 Scheduler (SchedulerReporterService)
    Phase 5: 心跳循环
  ```
- [ ] 2.2.2 修改 `SendHeartbeatAsync()`
  - 心跳中携带 Scheduler 状态摘要
  - 若 Scheduler 列表变化触发主动上报

### 2.3 Scheduler Accessor 集成

- [ ] 2.3.1 修改 `AgentExtensions.RegisterAgentServices()`
  - 新增 `RegisterSchedulerAccessor(services)` 调用
  - 优先级链：显式 → 多 IScheduler → 单 IScheduler → Deferred
- [ ] 2.3.2 修改 `QuartzService.cs`
  - 构造函数：`IScheduler` → `IAgentSchedulerAccessor`
  - 所有操作方法：接收 `schedulerName` 参数
  - `GetSchedulerStateAsync()` 增加 schedulerName 参数

### 2.4 API 端点改造

- [ ] 2.4.1 修改 `AgentApiExtensions.cs`
  - 所有端点增加 `schedulerName` 解析（Header → Query → 默认）
  - 新增 `ParseSchedulerName(HttpRequest)` 辅助方法
  - 解析顺序：`X-Scheduler-Name` > `?schedulerName=` > 默认第一个
- [ ] 2.4.2 新增 `GET /api/agent/schedulers` 端点
  - 返回当前 Agent 所有 Scheduler 的运行时信息

### 2.5 配置清理

- [ ] 2.5.1 删除 `AgentConfig.Agent.ClusterId`
- [ ] 2.5.2 修改 `ValidateAgentConfigOptions.cs`，移除 ClusterId 验证
- [ ] 2.5.3 修改 `PostConfigureAgentConfigOptions.cs`，移除 AgentId 随机生成

## Phase 3: Platform 端改造

### 3.1 新实体与数据库

- [ ] 3.1.1 新建 `Platform/Data/Entities/Agent.cs`
  - 所有时间字段 `DateTimeOffset`
  - Url 最长 512
  - 索引：`Status`、`LastHeartbeat`
- [ ] 3.1.2 新建 `Platform/Data/Entities/SchedulerInfo.cs`
  - `SchedulerName` + `SchedulerInstanceId` 联合唯一
  - 所有时间字段 `DateTimeOffset`
- [ ] 3.1.3 新建 `Platform/Data/Entities/AgentScheduler.cs`
  - 复合主键 `(AgentId, SchedulerInfoId)`
  - `ReportedAt` 为 `DateTimeOffset`
- [ ] 3.1.4 修改 `PlatformDbContext.cs`
  - 移除 `DbSet<Cluster>`
  - 替换 `DbSet<AgentInstance>` → `DbSet<Agent>`
  - 新增 `DbSet<SchedulerInfo>`、`DbSet<AgentScheduler>`
  - 应用全局 `timestamptz` 约定
- [ ] 3.1.5 生成 EF Core Migration

### 3.2 新服务

- [ ] 3.2.1 新建 `Platform/Services/AgentService.cs`
  - `RegisterAsync(RegisterAgentRequest, token)` — 首次注册/重连
  - `GetAsync(agentId)` — 获取 Agent 详情
  - `GetAllAsync()` — Agent 列表
  - `DeleteAsync(agentId)` — 软删除
  - `UpdateHeartbeatAsync(agentId)` — 心跳更新
  - 重连逻辑：AgentId 存在则 UPDATE，不存在则 CREATE
- [ ] 3.2.2 新建 `Platform/Services/SchedulerService.cs`
  - `ReportSchedulersAsync(agentId, SchedulerReportRequest)` — 接收上报
  - 全量替换策略：删旧关联 → 合并/新增 SchedulerInfo → 重建 AgentScheduler
  - `GetSchedulerAsync(name)` — Scheduler 详情（含 Agents）
  - `GetAllSchedulersAsync()` — 全局 Scheduler 列表
  - `GetAgentsBySchedulerAsync(name)` — 查询关联 Agents
  - `GetSchedulersByAgentAsync(agentId)` — 查询 Agent 的 Schedulers
- [ ] 3.2.3 新建 `Platform/Services/SchedulerRouterService.cs`
  - `PickAgentForSchedulerAsync(schedulerName)` — 选择健康 Agent
  - 实现随机选择，后续可扩展为轮询/一致性哈希

### 3.3 新控制器

- [ ] 3.3.1 新建 `Platform/Controllers/AgentsController.cs`
  - `POST /api/agents` — 注册
  - `GET /api/agents` — 列表
  - `GET /api/agents/{agentId}` — 详情
  - `DELETE /api/agents/{agentId}` — 删除
  - `POST /api/agents/{agentId}/heartbeat` — 心跳
  - `POST /api/agents/{agentId}/schedulers` — 上报 Scheduler
  - `GET /api/agents/{agentId}/schedulers` — 查询 Agent 的 Scheduler
- [ ] 3.3.2 新建 `Platform/Controllers/SchedulersController.cs`
  - `GET /api/schedulers` — 全局列表
  - `GET /api/schedulers/{schedulerName}` — 详情
  - `GET /api/schedulers/{schedulerName}/agents` — 关联的 Agents

### 3.4 改造现有服务

- [ ] 3.4.1 修改 `AgentProxyService.cs`
  - 方法签名增加 `schedulerName` 参数
  - 转发时设置 `X-Scheduler-Name` 请求头
  - 内部调用 `SchedulerRouterService` 选择 Agent
- [ ] 3.4.2 改造 `JobsController.cs`
  - 路由：`/api/clusters/{clusterId}/jobs` → `/api/schedulers/{schedulerName}/jobs`
  - 所有操作通过 `SchedulerRouterService` 选择 Agent
  - 保留旧路由为 301 重定向
- [ ] 3.4.3 改造 `JobService.cs`
  - `ClusterId` 参数 → `SchedulerName` 参数
  - 调用 `AgentProxyService` 时传入 `schedulerName`
- [ ] 3.4.4 改造 `ManifestController.cs`（如保留）
  - 路由改为 `/api/schedulers/{schedulerName}/manifest`

### 3.5 删除废弃代码

- [ ] 3.5.1 删除 `Controllers/ClustersController.cs`
- [ ] 3.5.2 删除 `Controllers/AgentInstancesController.cs`
- [ ] 3.5.3 删除 `Services/ClusterService.cs`
- [ ] 3.5.4 删除 `Services/AgentInstanceService.cs`
- [ ] 3.5.5 删除 `BackgroundServices/ClusterStatusMonitorService.cs`
- [ ] 3.5.6 删除 `Data/Entities/Cluster.cs`
- [ ] 3.5.7 删除 `Data/Entities/AgentInstance.cs`
- [ ] 3.5.8 删除 `Data/Entities/JobDefinition.cs`（若 JobDefinition 设计改变）

## Phase 4: UI 改造

### 4.1 类型与 API

- [ ] 4.1.1 修改 `types/index.ts`
  - 删除 Cluster 相关类型
  - 新增 `AgentDto`、`SchedulerInfoDto`、`AgentSchedulerDto`
  - 新增 `RegisterAgentRequest/Response`
  - 所有新类型时间字段使用 `string`（ISO 8601 UTC）
  - 修改 `CreateJobRequest`，去掉 `clusterId`
- [ ] 4.1.2 修改 `api/index.ts`
  - 删除 `clusterApi`
  - 新增 `agentApi`（`getAll`, `get`, `getSchedulers`）
  - 新增 `schedulerApi`（`getAll`, `get`, `getAgents`）
  - 改造 `jobApi`：方法从 `(clusterId, ...)` 变为 `(schedulerName, ...)`
  - 新增 `agentInstanceApi`：`reportSchedulers`

### 4.2 新页面

- [ ] 4.2.1 新建 `pages/AgentsPage.tsx`
  - Agent 列表页：ID、Name、Url、Status、Scheduler 数量
  - 状态指示（Online/Warning/Offline）
  - 点击跳转 AgentDetailPage
- [ ] 4.2.2 新建 `pages/AgentDetailPage.tsx`
  - Agent 基本信息
  - 关联 Scheduler 列表（表格：SchedulerName, InstanceId, Status, 关联时间）
  - 点击 Scheduler 跳转 SchedulerDetailPage
- [ ] 4.2.3 新建 `pages/SchedulersPage.tsx`
  - 全局 Scheduler 列表
  - 列：SchedulerName, InstanceId, Status, IsClustered, Agent 数量, 最后上报时间
  - 搜索过滤
- [ ] 4.2.4 新建 `pages/SchedulerDetailPage.tsx`
  - Scheduler 运行时信息（状态卡片）
  - 关联 Agents 列表
  - Job 操作模块（切换标签到 Job 列表/创建）

### 4.3 改造现有页面

- [ ] 4.3.1 改造 `JobsPage.tsx`
  - 入口改为选择 Scheduler（URL: `/schedulers/{name}/jobs`）
  - 移除 Cluster 上下文
- [ ] 4.3.2 改造 `JobDetailPage.tsx`
  - 路由改为 `/schedulers/{name}/jobs/{key}`
  - 操作按钮：Trigger / Pause / Resume → 调用新 API
- [ ] 4.3.3 改造 `PlatformDashboardPage.tsx`
  - 统计卡片：Agent 总数 / Scheduler 总数 / Job 总数
  - 移除 Cluster 相关统计
- [ ] 4.3.4 改造 `App.tsx`
  - Sidebar：Cluster 菜单改为 Agents + Schedulers
  - 路由：`/agents/*`, `/schedulers/*`
  - 移除 `/clusters/*` 路由（可选保留 301 跳转）

### 4.4 删除废弃页面

- [ ] 4.4.1 删除 `pages/ClustersPage.tsx`
- [ ] 4.4.2 删除 `pages/ClusterDashboardPage.tsx`
- [ ] 4.4.3 删除 `pages/ClusterDetailPage.tsx`
- [ ] 4.4.4 删除 `pages/AgentInstancesPage.tsx`（由 AgentsPage 替代）
- [ ] 4.4.5 删除 `components/ClusterTabs.tsx`
- [ ] 4.4.6 删除 `components/CreateClusterModal.tsx`
- [ ] 4.4.7 删除 `hooks/useClusters.ts`

### 4.5 验证 UI

- [ ] 4.5.1 前端编译无错误（TypeScript strict）
- [ ] 4.5.2 Agent 列表渲染正常
- [ ] 4.5.3 Scheduler 列表渲染正常
- [ ] 4.5.4 从 Scheduler 入口创建/查看 Job 正常
- [ ] 4.5.5 Sidebar 导航正常

## Phase 5: 清理与验证

### 5.1 数据库迁移

- [ ] 5.1.1 创建数据迁移脚本：AgentInstance → Agent
- [ ] 5.1.2 创建数据迁移脚本：Cluster → 保留但标记废弃
- [ ] 5.1.3 验证迁移前后数据一致性
- [ ] 5.1.4 删除 Cluster 表（确认无依赖后）

### 5.2 端到端验证

- [ ] 5.2.1 Agent 启动→注册→收到 AgentId→持久化到本地
- [ ] 5.2.2 Agent 重启→读取 AgentId→重连成功
- [ ] 5.2.3 Agent 注册后自动上报 Scheduler 信息
- [ ] 5.2.4 Platform 正常存储和查询 Scheduler 信息
- [ ] 5.2.5 通过 Scheduler 路由执行 Job 操作（create/trigger/pause/resume）
- [ ] 5.2.6 多 Scheduler 场景：Agent 上报多个 Scheduler，路由正确
- [ ] 5.2.7 Quartz 集群场景：多个 Agent 上报同一 Scheduler，路由任选一个
- [ ] 5.2.8 Agent 离线 → 心跳超时 → 标记 Offline
- [ ] 5.2.9 Agent 重新上线 → 重连 → 恢复 Online

### 5.3 兼容性

- [ ] 5.3.1 配置 `old/ClustersController.cs` 为 301 重定向
- [ ] 5.3.2 配置 `old/AgentInstancesController.cs` 为 301 重定向
- [ ] 5.3.3 更新 API 文档（docs/api-reference.md）
- [ ] 5.3.4 更新 README.md

### 5.4 文档

- [ ] 5.4.1 更新 `docs/产品及架构方案.md`
- [ ] 5.4.2 更新 `openspec/specs/system-architecture/spec.md`
- [ ] 5.4.3 更新 `docs/cluster-management.md`（或移除并创建 agent-scheduler 文档）
