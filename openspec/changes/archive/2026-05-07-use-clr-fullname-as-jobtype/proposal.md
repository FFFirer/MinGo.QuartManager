## Why

当前 `jobType` 使用 manifest key（如 `EchoJob`）或三层 fallback 混合策略，导致：
- 同一 Job 在不同注册路径下返回的 jobType 格式不一致
- 维护复杂度高（ResolveJobType 含三层逻辑）
- JobDataMap 存储的 jobType 与 JobDetail 原生 CLR 类型脱节
- 无法展示程序集信息，CLR FullName 在 UI 上可读性差

改为从 `Type.AssemblyQualifiedName` 解析出结构化 `JobTypeQualifiedName`（`fullName` + `assembly` + 元数据），确定性强、无 fallback、无需额外存储，且 UI 能精确展示"程序集.类名"。

## What Changes

- **Shared/Models/JobTypeQualifiedName.cs**: 新增结构化模型，提供 `ParseFrom(Type)` / `ParseFrom(string)` / `ToAssemblyQualifiedName()` 方法
- **Shared/Models/JobDtos.cs**: `JobType` 字段从 `string` 改为 `JobTypeQualifiedName`（4 个 DTO）
- **Shared/Models/JobManifestDtos.cs**: `JobTypeFullName` 改为 `JobTypeQualifiedName`
- **Agent/QuartzService.cs**: `ResolveJobType()` 返回 `JobTypeQualifiedName`；`CreateJobAsync()` 按 `fullName` 查 Registry
- **Agent/JobRegistry.cs**: `GetByFullName()` 匹配 `fullName`（不受 version/culture 影响）
- **Agent/JobConverter.cs**: `jobDataMap["jobType"]` 写入拼接串；`Type.GetType()` 用 `"fullName, assembly"` 解析
- **Agent/JobDiscoveryService.cs**: 填充 `JobTypeQualifiedName`
- **UI/types**: 新增 `JobTypeQualifiedName` 接口，更新所有 DTO
- **UI/JobTypeDisplay**: 接收结构化数据，按 `assembly` 灰色 + `fullName` 最后一段亮色渲染
- **UI/JobDetailPage**: 参数查找改为匹配 `jobTypeQualifiedName.fullName`
- **UI/CreateJobPanel**: 发送结构化 `JobTypeQualifiedName`

## Capabilities

### New Capabilities
- `job-type-display`: 针对 JobType 结构化数据的 UI 显示组件，支持程序集前缀、长名截断、hover tooltip 和快速复制

### Modified Capabilities
- `shared-models`: `JobType` 字段类型从 `string` 变为 `JobTypeQualifiedName` 结构化模型

## Impact

- **Breaking** for API consumers: `JobType` 响应字段从 `string` 变为 `JobTypeQualifiedName` 对象；`CreateJobRequest.JobType` 也变为对象
- Shared 侧 1 个新文件 + 2 个 DTO 文件字段类型变更
- Agent 侧 4 个文件需改动
- 前端 4 个文件需改动 + 1 个已有组件适配
