## Context

**Current State:**
- 自定义 `DataTable.tsx` (128 行) 使用 flex-wrap 模拟表格布局，支持: columns 定义、accessor (key/function)、format、行点击、loading/empty 状态
- 分页控件在 AgentsPage (server 分页) 和 JobsPage (client 分页) 重复实现 (~50 行/页)
- AgentDetailPage / SchedulerDetailPage 使用原生 `<table>` 渲染子表
- JobsPage 手动实现了 checkbox 多选和 batch actions
- 现有 UI 基于 Tailwind CSS 暗色主题 (slate-900/800/700 色系)

**Tech Stack:**
- React 19 + TypeScript ~6.0 + Vite
- Tailwind CSS 3
- TanStack Query v5
- React Router v7

## Goals / Non-Goals

**Goals:**
- 将 `DataTable.tsx` 内部实现替换为 `@tanstack/react-table` v8 的 headless hooks
- 对外保持 props 兼容，新增排序能力
- 提取 `<PaginationBar>` 公共组件，消除 2 页重复分页代码
- AgentDetailPage / SchedulerDetailPage 的子表迁移到 DataTable
- 完善泛型类型签名 (columns 不再使用 `any[]`)
- 支持列头点击排序 (client-side)

**Non-Goals:**
- 不引入列筛选 (filtering) — 后续可基于 TanStack Table 扩展
- 不引入列宽拖动 (column resizing)
- 不引入行分组/树形数据
- 不引入虚拟化 (当前数据量不需要)
- 不改变现有页面布局和视觉风格
- 不涉及 CalendarPage (无表格数据)

## Decisions

### Decision 1: TanStack Table v8 替代 flex-wrap 模拟表格

**方案**: 使用 `@tanstack/react-table` 的 `useReactTable` + `getCoreRowModel` + `getSortedRowModel`

**替代方案考虑**:
- **AG Grid Community**: 功能更全但包更大 (~100KB gzip)，与 Tailwind 定制成本高
- **手写排序**: 自己在 DataTable 上实现 sort state，但重新发明轮子且功能有限
- **保留现状**: 无排序，重复代码持续积累

**理由**: TanStack Table 与 TanStack Query 同源，headless 模式完美适配 Tailwind 自定义样式。~14KB 包体积，只引入需要的功能模块 (core + sorting)。

### Decision 2: 提取 `<PaginationBar>` 组件

**方案**: 将 AgentsPage 和 JobsPage 中重复的分页 UI 抽象为独立组件

```tsx
interface PaginationBarProps {
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
}
```

**替代方案考虑**:
- **合并到 DataTable**: 把分页逻辑内置到 DataTable 中会耦合 datasource 类型 (server vs client 分页)
- **用第三方分页组件**: 增加额外依赖

**理由**: 分页 UI 逻辑完全独立于表格渲染，独立组件更清晰。页面控制 server/client 分页策略。

### Decision 3: 内部用 `<table>` 替代 flex-wrap

**方案**: TanStack Table 提供行/列/表头数据模型，渲染层使用原生 `<table>`/`<thead>`/`<tbody>`/`<tr>`/`<th>`/`<td>`

**理由**: 原生 `<table>` 在列对齐、跨列宽一致性、无障碍方面天然优于 flex-wrap。Tailwind 对 `<table>` 支持良好。

### Decision 4: Sorting 模式 — client-side

**方案**: 使用 TanStack Table 的 `getSortedRowModel` 实现 client-side 排序。AgentsPage 的 server 分页场景暂不涉及 server 排序。

**理由**: 当前所有页面数据量不大 (schedulers 无分页、agents server 分页但单页~20条，client 排序完全够用)。后续可在分页 API 参数中扩展 sortBy/sortOrder 字段实现 server 排序。

### Decision 5: 类型签名重构

**方案**: 将 `DataTable<T>` 泛型约束为列定义的类型安全接口：

```tsx
interface ColumnDef<T> {
  id: string;
  header: string | React.ReactNode;
  accessorKey?: keyof T & string;
  accessorFn?: (row: T) => React.ReactNode;
  sortable?: boolean;
  align?: 'left' | 'center' | 'right';
  width?: string;
}
```

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| TanStack Table v8 API 学习成本 | 只使用 core + sorting 模块，API 稳定 |
| 现有 DataTable 使用者 (3页) 需适配新签名 | 保持 props 兼容性，只新增可选字段 |
| flex-wrap → `<table>` 的视觉差异 | 精确对应 Tailwind 暗色主题 class，对比截图验证 |
| PaginationBar 提取可能导致回归 | 逐页替换，每页验证分页逻辑正确 |
| TanStack Table 的 React 19 兼容性 | v8.20+ 已官方支持 React 19 |

## Migration Plan

1. **安装依赖**: `pnpm add @tanstack/react-table`
2. **创建 PaginationBar**: 抽取通用分页组件
3. **重写 DataTable**: 嵌入 TanStack Table，核心替换，保持接口兼容，增加排序支持
4. **替换 AgentsPage**: 使用新 DataTable，排序，PaginationBar
5. **替换 SchedulersPage**: 使用新 DataTable，排序
6. **替换 JobsPage**: 使用新 DataTable，PaginationBar，保留多选和 batch actions
7. **迁移 AgentDetailPage 子表**: 原生 `<table>` → DataTable
8. **迁移 SchedulerDetailPage 子表**: 原生 `<table>` → DataTable
9. **验证**: 逐页检查渲染、交互、分页正确

## Open Questions

- AgentsPage 的 server 分页是否要同时引入 server 排序？— **结论**: 暂不引入，先做 client 排序，后续 API 扩展 sort 参数后再升级。
