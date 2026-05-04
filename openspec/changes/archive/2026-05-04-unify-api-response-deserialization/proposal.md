## Why

Platform 和 Agent 之间的 HTTP 通信使用 `ApiResponse<T>` 作为统一响应包装，但接收方（AgentProxyService 和 Agent HttpClient 调用方）在反序列化时直接将 `ApiResponse<T>` 整个 JSON 对象当作裸 `T` 解析，没有解包 `.Data` 字段。这导致：
- `List<T>` 类型响应因 JSON 对象 ≠ 数组而崩溃
- 对象类型响应全部字段为默认值（`null`/`0`/`""`）
- 业务错误（`Success=false`）被静默吞掉

需要统一 `ApiResponse<T>` 的解包机制，确保两端通信正确可靠。

## What Changes

- **新增** `ReadFromApiResponseAsync<T>()` 扩展方法在 `MinGo.Qap.Shared`，统一解包 `ApiResponse<T>` 并处理错误
- **新增** `ApiResponseException` 异常类，携带 `ErrorCode`
- **修复** `AgentProxyService.HandleResponse<T>` 改用新的扩展方法解析 Agent 响应
- **修复** `AgentRegistrationService.RegisterAsync` 改用新的扩展方法解析 Platform 注册响应
- **修复** `HostedAgentService.SendHeartbeatAsync` 改用新的扩展方法解析 Platform 心跳响应
- **新增 capability** `api-response-deserialization` 覆盖统一解包规范

## Capabilities

### New Capabilities
- `api-response-deserialization`: 定义 `ApiResponse<T>` 的跨服务解包规范、扩展方法、异常处理策略

### Modified Capabilities
- `http-client-json-serialization`: 补充响应解包规范，与现有 JSON 序列化配置保持一致

## Impact

- `MinGo.Qap.Shared` — 新增 `HttpContentApiResponseExtensions.cs` 和 `ApiResponseException`
- `MinGo.Qap.Platform` — 修改 `AgentProxyService.HandleResponse<T>` 1 处
- `MinGo.Qap.Agent` — 修改 `AgentRegistrationService.cs` 和 `HostedAgentService.cs` 共 2 处
- 无外部依赖变更，无 breaking API 变更
