## Context

2026-05-02 的 agent-scheduler platform refactor（b1d2e34）完成了 Cluster→Scheduler 的核心架构迁移：

- **已删除**: ClusterTabs, CreateClusterModal, useClusters, ClustersPage, ClusterDashboardPage, ClusterDetailPage, AgentInstancesPage
- **已新增**: AgentsPage, AgentDetailPage, SchedulersPage, SchedulerDetailPage
- **已更新路由**: App.tsx sidebar 改为 Agents+Schedulers, jobApi 使用 schedulerName

但以下文件在前端重构中被遗漏，仍残留 Cluster 概念：

| 文件 | 残留引用 |
|------|---------|
| CalendarPage.tsx | 使用 clusterId、调用 /api/clusters/{id}/calendar、导入已删除的 ClusterTabs |
| CreateJobModal.tsx | props 声明 clusterId，但调用方已传 schedulerName |
| PlatformDashboardPage.tsx | fetch /api/dashboard 返回 cluster 模型、链接到 /clusters/{id} |
| UpcomingJobsList.tsx | 接口含 clusterId/clusterName 字段 |
| StatusBadge.tsx | blocked→bg-slate-500（应为 bg-red-500） |

同时 openspec/specs/ 下 6 个前端规格文件仍描述旧 Cluster 模型。

**约束**: 本 change 不涉及后端变更，不改动已正确的 Agent/Scheduler 页面。

## Goals / Non-Goals

**Goals:**
- 消除 CalendarPage、CreateJobModal、PlatformDashboardPage 中的 Cluster 残留引用
- 修复 StatusBadge 颜色映射错误
- 更新 UpcomingJobsList 数据模型为 scheduler 友好
- 更新 openspec/specs/ 下 6 个前端规格文件以反映 v2.0.0 架构

**Non-Goals:**
- 不重新引入 Cluster 概念
- 不改动已正确实现的 AgentsPage、AgentDetailPage、SchedulersPage、SchedulerDetailPage
- 不修改后端 API
- 不新增页面或功能

## Decisions

### 1. CalendarPage: 迁移到 schedulerName

- **方案**: 将 `clusterId` 替换为 `schedulerName`，API 调用改为 `/api/schedulers/{name}/calendar`
- **理由**: 后端已移除 Cluster 路由，DashboardController 中可能有 calendar 相关端点，且 Scheduler 模型已是标准
- **替代方案**: 删除 CalendarPage — 否决，calendar 功能本身有价值
- **ClusterTabs**: 删除 import，CalendarPage 自行渲染 header 信息（schedulerName + status badge）

### 2. CreateJobModal: props 对齐

- **方案**: props 接口从 `clusterId: string` 改为 `schedulerName: string`
- **理由**: 调用方 JobsPage 已传 schedulerName，类型不匹配会产生编译警告/错误
- **内部逻辑**: useCluster/useCreateJob/useManifest hooks 已在重构中被删除，需要改用直接 API 调用或创建新的 scheduler-based hooks

### 3. PlatformDashboardPage: 清理 cluster 引用

- **方案**: 
  - `/api/dashboard` 仍然使用（由 DashboardController 提供），但移除内部 `clusters` 数组中的数据模型中 cluster 字段命名
  - 连接改指向 `/schedulers/{name}` 而非 `/clusters/{id}`
  - 保留 dashboard 概览功能（total schedulers, total jobs, agent health）
- **理由**: DashboardController 是后端仍在维护的端点，只需要前端数据模型对齐

### 4. StatusBadge: 修正颜色映射

- **方案**: `case 'blocked': return 'bg-slate-500'` → `'bg-red-500'`
- **理由**: 规格要求 blocked = 红色，Offline = slate

### 5. 规格文件更新策略

- **platform-dashboard** 等 6 个规格文件: 直接在 openspec/specs/ 下创建 delta diff（MODIFIED/REMOVED）
- 不移动目录（保留原名），只更新内容以反映 Scheduler 模型

## Risks / Trade-offs

- **CalendarPage API 端点可能不存在**: `/api/schedulers/{name}/calendar` 后端可能未实现。需要确认 DashboardController 是否支持，若不支持则需要先补充后端端点或使用 calendar 数据模拟。
- **PlatformDashboardPage 数据模型**: DashboardController 返回的数据结构可能仍是 cluster 模型，需要检查其 DTO 是否已更新为 scheduler。
- **UpcomingJobsList 被多处使用**: 如果修改其接口，需确保所有调用方同步更新。
