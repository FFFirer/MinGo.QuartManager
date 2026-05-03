## Context

当前 UI 是 v1 Cluster 时代遗留的单体结构：
- `App.tsx` 包含 Sidebar 全部逻辑（~250行），与路由混合在同一文件
- `PlatformDashboardPage.tsx` 通过 `fetch('/api/dashboard')` 获取数据，返回类型仍是 `{ totalClusters, clusters: [...] }`，而 v2 已移除 Cluster
- `JobsPage.tsx` 使用 `DataTable` 但无批量操作、无搜索筛选、分页只有 Prev/Next
- `CreateJobModal` 是 Modal 弹出，完全遮挡上下文，大屏利用率低
- 无全局搜索、无实时推送、无响应式适配

本次设计以 **渐进式增强** 为原则，分 P0-P3 四个阶段增量交付，不阻塞发布。

## Goals / Non-Goals

**Goals:**
- 将 Sidebar 抽为独立组件，支持折叠/展开
- Dashboard 数据层完全迁移到 v2 API (Agent/Scheduler)，删除所有 Cluster 引用
- Job 列表页支持行内快速操作和批量选择/操作
- CreateJob 从 Modal 改为侧滑面板
- 新增全局搜索、实时事件流、快速操作面板
- 统一使用 PageHeader 面包屑
- 所有变更向后兼容，不破坏现有功能

**Non-Goals:**
- 不修改后端 API（仅调整前端调用方式）
- 不新增第三方 UI 库（保持 Tailwind + lucide-react）
- 不做完整设计系统 (Design System) 重构——仅组件化
- 不涉及 RBAC/权限系统

## Decisions

### 1. 布局架构: 四区布局替代单一 flex

**决策**: 从 `div.flex > Sidebar + main` 改为 `Sidebar + Header(面包屑+搜索) + Content + StatusBar`

**理由**:
- 面包屑从路由自动推导，统一所有页面使用
- 全局搜索栏固定在 Header 区而非 Sidebar，避免折叠后不可见
- 底部状态栏显示系统健康信息，不占用主内容区
- 四区布局使每个区域职责单一，便于维护

```
┌─────────┬────────────────────────────────────────┐
│ SIDEBAR │  HEADER (Breadcrumb + GlobalSearch)     │
│ (折叠)  ├────────────────────────────────────────┤
│         │  CONTENT (Routes)                       │
│         │                                         │
├─────────┴────────────────────────────────────────┤
│  STATUSBAR (系统状态 + 版本 + 刷新时间)            │
└──────────────────────────────────────────────────┘
```

### 2. Sidebar 折叠: CSS transition + Context

**决策**: Sidebar 折叠通过 `w-64` ↔ `w-16` + CSS transition 实现，折叠状态通过 React Context 全局共享

**理由**:
- CSS transition 零运行时开销，GPU 加速
- Context 让任意子组件感知折叠态（如面包屑自动调整）
- 折叠时只显示图标，hover 展开 tooltip
- `< lg` 断点自动折叠，并添加 Hamburger 按钮

### 3. Dashboard 数据聚合: React Query 组合查询

**决策**: Dashboard 不再依赖单一 `/api/dashboard` 端点，而是通过 `useQueries` 并行请求多个 API 聚合

```typescript
// 现状: 一个 fetch 返回完整数据（但数据模型已过时）
const { data } = useQuery({ queryKey: ['platform-dashboard'], queryFn: fetchDashboard })

// 改进: 多个 query 组合
const schedulers = useQuery({ queryKey: ['schedulers'], queryFn: () => schedulerApi.getAll() })
const agents = useQuery({ queryKey: ['agents'], queryFn: () => agentApi.getAll() })
const jobs = useQuery({ queryKey: ['dashboard-upcoming-jobs'], queryFn: () => /* 从各 scheduler 聚合 */ })

// 组合派生数据
const dashboardData = useMemo(() => ({
  totalSchedulers: schedulers.data?.data?.length ?? 0,
  totalJobs: /* 聚合 */,
  agentHealth: /* 统计 agents 状态 */,
  ...
}), [schedulers.data, agents.data])
```

**理由**:
- 无需创建新后端端点，复用现有 API
- React Query 自动缓存和去重（列表页已请求相同数据）
- 派生数据用 `useMemo` 计算，无额外渲染

### 4. 实时推送: SSE (Server-Sent Events) 替代轮询

**决策**: 新增 `useEventStream` hook 连接 Platform SSE 端点，实时接收 Agent/Scheduler 状态变更

**理由**:
- SSE 比 WebSocket 更简单（单向，自动重连）
- 30s 轮询改为 SSE 推送，减少 95% 网络请求
- 后端只需新增一个 SSE Controller 端点（EventStreamController）
- 浏览器原生 EventSource 支持，无需额外依赖

