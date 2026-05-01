# Tasks: Agent-Scheduler 平台架构重构

## Phase 1: 接口定义与基础设施

### 1.1 Shared DTO 定义

- [x] 1.1.1 新建 `Shared/Models/SchedulerInfoDtos.cs`
- [x] 1.1.2 新建 `Shared/Models/AgentRegistrationDtos.cs`
- [x] 1.1.3 删除 `Shared/Models/ClusterDtos.cs`
- [x] 1.1.4 删除 `Shared/Enums/ClusterStatus.cs`
- [x] 1.1.5 修改 `Shared/Models/JobDefinitionDto.cs`：`ClusterId` → `SchedulerName`
- [x] 1.1.6 修改 `Shared/Models/JobManifestDto.cs`：移除 `ClusterId`

### 1.2 Agent 接口定义

- [x] 1.2.1 新建 `Agent/Services/IAgentSchedulerAccessor.cs`
- [x] 1.2.2 新建 `Agent/Services/AgentSchedulerAccessor.cs`
- [x] 1.2.3 新建 `Agent/Services/DeferredSchedulerAccessor.cs`
- [x] 1.2.4 新建 `Agent/Services/IAgentIdentityStore.cs`
- [x] 1.2.5 新建 `Agent/Services/AgentIdentityFileStore.cs`
- [x] 1.2.6 新建 `Agent/Services/SchedulerReporterService.cs`

### 1.3 EF Core 基础设施（DateTimeOffset → UTC 落库）

- [x] 1.3.1 修改 `PlatformDbContext.ConfigureConventions()`，增加全局 `timestamptz` 约定
- [x] 1.3.2 在 `PlatformDbContext.OnModelCreating()` 中增加 Value Converter
- [x] 1.3.3 新建 `Platform/Data/UtcAuditInterceptor.cs`
- [x] 1.3.4 修改 `Program.cs`，注册 `UtcAuditInterceptor`
- [x] 1.3.5 在所有 Agent 侧代码中检查：新 `DateTimeOffset` 值统一使用 `DateTimeOffset.UtcNow`

## Phase 2: Agent 端改造

### 2.1 注册流程改造

- [x] 2.1.1 修改 `AgentRegistrationService.cs`
- [x] 2.1.2 修改 `AgentRegistrationInfo`（重构）
- [x] 2.1.3 修改 `AgentConfig.cs`

### 2.2 生命周期改造

- [x] 2.2.1 修改 `HostedAgentService.ExecuteAsync()`
- [x] 2.2.2 修改 `SendHeartbeatAsync()`

### 2.3 Scheduler Accessor 集成

- [x] 2.3.1 修改 `AgentExtensions.RegisterAgentServices()`
- [x] 2.3.2 修改 `QuartzService.cs`

### 2.4 API 端点改造

- [x] 2.4.1 修改 `AgentApiExtensions.cs`
- [x] 2.4.2 新增 `GET /api/agent/schedulers` 端点

### 2.5 配置清理

- [x] 2.5.1 删除 `AgentConfig.Agent.ClusterId`
- [x] 2.5.2 修改 `ValidateAgentConfigOptions.cs`，移除 ClusterId 验证
- [x] 2.5.3 修改 `PostConfigureAgentConfigOptions.cs`，移除 AgentId 随机生成

## Phase 3: Platform 端改造

### 3.1 新实体与数据库

- [x] 3.1.1 新建 `Platform/Data/Entities/Agent.cs`
- [x] 3.1.2 新建 `Platform/Data/Entities/SchedulerInfo.cs`
- [x] 3.1.3 新建 `Platform/Data/Entities/AgentScheduler.cs`
- [x] 3.1.4 修改 `PlatformDbContext.cs`
- [x] 3.1.5 生成 EF Core Migration（AgentSchedulerRedesign）

### 3.2 新服务

- [x] 3.2.1 新建 `Platform/Services/AgentService.cs`
- [x] 3.2.2 新建 `Platform/Services/SchedulerService.cs`
- [x] 3.2.3 新建 `Platform/Services/SchedulerRouterService.cs`

### 3.3 新控制器

