## Purpose

定义 `ApiResponse<T>` 统一响应包装的解包规范，确保 Agent 和 Platform 之间 HTTP 通信的响应解析一致性和错误处理正确性。

## Requirements

### Requirement: 提供 `ReadFromApiResponseAsync<T>` 扩展方法

`MinGo.Qap.Shared` SHALL 提供 `ReadFromApiResponseAsync<T>` 扩展方法，用于统一解包 HTTP 响应中的 `ApiResponse<T>` 包装。

#### Scenario: 成功解包标准 ApiResponse 响应

- **WHEN** HTTP 响应包含 `{"success": true, "data": {...}}` 且 `data` 的类型为 `T`
- **THEN** 返回 `data` 的反序列化结果，类型为 `T?`

#### Scenario: Success=false 时抛 ApiResponseException

- **WHEN** HTTP 响应包含 `{"success": false, "errorMessage": "...", "errorCode": "..."}`
- **THEN** 抛出 `ApiResponseException`，其 `Message` 为 `errorMessage` 值，`ErrorCode` 为 `errorCode` 值

#### Scenario: 响应格式不是 ApiResponse 包装时自动回退

- **WHEN** HTTP 响应不包含 `"success"` 或 `"data"` 顶层字段（即不是 `ApiResponse<T>` 格式）
- **THEN** 直接按 `T` 类型反序列化原始 JSON

#### Scenario: 空响应或无内容时返回 default

- **WHEN** HTTP 响应状态码为 204 NoContent 或响应体为空
- **THEN** 返回 `default(T)`

### Requirement: 定义 `ApiResponseException` 异常类型

`MinGo.Qap.Shared` SHALL 定义 `ApiResponseException` 异常类，用于表示 API 业务层错误（`Success=false`）。

#### Scenario: ApiResponseException 继承自 Exception

- **WHEN** `ApiResponseException` 被定义
- **THEN** 它继承自 `Exception`
- **AND** 包含 `ErrorCode` 属性（`string?`）
- **AND** 构造函数接受 `message` 和可选的 `errorCode` 参数

#### Scenario: ApiResponseException 在 Direction B 中被 AgentException 包装

- **WHEN** `AgentProxyService.HandleResponse<T>` 捕获到 `ApiResponseException`
- **THEN** 将其转为 `AgentException`，保留 `ErrorCode` 和 `Message`
- **AND** 上层 `catch (AgentException)` 块能正常捕获

### Requirement: AgentProxyService.HandleResponse 使用新解包逻辑

`AgentProxyService.HandleResponse<T>` SHALL 使用 `ReadFromApiResponseAsync<T>` 替代 `ReadFromJsonAsync<T>` 来解析 Agent 的 HTTP 响应。

#### Scenario: 成功解包

- **WHEN** Agent 返回 HTTP 200 + `ApiResponse<T>` 格式的 JSON
- **THEN** `HandleResponse` 返回 `.Data` 的解包结果

#### Scenario: Agent 返回 Success=false

- **WHEN** Agent 返回 HTTP 200 + `{"success": false, "errorMessage": "...", "errorCode": "..."}`
- **THEN** `HandleResponse` 抛出 `AgentException`，携带 `ErrorCode`
- **AND** 记录错误日志

#### Scenario: 非 200 HTTP 状态码

- **WHEN** Agent 返回 HTTP 4xx/5xx
- **THEN** `HandleResponse` 继续使用原有逻辑，抛 `AgentException`（不改变）

### Requirement: Agent 端 HTTP 调用使用新解包逻辑

Agent 项目中通过 `HttpClient` 调用 Platform API 并期望 `ApiResponse<T>` 响应的代码 SHALL 使用 `ReadFromApiResponseAsync<T>` 替代 `ReadFromJsonAsync<T>`。

#### Scenario: AgentRegistrationService 注册响应使用新解包

- **WHEN** `AgentRegistrationService.RegisterAsync` 从 Platform 读取注册响应
- **THEN** 使用 `ReadFromApiResponseAsync<RegisterAgentResponse>` 而非 `ReadFromJsonAsync<RegisterAgentResponse>`

#### Scenario: HostedAgentService 心跳响应使用新解包

- **WHEN** `HostedAgentService.SendHeartbeatAsync` 从 Platform 读取心跳响应
- **THEN** 使用 `ReadFromApiResponseAsync<AgentHeartbeatResponseV2>` 而非 `ReadFromJsonAsync<AgentHeartbeatResponseV2>`
