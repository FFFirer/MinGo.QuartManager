## Why

当前 `jobType` 使用 manifest key（如 `EchoJob`）或三层 fallback 混合策略，导致：
- 同一 Job 在不同注册路径下返回的 jobType 格式不一致
- 维护复杂度高（ResolveJobType 含三层逻辑）
- JobDataMap 存储的 jobType 与 JobDetail 原生 CLR 类型脱节

改为 CLR `Type.FullName`（如 `Sample.Jobs.EchoJob`）后，jobType 始终等于 Quartz 原生的完整类型名，确定性强、无 fallback、无需额外存储。

## What Changes

- **Agent/QuartzService.cs**: `ResolveJobType()` 简化为 `jobDetail.JobType?.FullName ?? "unknown"`；`CreateJobAsync()` 改为按 `JobTypeFullName` 查 Registry
- **Agent/JobRegistry.cs**: 新增 `GetByFullName(string)` 方法
- **Agent/JobConverter.cs**: `jobDataMap["jobType"]` 写入 FullName（向后兼容）
- **UI/types**: `JobTypeInfoDto` 补充 `jobTypeFullName` 字段
- **UI/JobDetailPage**: 参数查找改为匹配 `jobTypeFullName`
- **UI/CreateJobPanel**: 创建时发送 `jobTypeFullName` 而非 `key`
- **UI/JobTypeDisplay**: 新增显示组件（截断+复制+tooltip）

## Capabilities

### New Capabilities
- `job-type-display`: 针对 JobType 全限定名称的 UI 显示优化组件，支持长名截断、hover tooltip 和快速复制

### Modified Capabilities
<!-- No existing capabilities modified -->

## Impact

- **Breaking** for direct API consumers: `CreateJobRequest.JobType` 语义从 manifest key 变为 CLR FullName
- Agent 侧 3 个文件需改动
- 前端 4 个文件需改动 + 1 个新组件
