# Proposal: Agent-Scheduler 平台架构重构

## Why

当前架构以 **Cluster** 为核心设计，存在以下问题：

1. **Cluster 概念冗余**：Cluster 仅作为 Agent 的逻辑分组，不承载实际调度能力，增加理解和维护成本
2. **单 Scheduler 假设**：Agent 端假定宿主程序只有一个 IScheduler，无法支持多 Scheduler 场景
3. **Agent 身份不持久**：每次重启生成新的 AgentId，Platform 无法识别同一 Agent 的连续运行
4. **Scheduler 信息不透明**：Platform 不感知 Agent 内的 Quartz Scheduler 运行时细节
5. **配置依赖 ClusterId**：Agent 注册必须携带 clusterId，增加了部署配置复杂度

## What Changes

### 核心变更

1. **移除 Cluster 概念**：从 Platform、Agent、Shared、UI 全面删除 Cluster 实体和 API
2. **新增 Agent 身份持久化**：Agent 首次注册后从 Platform 获取唯一标识，本地持久化，重启后携带
3. **新增 ISchedulerAccessor**：Agent 侧接口，用于获取宿主程序中的所有 IScheduler 实例
4. **新增 Scheduler 主动上报**：Agent 注册成功后主动向 Platform 上报所有 Scheduler 运行时信息
5. **新增多对多关系**：Agent ↔ Scheduler 多对多关联，Platform 可按任意维度查询
6. **全量 DateTimeOffset 迁移**：新实体中时间字段统一使用 DateTimeOffset，数据库以 UTC 存储

### 删除的代码

| 组件 | 文件 |
|------|------|
| Platform | `Controllers/ClustersController.cs` |
| Platform | `Services/ClusterService.cs`, `IClusterService.cs` |
| Platform | `Data/Entities/Cluster.cs` |
| Platform | `BackgroundServices/ClusterStatusMonitorService.cs` |
| Shared | `Models/ClusterDtos.cs` |
| Shared | `Enums/ClusterStatus.cs` |
| UI | `pages/ClustersPage.tsx`, `ClusterDashboardPage.tsx`, `ClusterDetailPage.tsx` |
| UI | `components/ClusterTabs.tsx`, `CreateClusterModal.tsx` |

### 新增的代码

| 组件 | 文件 |
|------|------|
| Agent | `Services/IAgentSchedulerAccessor.cs` — Scheduler 访问接口 |
| Agent | `Services/AgentSchedulerAccessor.cs` — 默认实现（从 DI 发现） |
| Agent | `Services/IAgentIdentityStore.cs` — 身份持久化接口 |
| Agent | `Services/AgentIdentityFileStore.cs` — 文件实现 |
| Agent | `Services/SchedulerReporterService.cs` — Scheduler 上报服务 |
| Shared | `Models/SchedulerInfoDtos.cs` — Scheduler DTO 定义 |
| Shared | `Models/AgentRegistrationDtos.cs` — 简化注册 DTO |
| Platform | `Services/AgentService.cs` — 代替 ClusterService |
| Platform | `Services/SchedulerService.cs` — Scheduler 管理 |
| Platform | `Controllers/AgentsController.cs` — Agent CRUD |
| Platform | `Controllers/SchedulersController.cs` — Scheduler 查询 |
| Platform | `Data/Entities/Agent.cs` — 代替 AgentInstance |
| Platform | `Data/Entities/SchedulerInfo.cs` — Scheduler 运行时信息 |
| Platform | `Data/Entities/AgentScheduler.cs` — 多对多关联 |
| UI | `pages/AgentsPage.tsx` — Agent 列表 |
| UI | `pages/AgentDetailPage.tsx` — Agent 详情 |
| UI | `pages/SchedulersPage.tsx` — Scheduler 列表 |
| UI | `pages/SchedulerDetailPage.tsx` — Scheduler 详情 |

## Capabilities

### New Capabilities

#### 1. agent-identity-persistence
- Agent 启动时读取本地 `agent-identity.json` 文件
- 首次注册由 Platform 分配 AgentId，持久化到本地
- 后续重启自动携带 AgentId 重新连接

#### 2. multi-scheduler-discovery
- Agent 通过 `IAgentSchedulerAccessor` 发现宿主程序中的所有 IScheduler
- 支持宿主显式注册多个 Scheduler
- 兼容只注册单个 IScheduler 的场景

#### 3. scheduler-info-reporting
- Agent 注册成功后主动上报所有 Scheduler 信息
- 上报内容：SchedulerName, InstanceId, Status, IsClustered, JobStoreType, ThreadPool 等
- Platform 侧全量替换，维护最新快照

#### 4. agent-scheduler-routing
- Platform 按 SchedulerName 查找关联的 Agent
- Agent 端根据 `X-Scheduler-Name` 头部路由到对应 IScheduler
- 同一 Scheduler 被多个 Agent 共享时（Quartz 集群），任选健康 Agent

#### 5. scheduler-centric-ui
- UI 以 Agent 和 Scheduler 为导航核心
- 可查看全局 Scheduler 列表及其关联 Agent
- Job 操作面向 Scheduler 而非 Cluster

### Modified Capabilities

#### 1. agent-registration
- 移除 ClusterId 依赖
- 注册端点从 `/api/clusters/{clusterId}/agents` 变为 `/api/agents`
- 请求体增加可选 `agentId` 字段

#### 2. agent-heartbeat
- 从 Cluster 级别心跳迁移到 Agent 级别
- 心跳中携带 Scheduler 状态摘要

#### 3. job-operations
- 路由从 `/api/clusters/{clusterId}/jobs` 变为 `/api/schedulers/{schedulerName}/jobs`
- 平台根据 schedulerName 自动选择健康 Agent 转发

## Impact

### 破坏性变更

| 变更 | 影响 | 迁移策略 |
|------|------|----------|
| 删除 Cluster API | UI 和所有客户端 | 旧端点保留 301 重定向 |
| 注册端点变化 | Agent 配置 | Agent 升级后自动切换 |
| Job 路由变化 | UI 和 API 客户端 | 提供过渡期兼容端点 |
| 数据库表重构 | 迁移需保留历史数据 | 分阶段 Migration |

### 非破坏性变更

| 变更 | 说明 |
|------|------|
| ISchedulerAccessor 新增 | 宿主可选择性使用 |
| Scheduler 上报新增 | Agent 自动执行，不需宿主修改 |
| 身份持久化新增 | Agent 自动执行 |
| UI 新增页面 | 不影响旧页面访问 |
