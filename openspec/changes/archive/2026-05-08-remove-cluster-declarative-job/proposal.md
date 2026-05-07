## Why

移除已废弃的 Cluster 概念（实体、表、API、枚举），避免新旧概念混淆。同时将 Job 创建从"先调 Agent 再存"改为"声明式"——先在 Platform 侧记录创建意图（去重），再调 Agent 的幂等 Job 替换接口并回写结果，提升一致性和可追溯性。

## What Changes

- **BREAKING**: 删除 `Cluster.cs` 实体、`AgentInstance.cs` 实体、`Clusters` 表和 `AgentInstances` 表
- **BREAKING**: 删除 `OldClustersRedirectController`（/api/clusters/* 301 重定向端点）
- **BREAKING**: 删除 `ClusterStatus` 枚举、`DashboardDto.Clusters/TotalClusters`、`ClusterSummaryItem`/`ClusterDashboardDto` 等 DTO
- **BREAKING**: 删除 `DashboardController.GetClusterDashboard`/`GetClusterCalendar` 端点
- **BREAKING**: 删除 `AgentInstanceDto.ClusterId`、`AgentRegistrationResponse.ClusterId`、`IAgentRegistry.ClusterId` 字段
- **BREAKING**: DROP TABLE `Clusters` 和 `AgentInstances`（数据库迁移）
- **重构**: `JobDefinition.ClusterId` → `JobDefinition.SchedulerName`（正名）
- **重构**: `JobDefinition` 新增 `Group` 和 `ResultJson` 字段
- **重构**: `JobService.CreateAsync` 改为声明式流程（去重→存Pending→调Agent→回写）
- **重构**: Agent 侧 `CreateJobAsync` 内部改为幂等 `AddJob(jobDetail, replace:true)` + trigger 替换
- **重构**: Agent API 端点 `POST /api/agent/jobs` → `PUT /api/agent/jobs`（幂等语义）

## Capabilities

### New Capabilities
- `declarative-job-creation`: 声明式 Job 创建，先存 Platform 侧 Pending 记录，再调 Agent 幂等接口并回写结果

### Modified Capabilities
- `cluster-dashboard`: 移除 Cluster 相关仪表盘数据（已返回空数据，直接删除端点）
- `cluster-calendar`: 移除 Cluster 日历端点（已返回空数据，直接删除端点）
- `job-create-form`: 适配声明式创建流程的响应处理（409 去重冲突等）
- `unified-create-flow`: 移除 Cluster 引用

## Impact

- **Platform 后端**: 6 个实体/DTO 文件删除，~10 个文件修改，1 个新 Migration
- **Platform DB**: DROP Clusters 表、AgentInstances 表；JobDefinitions 改列名和索引
- **Agent 侧**: API 端点 method 变更（POST→PUT），内部 replace 语义；方法签名不变
- **Shared DTO**: Cluster 相关 DTO 清理
- **前端 UI**: PlatformDashboardPage 等移除 Cluster 展示（可能需单独梳理）
