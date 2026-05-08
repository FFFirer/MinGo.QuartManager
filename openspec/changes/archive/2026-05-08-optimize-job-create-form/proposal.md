## Why

当前创建 Job 表单存在三个问题：(1) Job Identity 块 Group 在前 Name 在后，不符合直觉，且 Name 缺少字符合法性校验；(2) 带默认值的参数项，用户不修改时仍提示"此字段为必填项"；(3) Schedule=None 时 storeNonDurableWhileAwaitingScheduling 参数传递不够精确，应仅当 storeDurable=false 时使用。

## What Changes

1. **Job Identity 布局与校验调整**：交换 Group 和 Name 的 UI 位置，Name 在前 Group 在后；Name 添加正则校验，只允许字母数字、`-` 和 `_`。
2. **Parameters 默认值预填**：选择 Job Type 时，将 Manifest 中定义参数项的 `default` 值自动写入 `params` state，避免因未显式设值而导致必填校验误报。
3. **Schedule=None 时 storeNonDurableWhileAwaitingScheduling 条件化**：后端 `QuartzService.CreateJobAsync` 中，仅当 `storeDurable=false` 时传 `storeNonDurableWhileAwaitingScheduling: true`；当 `storeDurable=true` 时不传此参数。

## Capabilities

### New Capabilities

无新能力引入。

### Modified Capabilities

- `job-create-form`: Job Identity 中 Name/Group 布局变更，Name 添加正则校验；Parameters 区域默认值预填逻辑变更；Schedule=None 场景的 storeDurable 交互逻辑更新
- `none-schedule-type`: None Schedule 下非持久化 Job 的 storeNonDurableWhileAwaitingScheduling 条件更新
- `durable-job-option`: 无 spec 级行为变更，仅实现调整

## Impact

- **前端**: `src/MinGo.Qap.UI/src/pages/CreateJobPage.tsx` — JSX 布局调整、validate() 逻辑、handleJobTypeChange 逻辑
- **后端 Agent**: `src/MinGo.Qap.Agent/Services/QuartzService.cs` — CreateJobAsync 条件化 storeNonDurableWhileAwaitingScheduling
- **Specs**: `openspec/specs/job-create-form/spec.md` 和 `openspec/specs/none-schedule-type/spec.md` 更新
