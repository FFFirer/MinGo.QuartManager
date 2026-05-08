## Why

用户在创建 Job 时需要更灵活的调度配置选项：支持创建持久化 Job（不依赖 Trigger 也能保留在 Scheduler 中），以及创建时完全不绑定 Trigger（用于后续手动或通过 API 添加 Trigger）。

## What Changes

- **Schedule 新增 "None" 类型**：创建 Job 时可选不创建 Trigger，Job 以 `storeNonDurableWhileAwaitScheduling` 方式保留，直到 Trigger 被添加
- **新增 "持久化 Job" 选项**：独立于 Schedule 的 `StoreDurable` 开关，勾选后 Job 即使无 Trigger 也永久保留
- **Agent 侧 CreateJob 方法重构**：优化 Job 创建逻辑，使其能正确处理无 Trigger 场景

## Capabilities

### New Capabilities
- `durable-job-option`: 创建 Job 时支持勾选"持久化 Job"(StoreDurable)，独立于 Schedule 类型
- `none-schedule-type`: 创建 Job 时 Schedule 可选"None"类型，不创建 Trigger，Job 通过 storeNonDurableWhileAwaitScheduling 保留

### Modified Capabilities
- `job-create-form`: 创建 Job 表单新增 Schedule=None 选项和持久化 Job 开关

## Impact

- **Agent**: `QuartzService.CreateJobAsync` — 重构以支持无 Trigger 创建；`JobConverter.ConvertToTrigger` — 返回 null 处理；`JobConverter.ConvertToDetail` — 处理 StoreDurable
- **Shared**: `QuartzOptionsDto` — 新增 `StoreDurable` 属性
- **UI**: `CreateJobPage.tsx` — Schedule 选择器增加 None 选项，Options 增加持久化 Job checkbox；`types/index.ts` — ScheduleType 增加 None
