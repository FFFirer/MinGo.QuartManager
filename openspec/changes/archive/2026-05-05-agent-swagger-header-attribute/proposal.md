## Why

当前系统中的 Agent 相关 API 接口缺少统一的机制来在 Swagger UI 中展示 Header 参数。

- **Platform 端**: `AgentsController` 通过硬编码路径匹配的 `AgentTokenHeaderProcessor` 添加了 `X-Agent-Token`，但 `SchedulersController`、`JobsController`、`ManifestController` 等代理到 Agent 的端点没有任何 Header 文档。维护成本高，扩展新端点容易遗漏。
- **Agent 端**: `MapMinGoAgentApi()` 的 Minimal API 端点完全没有 OpenAPI 文档，`X-Scheduler-Name` Header 虽被代码实际读取但 Swagger UI 不可见。

这导致 API 消费者无法从 Swagger UI 了解认证和路由参数的传递方式，增加了集成难度和调试成本。

## What Changes

1. **新增 `SwaggerHeaderAttribute`** — 可在 Controller action 和 Minimal API 处理函数上标注的自定义特性，零外部依赖，放在 `MinGo.Qap.Shared` 项目
2. **新增 `SwaggerHeaderProcessor : IOperationProcessor`** — 统一的 NSwag Operation Processor，读取 `[SwaggerHeader]` 特性并自动添加到 OpenAPI operation.parameters，替换现有的 `AgentTokenHeaderProcessor`
3. **Platform 控制器标注** — 在 `AgentsController`、`SchedulersController`、`JobsController`、`ManifestController` 的 action 方法上按实际用途添加 `[SwaggerHeader]`
4. **Agent Minimal API 重构** — `AgentApiExtensions.cs` 中的 lambda 表达式改为局部函数，添加 `[SwaggerHeader]` 标注，使 `X-Scheduler-Name` 在 Swagger UI 中可见
5. **Agent 端 OpenApi 扩展** — 提供 `AddMinGoAgentOpenApi()` 扩展方法，宿主应用一行代码即可注册 Processor
6. **移除旧的 `AgentTokenHeaderProcessor`** — 不再需要硬编码路径匹配

## Capabilities

### New Capabilities
- `agent-swagger-header`: 为 Agent 相关 API 端点通过 `[SwaggerHeader]` 特性 + NSwag `IOperationProcessor` 的统一机制在 Swagger UI 中展示 Header 参数

### Modified Capabilities
- （无，本次不修改已有 capability 的行为要求）

## Impact

| 项目 | 文件 | 变更类型 |
|------|------|----------|
| `MinGo.Qap.Shared` | `Attributes/SwaggerHeaderAttribute.cs` | **新增** |
| `MinGo.Qap.Platform` | `NSwag/SwaggerHeaderProcessor.cs` | **新增**（替换 `AgentTokenHeaderProcessor`） |
| `MinGo.Qap.Platform` | `NSwag/AgentTokenHeaderProcessor.cs` | **删除** |
| `MinGo.Qap.Platform` | `Program.cs` | **修改**（注册新 Processor） |
| `MinGo.Qap.Platform` | `Controllers/AgentsController.cs` | **修改**（添加 `[SwaggerHeader]`） |
| `MinGo.Qap.Platform` | `Controllers/SchedulersController.cs` | **修改**（添加 `[SwaggerHeader]`） |
| `MinGo.Qap.Platform` | `Controllers/JobsController.cs` | **修改**（添加 `[SwaggerHeader]`） |
| `MinGo.Qap.Platform` | `Controllers/ManifestController.cs` | **修改**（添加 `[SwaggerHeader]`） |
| `MinGo.Qap.Agent` | `AgentApiExtensions.cs` | **修改**（lambda → 局部函数 + `[SwaggerHeader]`） |
| `MinGo.Qap.Agent` | `OpenApi/AgentOpenApiExtensions.cs` | **新增** |
| `MinGo.Qap.Agent` | `MinGo.Qap.Agent.csproj` | **修改**（增加 `NSwag.AspNetCore` 引用） |
| `Sample.Agent` | `Program.cs` | **修改**（调用 `AddMinGoAgentOpenApi()`） |
