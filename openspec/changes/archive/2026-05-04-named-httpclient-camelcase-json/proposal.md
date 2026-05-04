## Why

Agent 项目与 Platform 项目均使用未命名 HttpClient（`IHttpClientFactory.CreateClient()`），导致 JSON 序列化/反序列化使用 System.Text.Json 默认配置（PascalCase），与 ASP.NET Core 默认的 CamelCase 不一致，且连接池管理不完善。这会造成：

1. JSON 属性命名风格不一致，Agent ↔ Platform 之间 DTO 传递可能出现字段匹配问题
2. `AgentProxyService` 每次调用都创建新 HttpClient，socket 耗尽风险
3. 配置分散在各服务中，无法集中管理超时、认证头、序列化等

## What Changes

- **Agent 项目**：将 `AddHttpClient()` 改为命名 HttpClient `"PlatformApi"`，集中配置 BaseAddress、认证头、JSON 序列化选项
- **Platform 项目**：将 `AddHttpClient()` 改为命名 HttpClient `"AgentApi"`，集中配置 JSON 序列化选项
- 所有使用 `IHttpClientFactory.CreateClient()` 的服务改为使用命名 HttpClient
- 统一 JSON 序列化配置为 CamelCase（`JsonNamingPolicy.CamelCase`），包括序列化与反序列化
- `AgentProxyService` 的 `CreateClient()` 方法重构为使用命名 HttpClient，消除手动设置 BaseAddress/Timeout 的模式

## Capabilities

### New Capabilities
- `http-client-json-serialization`: Named HttpClient 配置与 camelCase JSON 序列化策略，覆盖 Agent 到 Platform 以及 Platform 到 Agent 的所有 HTTP 通信

### Modified Capabilities
- *无*（纯基础设施改进，不涉及用户可见行为变更）

## Impact

### 受影响的项目
- **src/MinGo.Qap.Agent/** — `AgentExtensions.cs`, `AgentRegistrationService.cs`, `HostedAgentService.cs`, `SchedulerReporterService.cs`, `LogCollectionService.cs`
- **src/MinGo.Qap.Platform/** — `Program.cs`, `AgentProxyService.cs`

### 受影响的外部 API
- 无（内部通信重构，不改变公开 API）

### 依赖项
- 无新增依赖

### 兼容性
- 向后兼容：Agent 与 Platform 双方的 JSON 命名策略统一为 CamelCase，DTO 序列化行为一致
