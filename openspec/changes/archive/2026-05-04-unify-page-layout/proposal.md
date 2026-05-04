## Why

当前前端页面布局不统一：AgentsPage 和 AgentDetailPage 使用了与其他页面不同的 wrapper 结构（冗余的 `min-h-screen bg-slate-900 text-slate-50`），且 AgentsPage 使用自定义 CSS grid 表格而非统一的 DataTable 组件，SchedulersPage 等也使用原生 `<table>` 而非 DataTable。同时 AgentsPage 缺少分页功能。这不仅影响视觉一致性，也增加了维护成本。

## What Changes

1. **AgentsPage** — wrapper 从 `min-h-screen bg-slate-900 text-slate-50` 改为 `p-6`，自定义 grid 表格迁移至 DataTable，增加分页
2. **AgentDetailPage** — wrapper 精简，PageHeader 正确使用（删除手动 Back 链接、删除重复 h1 标题）
3. **SchedulersPage** — 原生 `<table>` 迁移至 DataTable 组件
4. **AgentDetailPage / SchedulerDetailPage** — 内嵌原生 `<table>` 迁移至 DataTable 组件
5. **后端 Agent API** — 增加分页查询参数支持（page, pageSize, 返回 total count）
6. **前端 Agent API** — 增加分页参数，增加分页响应类型

## Capabilities

### New Capabilities
- `agent-pagination`: Agent 列表分页查询能力，后端支持 page/pageSize 参数，返回分页元数据
- `data-table-standardization`: DataTable 组件统一使用规范，替代所有原生 `<table>` 用法

### Modified Capabilities
- `agent-management`: AgentsPage 布局和 AgentDetailPage 布局规范化，与现有页面模板保持一致

## Impact

- **前端**: AgentsPage.tsx, AgentDetailPage.tsx, SchedulersPage.tsx, SchedulerDetailPage.tsx, DataTable.tsx, api/index.ts, types/index.ts
- **后端**: AgentsController.cs, AgentService.cs, Shared DTO (PagedResult)
- **无新增依赖**，全部使用已有组件（DataTable, PageHeader）
