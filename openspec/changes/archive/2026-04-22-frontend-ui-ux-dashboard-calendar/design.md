## Context

### 当前状态

**前端项目结构**:
- 现有页面：ClustersPage, ClusterDetailPage, JobsPage, JobDetailPage, AgentInstancesPage
- 路由：`/`, `/clusters/:id`, `/clusters/:id/jobs`, `/clusters/:id/jobs/:jobKey`, `/clusters/:id/agents`
- 侧边栏：仅包含静态导航链接，无动态集群选择
- 组件：CreateClusterModal (单步), CreateJobModal (4步), ConfirmDialog, DataTable, StatusBadge

**后端 API**:
- Cluster 相关：`/api/clusters`, `/api/clusters/:id`
- Job 相关：`/api/clusters/:id/jobs`, `/api/clusters/:id/jobs/:jobKey`
- Agent 相关：`/api/clusters/:id/agents`

**存在的问题**:
1. 用户进入首页 (`/`) 直接看到集群列表，无整体概览
2. 缺少日历视图，无法可视化调度计划
3. 侧边栏无法体现"当前操作在哪个集群上下文"
4. 创建资源流程不统一
5. 无统一通知机制

### 约束

- 技术栈：React + TypeScript + Tailwind CSS + React Router
- 后端：ASP.NET Core + EF Core + Quartz
- 目标用户：运维人员、调度管理员
- 桌面端优先，暂不考虑移动端

### 利益相关方

- 前端开发者：需要实现新页面和组件
- 后端开发者：需要新增 API
- 运维人员：需要更好的监控和调度可视化

## Goals / Non-Goals

### Goals

1. **平台级 Dashboard** - 聚合所有集群的整体运行状况，提供快速概览
2. **集群级 Dashboard** - 作为集群首页，展示该集群的聚合信息
3. **日历视图** - 可视化展示作业调度计划，支持月/周/列表视图
4. **侧边栏重构** - Portainer 风格，选择集群后切换到集群上下文
5. **统一操作体验** - Toast 通知、统一创建流程、页面布局规范

### Non-Goals

- 移动端适配
- 跨集群视图（每个集群独立管理）
- 执行历史功能（仅预留 UI，后端未来实现）
- 实时推送（使用轮询）

## Decisions

### D1: 两层 Dashboard 设计

**决策**: 平台级 Dashboard (`/`) 和集群级 Dashboard (`/clusters/:id`) 分离

**理由**:
- 平台级汇总全局信息，适合 SRE/运维负责人
- 集群级展示单集群详情，适合业务负责人
- 职责分离，API 独立，便于扩展

**备选方案**:
- 单层 Dashboard + 集群选择器 → 不符合 Portainer 模式

### D2: 侧边栏上下文切换模式

**决策**: 选择集群后，侧边栏自动切换为集群上下文，显示集群子菜单

**理由**:
- 与 Portainer 交互模式一致，用户熟悉
- 明确当前操作所属集群上下文
- 导航路径清晰

**实现方式**:
```typescript
// 侧边栏状态
interface SidebarState {
  selectedCluster: ClusterSummaryDto | null;
  expandedItems: string[];
  currentPath: string;
}
```

### D3: Calendar 组件选择

**决策**: 使用 `react-calendar` 而非 `react-big-calendar`

**理由**:
- 轻量级 (约 50KB vs 500KB+)
- 完全定制化，适合作业调度这种简单场景
- 无商业许可问题

**备选方案**:
- `react-big-calendar` → 功能全但体积大
- `FullCalendar` → 商业许可复杂
- 自研 → 重复造轮子

### D4: 前端计算 Calendar Fire Times

**决策**: 日历数据在前端使用 `cron-parser` 计算

**理由**:
- 减少后端计算压力
- 响应更快（无需每次切换月份都请求后端）
- 后端只需返回作业定义和 cron 表达式

**备选方案**:
- 后端计算所有 fire times → 数据量大，网络开销大

