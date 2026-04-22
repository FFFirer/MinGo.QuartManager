## Why

当前前端存在以下问题需要解决：

1. **缺少统一的 Dashboard** - 用户无法快速了解平台整体运行状况
2. **缺少日历视图** - 无法可视化查看作业调度计划
3. **侧边栏导航模式单一** - 所有操作都在同一层级，无法体现集群上下文
4. **操作体验不统一** - 创建集群是单步表单，创建作业是4步向导，缺乏一致性
5. **反馈机制缺失** - 操作成功/失败无 Toast 通知

## What Changes

### 新增页面
- **平台级 Dashboard** (`/`) - 聚合所有集群的健康状况、作业统计、待执行任务
- **集群级 Dashboard** (`/clusters/:clusterId`) - 单个集群的作业、Agent、待执行任务汇总
- **集群级 Calendar** (`/clusters/:clusterId/calendar`) - 可视化作业调度日历

### 侧边栏重构
- 采用 Portainer 风格：选中集群后，侧边栏切换为集群上下文模式
- 未选择集群：显示 Dashboard、Clusters、Settings
- 选择集群后：Clusters 列表 + 集群子菜单 (Dashboard/Jobs/Calendar/Agents)

### UI/UX 优化
- 新增统一 Toast 通知系统
- 统一创建资源流程（4步向导模式）
- 统一页面布局规范
- JobDetailPage 预留执行历史区域

### API 新增
- `GET /api/dashboard` - 平台级聚合数据
- `GET /api/clusters/:clusterId/dashboard` - 集群级聚合数据
- `GET /api/clusters/:clusterId/calendar` - 日历数据

## Capabilities

### New Capabilities
- **platform-dashboard**: 平台级 Dashboard，聚合所有集群的整体运行状况
- **cluster-dashboard**: 集群级 Dashboard，作为集群首页展示聚合信息
- **cluster-calendar**: 集群级日历视图，可视化作业调度计划
- **sidebar-navigation**: Portainer 风格侧边栏，支持集群上下文切换
- **toast-notification**: 统一 Toast 通知系统
- **unified-create-flow**: 统一创建资源流程（4步向导）

### Modified Capabilities
- (无，现有功能需求不变，仅优化 UI/UX)

## Impact

### 前端 (MinGo.Qap.UI)
- 新增 3 个页面：PlatformDashboard、ClusterDashboard、Calendar
- 重构侧边栏组件：Sidebar、SidebarItem、ClusterSelector
- 新增 Toast 组件
- 新增 Calendar 组件（基于 react-calendar）
- 新增 StatsCard、UpcomingJobsList 等 UI 组件
- 路由结构调整

### 后端 (MinGo.Qap.Platform)
- 新增 DashboardController
- 新增 `/api/dashboard` 端点
- 新增 `/api/clusters/:id/dashboard` 端点
- 新增 `/api/clusters/:id/calendar` 端点
- 修改 ClusterService：支持聚合查询

### 依赖
- 前端新增：`react-calendar`、`cron-parser`、`react-hot-toast`