- [x] 3.3.1 新建 `Platform/Controllers/AgentsController.cs`
- [x] 3.3.2 新建 `Platform/Controllers/SchedulersController.cs`

### 3.4 改造现有服务

- [x] 3.4.1 修改 `AgentProxyService.cs`
- [x] 3.4.2 改造 `JobsController.cs`
- [x] 3.4.3 改造 `JobService.cs`
- [x] 3.4.4 改造 `ManifestController.cs`

### 3.5 删除废弃代码

- [x] 3.5.1 删除 `Controllers/ClustersController.cs`
- [x] 3.5.2 删除 `Controllers/AgentInstancesController.cs`
- [x] 3.5.3 删除 `Services/ClusterService.cs`
- [x] 3.5.4 删除 `Services/AgentInstanceService.cs`
- [x] 3.5.5 删除 `BackgroundServices/ClusterStatusMonitorService.cs`
- [ ] 3.5.6 数据迁移阶段再删除 `Data/Entities/Cluster.cs`
- [ ] 3.5.7 数据迁移阶段再删除 `Data/Entities/AgentInstance.cs`
- [ ] 3.5.8 数据迁移阶段再确认 `Data/Entities/JobDefinition.cs`

## Phase 4: UI 改造

### 4.1 类型与 API

- [x] 4.1.1 修改 `types/index.ts`
- [x] 4.1.2 修改 `api/index.ts`

### 4.2 新页面

- [x] 4.2.1 新建 `pages/AgentsPage.tsx`
- [x] 4.2.2 新建 `pages/AgentDetailPage.tsx`
- [x] 4.2.3 新建 `pages/SchedulersPage.tsx`
- [x] 4.2.4 新建 `pages/SchedulerDetailPage.tsx`

### 4.3 改造现有页面

- [x] 4.3.1 JobsPage.tsx 改造（路由改为 /schedulers/{name}/jobs）
- [x] 4.3.2 JobDetailPage.tsx 改造（路由改为 /schedulers/{name}/jobs/{key}）
- [x] 4.3.3 改造 `PlatformDashboardPage.tsx`
- [x] 4.3.4 改造 `App.tsx`

### 4.4 删除废弃页面

- [x] 4.4.1 删除 `pages/ClustersPage.tsx`
- [x] 4.4.2 删除 `pages/ClusterDashboardPage.tsx`
- [x] 4.4.3 删除 `pages/ClusterDetailPage.tsx`
- [x] 4.4.4 删除 `pages/AgentInstancesPage.tsx`
- [x] 4.4.5 删除 `components/ClusterTabs.tsx`
- [x] 4.4.6 删除 `components/CreateClusterModal.tsx`
- [x] 4.4.7 删除 `hooks/useClusters.ts`
- [x] 4.4.8 删除 `hooks/useAgentInstances.ts`

## Phase 5: 清理与验证

### 5.1 数据库迁移

- [x] 5.1.1 创建数据迁移脚本：AgentInstance → Agent（docs/migrations/v2_migrate_agent_instance_to_agent.sql）
- [x] 5.1.2 创建数据迁移脚本：Cluster → 保留但标记废弃（docs/migrations/v2_migrate_cluster_deprecate.sql）
- [ ] 5.1.3 验证迁移前后数据一致性（需手动执行迁移后校验）
- [ ] 5.1.4 删除 Cluster 表（确认无依赖后操作）

### 5.2 端到端验证

- [ ] 5.2.1-5.2.9 端到端验证（需部署后手动验证，自动化测试脚本待创建）

### 5.3 兼容性

- [x] 5.3.1 新建 `Controllers/Old/OldClustersRedirectController.cs`（301 重定向）
- [x] 5.3.2 AgentInstances 端点已删除，复用的 Cluster 重定向覆盖
- [x] 5.3.3 更新 API 文档（docs/api-reference.md）
- [x] 5.3.4 更新 README.md

### 5.4 文档

- [x] 5.4.1 更新 `docs/agent-scheduler-management.md`（替换 cluster-management.md）
- [x] 5.4.2 更新 `openspec/specs/system-architecture/spec.md`（v2.0.0 架构说明）
- [x] 5.4.3 新建 `docs/agent-scheduler-management.md` 替代 `docs/cluster-management.md`