### D5: Toast 通知方案

**决策**: 使用 `react-hot-toast`

**理由**:
- 轻量、API 简洁
- 支持 Promise 自动处理 loading/success/error
- TypeScript 支持好

**备选方案**:
- 自研 → 需要处理动画、位置、关闭逻辑
- `sonner` → 新兴但生态较小

### D6: 创建流程统一为 4 步向导

**决策**: 将 CreateClusterModal 也改为 4 步向导模式

**理由**:
- 与 CreateJobModal 保持一致
- 步骤清晰：选择类型 → 配置参数 → 调度 → 确认
- 用户学习成本低

**步骤设计**:
1. **基础信息**: 名称、环境、Agent URL、描述
2. **高级配置**: (预留，未来扩展)
3. **确认**: 显示摘要信息
4. **提交**: 创建

### D7: Dashboard API 设计

**决策**: 后端提供聚合 API，前端直接消费

**理由**:
- 后端数据库查询更高效（可做缓存）
- 减少前端复杂度
- 便于未来扩展权限控制

**API 结构**:
```csharp
// 平台级
GET /api/dashboard
// 集群级
GET /api/clusters/{id}/dashboard
// 日历
GET /api/clusters/{id}/calendar?year=2024&month=10
```

## Risks / Trade-offs

### R1: 侧边栏状态持久化

**风险**: 用户刷新页面后，丢失选中的集群和展开状态

**缓解**: 使用 localStorage 持久化侧边栏状态，页面加载时恢复

```typescript
// 恢复逻辑
useEffect(() => {
  const saved = localStorage.getItem('sidebar-state');
  if (saved) {
    const state = JSON.parse(saved);
    setSelectedCluster(state.selectedCluster);
    setExpandedItems(state.expandedItems);
  }
}, []);
```

### R2: Calendar 性能

**风险**: 大量作业时，前端计算 fire times 可能卡顿

**缓解**:
- 仅计算当前视图范围的日期（当月/当周）
- 使用 useMemo 缓存计算结果
- 虚拟滚动（未来优化）

### R3: Dashboard 数据获取

**风险**: 多个并行请求可能导致页面加载慢

**缓解**:
- 使用 React Query 的 staleTime 缓存
- 骨架屏过渡
- 增量加载（先显示框架，后加载数据）

### R4: 后端 API 变更

**风险**: 需要后端配合新增 API，可能有开发排期

**缓解**:
- 前端可先用 Mock 数据开发
- API 设计先行，与后端对齐接口

### R5: 现有代码兼容性

**风险**: 侧边栏重构可能影响现有页面导航

**缓解**:
- 渐进式重构，先新增页面再改造侧边栏
- 保持现有路由兼容
- 充分测试

## Migration Plan

### Phase 1: 基础设施
1. 新增 Toast 组件
2. 新增 StatsCard、UpcomingJobsList 等 UI 组件
3. 新增 Dashboard API (后端)

### Phase 2: 平台级功能
1. 实现 PlatformDashboardPage
2. 配置路由 `/`
3. 侧边栏增加 Dashboard 入口

### Phase 3: 集群级功能
1. 实现 ClusterDashboardPage
2. 实现 CalendarPage
3. 侧边栏重构：集群上下文切换
4. 路由调整

### Phase 4: 优化
1. 统一创建流程
2. 执行历史预留 UI
3. 细节打磨

### 回滚策略
- 前端：使用 Git 分支管理，回滚代码即可
- 后端：API 保持向后兼容，旧前端仍可工作

## Open Questions

1. **Q1**: Dashboard 刷新频率？建议 30 秒轮询，还是用户手动刷新？
2. **Q2**: Calendar 是否需要支持手动触发作业的显示？（当前只显示定时作业）
3. **Q3**: 是否需要权限控制？不同用户看到不同的集群？
4. **Q4**: Create Cluster 向导的第二步放什么？当前是空的

这些问题的答案将影响后续实现细节。