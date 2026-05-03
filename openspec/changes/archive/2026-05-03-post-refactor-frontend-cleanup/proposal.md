## Why

2026-05-02 的 v2.0.0 架构重构（agent-scheduler platform refactor）已将后端和前端核心路由从 Cluster 模型迁移到 Agent+Scheduler 模型，但部分前端文件在重构中被遗漏，仍残留 Cluster 概念引用，导致编译风险（缺失组件引用）、API 端点错配、以及类型不一致。同时，openspec/specs/ 下的前端规格书仍描述旧的 Cluster 模型，与实际代码和 v2.0.0 架构方向脱节。

本 change 的目标是**收尾清理**，而非新一轮架构变更。

## What Changes

1. **修复残留的 Cluster 引用** — CalendarPage、CreateJobModal、PlatformDashboardPage、UpcomingJobsList 中仍使用 clusterId/Cluster API，改为 schedulerName
2. **修复 CalendarPage 对已删除 ClusterTabs 的 dangling import** — 该组件已在重构中被删除
3. **修复 StatusBadge 颜色映射** — blocked 状态应为红色（bg-red-500），当前误标为 slate
4. **更新前端 specs** — platform-dashboard、sidebar-navigation、cluster-dashboard、cluster-tabs、unified-create-flow、cluster-calendar 从 Cluster 模型更新为 Scheduler 模型描述

## Capabilities

### New Capabilities
- `<none>`: 本 change 不引入新能力，只清理和修复

### Modified Capabilities
- `platform-dashboard`: 更新数据模型和路由描述从 Cluster→Scheduler
- `sidebar-navigation`: 更新导航项从 Clusters→Agents+Schedulers；移除集群上下文模式描述
- `cluster-dashboard`: 更新为 scheduler-dashboard，替换 cluster 概念
- `cluster-tabs`: 重命名为 scheduler-tabs，替换 cluster 概念
- `unified-create-flow`: 更新创建流程的上下文（Scheduler 而非 Cluster）
- `cluster-calendar`: 更新为 scheduler-calendar，替换 cluster 概念

## Impact

- **前端代码**: CalendarPage.tsx、CreateJobModal.tsx、PlatformDashboardPage.tsx、UpcomingJobsList.tsx、StatusBadge.tsx
- **前端规格**: openspec/specs/ 下 6 个 spec 目录
- **不涉及**后端 API 变更——后端已经是 Scheduler 模型
