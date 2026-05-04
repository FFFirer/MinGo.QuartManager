## Context

前端 React + TypeScript + Tailwind CSS，后端 ASP.NET Core + EF Core + PostgreSQL。App.tsx 的 `AppLayout` 已提供 `min-h-screen bg-slate-900 flex flex-col` 等全局样式，页面内容在 `<main className="flex-1 overflow-auto">` 中渲染。

当前 8 个页面中 6 个已遵循统一模板（`div className="p-6"` + `PageHeader` 开头），但 AgentsPage 和 AgentDetailPage 的 wrapper 与其他页面不一致。表格渲染方式也有 3 种变体：DataTable 组件（JobsPage）、原生 `<table>`（SchedulersPage、DetailPages）、自定义 CSS grid（AgentsPage）。

Agents API (`GET /api/agents`) 当前无分页支持，返回全部数据。

## Goals / Non-Goals

**Goals:**
- 所有页面的外层 wrapper 统一为 `div className="p-6"`
- AgentDetailPage 正确使用 PageHeader（删除手动 Back 链接和重复 h1）
- 所有数据列表统一使用 DataTable 组件
- AgentsPage 增加分页功能
- 保持现有页面头部规范：列表页无面包屑，详情页有面包屑

**Non-Goals:**
- 不改变 DataTable 组件的底层渲染方式（flexbox 布局）
- 不重构 StatusBar、Sidebar、FloatingActionPalette 等全局组件
- 不改变 Dashboard/Calendar/JobDetail 页面（他们已经统一）
- 不处理 SchedulersPage 的分页（按决策保持现状）

## Decisions

### Decision 1: 分 PR 实施

方案：分 3 个 PR 逐步推进

| PR | 内容 | 文件数 | 风险 |
|----|------|--------|------|
| PR #1 | AgentsPage + AgentDetailPage wrapper 修复 | 2 个前端文件 | 低 — 纯 CSS 变化 |
| PR #2 | 后端 Agent 分页 + 前端 API 层 | 3-4 个文件 | 中 — 涉及 API 变更 |
| PR #3 | DataTable 统一 + 前端分页 UI | 5-6 个文件 | 中 — 涉及组件迁移 |

**Rationale**: PR #1 是纯前端 UI 修复，可独立验证；PR #2 是后端基础设施，独立部署；PR #3 依赖 PR#2 的分页接口。拆分后每个 PR 可独立 review 和发布。

### Decision 2: 分页响应格式

方案：复用现有 `PagedResponse<T>` 类型（已在 types/index.ts 中定义）

```typescript
interface PagedResponse<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
```

**Rationale**: 与 Jobs API 的模式一致。后端已有一个 `PagedResponse` 在 Shared 层，前端有对应的 TS 类型。

### Decision 3: AgentsPage 保留自定义 grid → DataTable 放到 PR#3

PR #1 只修 wrapper，不改表格。PR #3 才做 DataTable 迁移 + 分页。这样每个 PR 的改动聚焦，便于 review。

### Decision 4: DataTable 组件类型增强

当前 DataTable 的 `Column.header` 定义为 `string`，但 JobsPage 用 `as any` 塞了 checkbox ReactNode。需要将 `header` 改为 `string | ReactNode` 类型。

**Rationale**: 不改类型就无法移除 JobsPage 的 `as any`，而且未来其他页面也可能需要自定义表头。

## Risks / Trade-offs

| Risk | Mitigation |
|------|-----------|
| [回归] AgentsPage 布局修改后样式异常 | 对比前后截图；wrapper 变更只影响外间距，不影响内部表格布局 |
| [回归] DataTable 类型变更影响 JobsPage | JobsPage 用 DataTable，需要在更改类型后验证 checkbox 列仍正常工作 |
| [兼容] 后端分页是后向兼容的？ | 分页参数有默认值（page=1, pageSize=20），不传参时行为应与原来类似（返回第一页） |
| [数据] 分页后总数不准确（因 status 实时变化） | Agent 数据量预计不大，允许近似值。前端用 `Approx. N total` 而非 `Showing X-Y of N` |
