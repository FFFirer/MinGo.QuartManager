## Context

Job 创建流程目前强制绑定 Trigger（Schedule 类型为 Once/Cron/Interval）。但在某些场景下，用户需要：
1. 创建纯手动触发的 Job（无 Trigger，通过 API 手动触发）
2. 先创建 Job 定义，后续再配置 Trigger
3. 持久化 Job 确保即使无 Trigger 也不会被 Quartz 自动删除

当前 QuartzService.CreateJobAsync 总是创建 Trigger，且 JobDetail 未设置 StoreDurable。

## Goals / Non-Goals

**Goals:**
- Schedule 新增 "None" 类型，创建时不生成 Trigger
- QuartzOptionsDto 新增 StoreDurable 选项，独立于 Schedule 类型
- 无 Trigger 时通过 `scheduler.AddJob(detail, replace, storeNonDurableWhileAwaitScheduling: true)` 保留 Job
- Agent 侧 CreateJob 方法重构，清晰分离"创建 JobDetail"和"创建 Trigger"两个步骤

**Non-Goals:**
- 不修改 UpdateJob 流程（更新时仍可修改 Schedule）
- 不涉及 Trigger 的手动管理界面（后续可单独实现）
- 不修改数据库表结构（StoreDurable 存储在 Options JSON 中）

## Decisions

### Decision 1: StoreDurable 作为 QuartzOptionsDto 属性
- **方案**: 在 `QuartzOptionsDto` 新增 `StoreDurable` bool 属性
- **理由**: Options 本身就是传递 Quartz 配置的载体，StoreDurable 是 Quartz JobDetail 的标准属性，放在此处语义清晰
- **替代方案**: 放在 ScheduleDto 中 → 不合理，StoreDurable 与 Schedule 正交

### Decision 2: Schedule=None 的触发行为
- **方案**: 当 `schedule.Type == "none"` 时，JobConverter.ConvertToTrigger 返回 null，QuartzService.CreateJobAsync 跳过 trigger 创建
- **理由**: 简洁，最小改动；Quartz 的 `AddJob` 第三个参数 `storeNonDurableWhileAwaitScheduling` 天然支持此场景
- **行为**:
  - StoreDurable=false + Schedule=None → `AddJob(detail, true, storeNonDurableWhileAwaitScheduling: true)` → Job 保留直到 Trigger 被添加
  - StoreDurable=true + Schedule=None → `AddJob(detail, true)` → Job 永久保留

### Decision 3: GetScheduleType 映射 "none"
- **方案**: 在 GetScheduleType 中，trigger==null 时返回 "none" 而非 "unknown"
- **理由**: 前端需要明确的类型标识来展示 Schedule 信息；"unknown" 不够精确

### Decision 4: 前端 Schedule=None 的文案
- **方案**: 按钮文案 "None"（英文）、Tooltip "不创建 Trigger"
- **理由**: 与现有 Once/Cron/Interval 保持一致风格

## Risks / Trade-offs

- **storeNonDurableWhileAwaitScheduling 的语义**：在 Quartz 中，此参数确保非持久化 Job 在 AddJob 时不会因无 Trigger 被立即删除。但当 Scheduler 重启后，这些 Job 是否保留取决于 JobStore 实现。RAMStore 会丢失，AdoJobStore 会保留。
- **无 Trigger 的 Job 在 UI 中被视为 "none" 状态**：现有的 StatusBadge 和 Trigger 状态显示逻辑需要确认能正确处理 triggerState=None。