```typescript
// 新增自定义 hook
function useEventStream(onEvent: (event: StreamEvent) => void) {
  useEffect(() => {
    const es = new EventSource('/api/events');
    es.onmessage = (e) => onEvent(JSON.parse(e.data));
    es.onerror = () => { /* 自动重连 */ };
    return () => es.close();
  }, []);
}
```

### 5. 侧滑面板: 推入式 (Push) 而非覆盖式 (Overlay)

**决策**: CreateJob 改用从右侧滑入的面板，主内容区向左推入而非覆盖

**理由**:
- 用户可在创建 Job 时参考列表内容
- 大屏上更充分利用水平空间
- 动画自然: `transform: translateX(0%)` ↔ `translateX(100%)`
- 移动端自动切换为全屏滑入

```
┌─────────────────────────┬──────────────┐
│  Job 列表 (宽度压缩)      │  创建作业面板  │
│                         │              │
│  ☐ daily-sync  🟢      │  Step 1:     │
│  ☐ weekly-rpt  🟡      │  Job Key:    │
│  ☐ hourly-chk  🟢      │  Type:       │
│                         │  ...         │
└─────────────────────────┴──────────────┘
    原有内容推入左侧         新增右侧面板
```

### 6. 全局搜索: 模态面板 + Fuse.js 模糊搜索

**决策**: Ctrl+K 触发搜索模态，使用 Fuse.js (轻量) 前端模糊搜索已加载的数据

**理由**:
- 无需新增后端搜索 API
- 数据已在 React Query 缓存中，搜索零网络开销
- Fuse.js 约 5KB gzip，支持模糊/排序/阈值
- 搜索结果按类型分组：Agents / Schedulers / Jobs

### 7. 批量操作: 受控选择 + 并行 mutation

**决策**: 使用 `Set<string>` 管理选中 key，批量操作通过 `Promise.allSettled` 并行执行 mutation

```typescript
const [selectedKeys, setSelectedKeys] = useState<Set<string>>(new Set());
const batchTrigger = useMutation({
  mutationFn: async (keys: string[]) => {
    const results = await Promise.allSettled(
      keys.map(key => jobApi.trigger(schedulerName, key))
    );
    const failed = results.filter(r => r.status === 'rejected');
    if (failed.length > 0) throw new Error(`${failed.length} jobs failed`);
  },
  onSuccess: () => { toast.success(`Triggered ${selectedKeys.size} jobs`); queryClient.invalidateQueries(...) }
});
```

## Risks / Trade-offs

| 风险 | 缓解措施 |
|------|---------|
| Dashboard 聚合查询性能：N 个 Scheduler 各需请求一次 | React Query `staleTime` 设为 30s，列表页缓存复用 |
| SSE 后端端点未实现 | Phase 1 保留 30s 轮询作为 fallback，SSE 为渐进增强 |
| Sidebar 折叠后图标含义不清 | 加 tooltip + aria-label，hover 展开显示文字 |
| 批量操作部分失败 | 使用 `Promise.allSettled` 收集失败项，toast 汇总展示 |
| 侧滑面板在内容超长时滚动混乱 | 面板自身 `overflow-y-auto`，Header/Footer 固定 |
| 与现有页面风格不一致 | 所有新组件复用现有的 StatusBadge/DataTable/PageHeader |

## Migration Plan

1. **P0 (核心功能不改体验)**: Sidebar 抽取、Dashboard 数据迁移、行内操作
   - 提取 Sidebar 为独立组件，接入四区布局
   - 修改 `PlatformDashboardPage.tsx` 数据层
   - `JobsPage.tsx` 每行新增操作按钮
   - 每条变更独立验证 LSP diagnostics

2. **P1 (新体验)**: 全局搜索、Dashboard 可视化、侧滑面板
   - 新增 GlobalSearch、健康矩阵、趋势图
   - CreateJobModal → SlidePanel

3. **P2 (批量/实时)**: 批量操作、SSE 推送、FAB
   - 复选框 + 批量操作栏
   - SSE hook + 事件流组件
   - 浮动操作面板

4. **P3 (收尾)**: 状态栏、面包屑统一
   - StatusBar 组件
   - 所有页面统一使用 PageHeader 面包屑

## Open Questions

- SSE 端点路径？决定: `/api/events`，事件格式 `{ type: 'heartbeat' | 'scheduler_update' | 'job_event', payload: {} }`
- 搜索面板的快捷键冲突处理？决定: 仅在未聚焦 input 时生效
- 侧滑面板宽度？决定: `w-96` (384px) 在小屏全屏
