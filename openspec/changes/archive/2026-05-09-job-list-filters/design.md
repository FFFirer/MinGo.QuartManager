## Context

JobsPage 目前展示一个分页表格，但缺少按 Group 或 Name 筛选的功能。后端 API `GET /api/schedulers/{schedulerName}/jobs` 已支持 `group` 和 `keyword` 查询参数，前端 `jobApi.getAll()` 也已包含对应的函数签名。本次设计仅涉及前端 UI 层，后端无需改动。

当前技术栈：React 19 + TypeScript + Vite + TanStack Query + Tailwind CSS。

## Goals / Non-Goals

**Goals:**
- 在 JobsPage 表头区域添加 Group 文本输入框和 Name 文本输入框
- Name 输入添加 500ms 防抖
- 筛选条件变更时自动重置到第 1 页
- 筛选状态通过 URL query params 持久化，支持分享和浏览器回退

**Non-Goals:**
- 不添加下拉多选或高级筛选
- 不改动后端 API
- 不改动其他页面

## Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| 防抖策略 | `useMemo` + `setTimeout` 自定义 hook | 轻量，无需额外依赖。`useDebounce` 自定义 hook 替代 lodash.debounce |
| 筛选状态持久化 | URL query params (`?group=xxx&name=xxx`) | 支持分享链接、浏览器前进/后退，与 React Router 结合良好 |
| Group 输入类型 | 自由文本输入框 | 后端支持任意文本匹配，用户可输入完整组名或部分匹配 |
| 筛选器位置 | PageHeader actions 区域左侧 | 与 Refresh/Create 按钮同一行，紧凑布局 |

## Risks / Trade-offs

- [Risk] 频繁输入触发 API 请求 → [Mitigation] 使用 500ms 防抖，仅在用户停止输入后发起请求
- [Risk] URL query params 与组件内部状态不同步 → [Mitigation] 使用 `useSearchParams` 作为唯一数据源
