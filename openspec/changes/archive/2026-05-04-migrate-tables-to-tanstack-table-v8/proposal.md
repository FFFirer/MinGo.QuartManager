## Why

当前 `DataTable` 组件基于 flex-wrap 布局，功能薄弱（无排序、无筛选），分页逻辑在 3 个页面中重复实现，且 2 个详情页仍在使用原生 `<table>`。需要引入成熟的 headless table 方案统一所有表格实现，消除重复代码，并为后续功能扩展打好基础。

## What Changes

- 将 `@tanstack/react-table` v8 引入依赖，重构 `DataTable.tsx` 内部实现（对外接口基本不变）
- 提取通用 `<PaginationBar>` 组件，消除 AgentsPage / JobsPage 中重复的分页 UI 代码
- AgentDetailPage 的 schedulers 子表从原生 `<table>` 迁移到 DataTable
- SchedulerDetailPage 的 agents 子表从原生 `<table>` 迁移到 DataTable
- 新增列排序支持（基于 TanStack Table 的排序模型）
- 完善类型签名，消除 `columns: any[]` 模式
- 更新 `ui-data-table.spec.md` 和 `data-table-standardization` delta spec 以反映新能力

## Capabilities

### New Capabilities
- `sortable-columns`: 表头点击排序功能，支持多列排序和服务器端排序

### Modified Capabilities
- `data-table-standardization`: 扩展 DataTable 标准化要求，纳入 TanStack Table 作为底层实现，增加排序支持
- `agent-pagination`: 分页 UI 改为使用通用 `<PaginationBar>` 组件

## Impact

- **新依赖**: `@tanstack/react-table` (~14KB gzipped)
- **修改文件**:
  - `src/components/DataTable.tsx` — 重写内部逻辑，对外接口小幅扩展（支持排序）
  - `src/components/PaginationBar.tsx` — 新组件，从 AgentsPage/JobsPage 提取
  - `src/pages/AgentsPage.tsx` — 改用 PaginationBar，受益于 DataTable 排序
  - `src/pages/SchedulersPage.tsx` — 受益于 DataTable 排序
  - `src/pages/JobsPage.tsx` — 改用 PaginationBar，受益于 DataTable 排序+行选择
  - `src/pages/AgentDetailPage.tsx` — 子表从原生 `<table>` 改为 DataTable
  - `src/pages/SchedulerDetailPage.tsx` — 子表从原生 `<table>` 改为 DataTable
  - `openspec/specs/ui-data-table.spec.md` — 新增排序相关场景
  - `openspec/specs/data-table-standardization/` — delta spec 更新
- **无 Breaking Change**: 现有 DataTable props 保持兼容，新增属性为可选
