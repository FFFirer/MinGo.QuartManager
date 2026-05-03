## Why

当前 MinGo.Qap.UI 基于 v1 Cluster 架构开发，经历了 v2 Agent-Scheduler 架构重构后，UI 层未同步升级。Dashboard 仍使用旧的 Cluster 数据模型、导航无响应式适配、作业管理缺少批量操作和快速交互入口，整体体验落后于后端架构演进。

本次 Phase 1 从三个核心方向同步推进：导航与布局重构、Dashboard V2 适配升级、作业管理体验增强，使前端与 v2 架构对齐并显著提升日常运维效率。

## What Changes

### 导航与布局重构
- Sidebar 从 App.tsx 抽为独立组件，支持折叠/展开 (`w-64` ↔ `w-16`)
- 新增全局搜索栏 (Ctrl+K / ⌘K)，跨 Agent/Scheduler/Job 搜索
- 统一面包屑导航，从路由自动推导
- 新增底部状态栏 (系统健康度、连接状态、版本)
- `< lg` 断点响应式适配 (自动折叠 Sidebar)

### Dashboard V2 大改
- **BREAKING**: 替换 `fetch('/api/dashboard')` 调用，改用 `GET /api/schedulers` + `GET /api/agents` 聚合数据
- 移除所有 Cluster 引用 (clusterId → schedulerName, clusterStatus → agentStatus)
- 新增 Scheduler 健康矩阵 (卡片网格展示所有 Scheduler 状态)
- 新增 Job 执行趋势图 (过去 24h 执行频率)
- 新增实时事件流 (Activity Feed，SSE 推送)
- 指标卡支持实时数据聚合

### 作业管理体验提升
- 列表页每行新增行内快速操作按钮 (Trigger/Pause/Resume/Delete)
- 新增批量选择 (checkbox) + 批量操作栏 (批量触发/暂停/恢复/删除)
- CreateJob 从 Modal 改为侧滑面板 (保留上下文)
- 新增从模板创建 / 从已有 Job 复制功能
- 分页组件升级 (页码选择 + 每页条数)
- 新增快速操作浮动面板 (FAB)

## Capabilities

### New Capabilities
- `global-search`: Ctrl+K 全局搜索面板，跨 Agent/Scheduler/Job 搜索
- `responsive-layout`: 响应式布局系统 (可折叠 Sidebar、移动端适配)
- `job-batch-operations`: Job 列表批量选择与批量操作
- `floating-action-palette`: 浮动快速操作面板
- `job-templates`: Job 模板创建与从已有 Job 复制
- `activity-feed`: Dashboard 实时事件流

### Modified Capabilities
- `platform-dashboard`: 数据模型从 Cluster 迁移到 Scheduler/Agent v2 API，新增健康矩阵、趋势图、事件流
- `sidebar-navigation`: 响应式可折叠、全局搜索入口、底部状态栏
- `unified-create-flow`: CreateJob 从 Modal 改为侧滑面板，增加模板功能
- `toast-notification`: 补充批量操作反馈、SSE 推送通知场景

## Impact

- **src/MinGo.Qap.UI/**: 所有页面和组件文件需调整
  - `App.tsx`: Sidebar 抽出、新增布局结构、路由调整
  - `PlatformDashboardPage.tsx`: 完全重写数据层和可视化层
  - `JobsPage.tsx`: 新增批量操作、行内操作、分页升级
  - `JobDetailPage.tsx`: 小幅调整
  - `CreateJobModal.tsx`: 改造为侧滑面板组件
  - 新增 `Sidebar.tsx`, `GlobalSearch.tsx`, `StatusBar.tsx`, `FloatingActionPalette.tsx` 等组件
- **依赖无变更**: React 19, Tailwind, @tanstack/react-query, lucide-react 等保持不变
- **后端无变更**: 仅调整前端 API 调用方式，后端无需新增端点
