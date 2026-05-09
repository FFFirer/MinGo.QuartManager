## Why

当前 Job 详情页只展示第一个 Trigger 的摘要信息（通过 Schedule 字段），用户无法看到该 Job 关联的完整 Trigger 列表。在 Quartz 中一个 Job 可以关联多个 Trigger，但目前系统：

- Agent 端 `GetTriggersOfJob()` 获取了全部 triggers 却只用了 `FirstOrDefault()`
- `JobDetailDto` 只包含单 Schedule 字段，没有 trigger 列表
- 前端 JobDetailPage 没有任何 trigger 相关信息展示

## What Changes

- **新增** `TriggerSummaryDto` 共享模型，包含 Trigger 的关键元数据
- **更新** `JobDetailDto` 添加 `List<TriggerSummaryDto> Triggers` 字段
- **更新** `JobDefinitionDto` 添加 `List<TriggerSummaryDto>? Triggers` 字段
- **更新** Agent `QuartzService.GetJobAsync()`：迭代所有 triggers 填充列表，每个 trigger 附带 state
- **更新** Platform `JobService.GetAsync()`：从 Agent 返回的 `JobDetailDto` 映射 triggers 到 `JobDefinitionDto`
- **前端新增** `TriggerSummaryDto` TypeScript 接口
- **前端更新** `JobDetailPage.tsx`：在信息网格下方展示 Trigger 列表（使用 DataTable 组件）
- 不改变现有 API 路由或 DB schema
