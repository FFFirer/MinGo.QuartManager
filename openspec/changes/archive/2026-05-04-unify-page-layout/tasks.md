## 1. PR #1 — AgentsPage + AgentDetailPage Layout Wrapper Fix

- [ ] 1.1 AgentsPage: Change wrapper from `min-h-screen bg-slate-900 text-slate-50` to `p-6`, remove redundant `px-4 py-6` inner div
- [ ] 1.2 AgentDetailPage: Change wrapper from `min-h-screen bg-slate-900 text-slate-50 p-6` to `p-6` (apply to loading, error, and main states)
- [ ] 1.3 AgentDetailPage: Remove manual `← Back to Agents` link, use PageHeader `backPath="/agents"` instead
- [ ] 1.4 AgentDetailPage: Remove duplicate `<h1>` title, let PageHeader render the title
- [ ] 1.5 Verify: `lsp_diagnostics` clean on changed files, visual check of AgentsPage and AgentDetailPage

## 2. PR #2 — Backend Agent Pagination Support

- [ ] 2.1 AgentsController: Add `[FromQuery] int page = 1, int pageSize = 20` to `GetList()`
- [ ] 2.2 AgentService: Rename `GetAllAsync()` to `GetPagedAsync(int page, int pageSize)` that returns paginated result with total count
- [ ] 2.3 Shared: Add/verify `PagedResult<T>` response type if not already present
- [ ] 2.4 Build and verify backend compiles and pagination works

## 3. PR #3 — DataTable Standardization + AgentsPage Pagination

- [ ] 3.1 DataTable: Change `Column.header` type from `string` to `string | ReactNode`, remove `as any` cast in JobsPage
- [ ] 3.2 Frontend types: Add pagination response type for agents (extend or reuse `PagedResponse<T>`)
- [ ] 3.3 Frontend API: Update `agentApi.getAll(page?, pageSize?)` to pass pagination params
- [ ] 3.4 AgentsPage: Replace custom CSS grid with DataTable component, add pagination controls
- [ ] 3.5 SchedulersPage: Migrate native `<table>` to DataTable
- [ ] 3.6 AgentDetailPage: Migrate associated schedulers table to DataTable
- [ ] 3.7 SchedulerDetailPage: Migrate associated agents table to DataTable
- [ ] 3.8 Verify: `lsp_diagnostics` clean, build passes, all pages render correctly
