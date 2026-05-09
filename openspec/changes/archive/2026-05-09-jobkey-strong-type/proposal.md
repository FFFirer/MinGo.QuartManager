## Why

目前 JobKey/TriggerKey 以字符串 `"Group.Name"` 格式在整个系统中传递，依赖 3 个不一致的 `ParseJobKey()` 方法来解析 group 和 name。这种设计导致：
- 解析顺序不统一（Platform 返回 `(group, name)`，Agent 返回 `(name, group)`），埋下隐患
- group/name 中包含 `.` 时无法正确解析
- 前端表单已分开输入 group 和 name，却在 API 边界强行拼成字符串再解析
- 没有类型安全，编译期无法捕获错误

需要引入强类型 `JobKeyDto` 来消除所有字符串解析，并统一前后端数据传递。

## What Changes

- **BREAKING**: 新增 `JobKeyDto` readonly record struct，包含 `Name`（必填）+ `Group`（默认 `"DEFAULT"`）
- **BREAKING**: 所有 DTO 的 `JobKey: string` 或 `JobKey: string` + `Group: string` 替换为 `JobKey: JobKeyDto`
- **BREAKING**: API URL 路径从 `{jobKey}` 改为 `{name}/{group?}`，不再使用 `Group.Name` 格式
- **BREAKING**: 删除所有 `ParseJobKey()` 方法（Platform/JobService、Agent/QuartzService、Agent/JobConverter）
- **BREAKING**: 前端所有 `jobKey: string` 替换为 `{ name: string; group: string }`
- DB: JobDefinition 添加 `Name` 列，新增联合索引 `(SchedulerName, Group, Name)`
- TriggerKey 保持内部使用（由 JobKey 确定性派生），不暴露到 API

## Capabilities

### New Capabilities
- `job-key-dto`: 强类型 JobKey 定义、序列化、DTO 集成
- `job-api-routing`: 新的 RESTful URL 路由模式 `{name}/{group?}`

### Modified Capabilities
- (无既存 specs 需要修改)

## Impact

| 层 | 影响范围 |
|---|---|
| Shared/Models | 6 个 DTO 改动，新增 JobKeyDto.cs |
| Platform/Controllers | JobsController 所有端点路由签名 |
| Platform/Services | JobService 接口和实现的方法签名 |
| Platform/Data | JobDefinition entity + EF migration |
| Agent/API | AgentApiExtensions 所有 handler 签名 |
| Agent/Services | QuartzService、JobConverter、QapJobListener |
| Platform/Services | AgentProxyService URL 构造 |
| UI/types | 6 个接口改动，parseJobKey() 删除 |
| UI/api | 8 个 API 方法 URL 构造 |
| UI/pages | 4 个页面 + App.tsx 路由 |
