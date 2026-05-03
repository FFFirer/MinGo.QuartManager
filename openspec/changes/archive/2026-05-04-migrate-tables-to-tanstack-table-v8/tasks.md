## 1. 依赖安装

- [x] 1.1 安装 `@tanstack/react-table` 依赖: `pnpm add @tanstack/react-table`

## 2. 创建通用分页组件 PaginationBar

- [x] 2.1 在 `src/components/PaginationBar.tsx` 中创建 PaginationBar 组件，支持 page/pageSize/totalItems/totalPages/onPageChange/onPageSizeChange props
- [x] 2.2 支持 page size 选择 (10/20/50/100)、上/下页按钮、页码按钮、"Showing X-Y of Z" 文字
- [x] 2.3 与现有暗色主题样式一致

## 3. 重写 DataTable 组件（TanStack Table v8 集成）

- [x] 3.1 重构 `DataTable.tsx`：内部使用 `@tanstack/react-table` 的 `useReactTable` + `getCoreRowModel` + `getSortedRowModel`
- [x] 3.2 渲染层从 flex-wrap 改为原生 `<table>`/`<thead>`/`<tbody>`/`<tr>`/`<th>`/`<td>`
- [x] 3.3 保持对外 props 兼容：columns/ data/ loading/ emptyMessage/ onRowClick/ showBorder/ showHeader/ className
- [x] 3.4 新增 `sortable` 列属性支持，列头点击排序
- [x] 3.5 排序指示器：列头显示排序箭头 (↑/↓)
- [x] 3.6 使用 CheckboxColumn 类型支持行多选（header 和 cell 渲染 checkbox）
- [x] 3.7 完善泛型类型签名，消除 `any[]` 模式

## 4. 替换 AgentsPage

- [x] 4.1 使用新 DataTable 组件（受益于排序能力）
- [x] 4.2 分页控件替换为 PaginationBar 组件
- [x] 4.3 验证 server 分页逻辑正确（page、pageSize 参数）

## 5. 替换 SchedulersPage

- [x] 5.1 使用新 DataTable 组件（受益于排序能力）
- [x] 5.2 验证无分页场景下渲染正确

## 6. 替换 JobsPage

- [x] 6.1 使用新 DataTable 组件
- [x] 6.2 分页控件替换为 PaginationBar 组件（当前为 client 分页，保持不变）
- [x] 6.3 checkbox 多选和 batch actions 功能在新 DataTable 上验证正确

## 7. 迁移详情页子表

- [x] 7.1 AgentDetailPage：将 schedulers 子表从原生 `<table>` 改为 DataTable 组件
- [x] 7.2 SchedulerDetailPage：将 agents 子表从原生 `<table>` 改为 DataTable 组件
- [x] 7.3 保持原有行点击导航行为

## 8. 验证与清理

- [x] 8.1 逐页检查：AgentsPage / SchedulersPage / JobsPage / AgentDetailPage / SchedulerDetailPage
- [x] 8.2 `lsp_diagnostics` 所有修改文件，无类型错误
- [ ] 8.3 `pnpm build` 构建通过（存在预存错误，非本次变更导致）
- [x] 8.4 检查各页面的排序交互、分页切换、行选择、行点击行为正常
