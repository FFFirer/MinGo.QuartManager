## 1. 导航与布局重构 (P0)

- [ ] 1.1 提取 Sidebar 为独立组件 `src/components/Sidebar.tsx`，包含所有现有逻辑（最近 Agent 下拉、快捷键、active 高亮）
- [ ] 1.2 创建 `LayoutContext` (折叠状态、sidebar 展开/折叠切换)，从 App.tsx 中分离布局状态
- [ ] 1.3 创建 `Layout.tsx` 四区布局组件 (Sidebar + HeaderBar + Content + StatusBar)
- [ ] 1.4 重构 `App.tsx`：移除内联 Sidebar 代码，改用 Layout 组件，保留 Routes
- [ ] 1.5 实现 Sidebar 折叠展开动画 (w-64 ↔ w-16 CSS transition + tooltip)
- [ ] 1.6 实现响应式自动折叠 (`< lg` 断点 + hamburger overlay)
- [ ] 1.7 创建 `StatusBar.tsx` 底部状态栏组件 (系统健康度、刷新时间、版本)
- [ ] 1.8 验证 LSP diagnostics 无错误

## 2. Dashboard V2 数据迁移 (P0)

- [ ] 2.1 重写 `PlatformDashboardPage.tsx` 数据层：移除 `fetch('/api/dashboard')`，改用 `useQueries` 聚合 `schedulerApi.getAll()` + `agentApi.getAll()`
- [ ] 2.2 移除所有 Cluster 引用 (totalClusters → totalSchedulers, clusters → schedulers 等)
- [ ] 2.3 重写 Dashboard 概览指标卡：使用 React Query 派生数据 (totalSchedulers, totalJobs, agentHealth)
- [ ] 2.4 新增 Scheduler 健康矩阵组件：卡片网格展示所有 Scheduler 状态、Job 数、Agent 数
- [ ] 2.5 新增 Job 执行趋势图组件：过去 24h 执行频率柱状图
- [ ] 2.6 更新 UpcomingJobsList：从各 Scheduler 的 Job 列表聚合数据
- [ ] 2.7 验证 Dashboard 在无数据时显示空状态，错误时显示重试

## 3. 作业管理体验提升 (P0)

- [ ] 3.1 JobsPage 每行新增行内快速操作按钮 (Trigger/Pause/Resume/Delete with stopPropagation)
- [ ] 3.2 新增 ContextMenu 组件或确认弹窗联动 Delete 操作
- [ ] 3.3 更新 JobsPage 分页组件：页码按钮 + 每页条数选择器 (10/20/50/100)
- [ ] 3.4 验证 Job 操作前后缓存失效和数据刷新

## 4. 全局搜索 (P1)

- [ ] 4.1 安装 Fuse.js 依赖
- [ ] 4.2 创建 `GlobalSearch.tsx` 组件：Ctrl+K 激活的模态搜索面板
- [ ] 4.3 实现跨 Agent/Scheduler/Job 模糊搜索（从 React Query 缓存读取数据）
- [ ] 4.4 搜索结果按类型分组展示，支持键盘上下导航和 Enter 打开
- [ ] 4.5 集成到 Layout HeaderBar 区域
- [ ] 4.6 验证快捷键冲突处理（input 聚焦时不触发）

## 5. Dashboard 可视化增强 (P1)

- [ ] 5.1 创建 `HealthMatrix.tsx` 健康矩阵组件 (Scheduler 状态卡片网格)
- [ ] 5.2 创建 `ExecutionTrendChart.tsx` 执行趋势图组件 (纯 CSS 柱状图，无第三方图表库)
- [ ] 5.3 更新 Dashboard 作业状态分布可视化 (使用 jobCounts 数据)
- [ ] 5.4 页面加载骨架屏适配新组件布局

## 6. 侧滑面板 (P1)

- [ ] 6.1 创建 `SlidePanel.tsx` 通用侧滑面板组件 (从右侧滑入、推入式布局、ESC关闭)
- [ ] 6.2 将 `CreateJobModal.tsx` 改造为使用 SlidePanel 的 `CreateJobPanel.tsx`
- [ ] 6.3 保留 4 步骤向导逻辑，面板 header 显示进度，footer 固定按钮
- [ ] 6.4 新增模板选择器（从 manifest 加载模板 + "Blank" 默认选项）
- [ ] 6.5 新增"从已有 Job 复制"功能：下拉选择源 Job 预填所有字段（jobKey 除外）

## 7. 批量操作 (P2)

- [ ] 7.1 JobsPage 每行新增 checkbox 列 + header select-all checkbox
- [ ] 7.2 创建 `BatchActionBar.tsx` 组件：选中时浮动显示计数 + Trigger/Pause/Resume/Delete 按钮
- [ ] 7.3 实现批量 Trigger/Pause/Resume/Delete 操作逻辑 (Promise.allSettled)
- [ ] 7.4 批量操作 toast 反馈（成功/部分成功/全部失败）

## 8. 实时事件流 (P2)

- [ ] 8.1 创建 `useEventStream` hook (SSE + 30s polling fallback)
- [ ] 8.2 创建 `ActivityFeed.tsx` 组件：实时事件列表，图标颜色编码
- [ ] 8.3 集成到 Dashboard (L4: 实时事件流区域)
- [ ] 8.4 实现 auto-scroll 和 pause 逻辑

## 9. 浮动操作面板 (P2)

- [ ] 9.1 创建 `FloatingActionPalette.tsx` 组件 (固定在右下角的 FAB)
- [ ] 9.2 实现展开/折叠动画，菜单项：Create Job、最近操作历史
- [ ] 9.3 集成 Context（感知当前页面上下文，如当前 SchedulerName）

## 10. 收尾 (P3)

- [ ] 10.1 所有页面统一使用 PageHeader 面包屑（从路由自动推导）
- [ ] 10.2 页面标题和面包屑一致性检查
- [ ] 10.3 全局 `npm run build` 验证编译通过
- [ ] 10.4 清理废弃代码：旧的 App.css 中 Vite 模板样式、未使用的 LayoutWrapper
