## Context

Platform 和 Agent 之间通过 HTTP 通信时，两端都使用 `ApiResponse<T>` 作为统一响应包装格式。但接收方在反序列化时直接将 HTTP Response Body（完整 `ApiResponse<T>` JSON 对象）当作目标类型 T 解析，没有解包 `.Data` 字段。

涉及两条通信链路：
- **Direction B**: Platform → Agent（`AgentProxyService` 转发 HTTP 到 Agent Minimal API）
- **Direction C**: Agent → Platform（Agent 的 `HttpClient` 调用 Platform Controller）

当前现状：
- 两端均通过 `MinGoJsonDefaults.Options`（`JsonSerializerDefaults.Web`，CamelCase）序列化/反序列化
- Agent 的 Minimal API 统一返回 `Results.Ok(ApiResponse<T>.Ok(data))`
- Platform Controller 统一返回 `Ok(ApiResponse<T>.Ok(response))`
- 但接收方用 `ReadFromJsonAsync<T>()` 直接读完整包装，而不是先读 `ApiResponse<T>` 再取 `.Data`

## Goals / Non-Goals

**Goals:**
- 统一 `ApiResponse<T>` 响应解包机制，覆盖 Direction B 和 Direction C
- 新增 `ReadFromApiResponseAsync<T>()` 扩展方法，自动解包 `.Data` 并处理 `Success=false`
- 新增 `ApiResponseException` 异常类型，携带 `ErrorCode` 供调用方区分业务错误
- 修复 `AgentProxyService.HandleResponse<T>`、`AgentRegistrationService.RegisterAsync`、`HostedAgentService.SendHeartbeatAsync` 三个调用点
- 向后兼容：支持非 `ApiResponse<T>` 包装的端点（fallback 到直接反序列化）

**Non-Goals:**
- 不改变 `ApiResponse<T>` 本身的定义
- 不改变 `MinGoJsonDefaults.Options` 序列化配置
- 不改变 Platform Controller 的返回值模式（继续返回 `ActionResult<ApiResponse<T>>`）
- 不改变 Agent Minimal API 的返回值模式（继续返回 `ApiResponse<T>`）
- 不涉及前端 UI 层的响应处理

## Decisions

### Decision 1: 扩展方法 vs 中间件 vs JsonConverter

**选择：`HttpContent` 扩展方法 `ReadFromApiResponseAsync<T>()`**

| 方案 | 优点 | 缺点 |
|---|---|---|
| **扩展方法**（选） | 显式调用、类型安全、可加 fallback、无隐式行为 | 需手动替换所有调用点 |
| 中间件自动解包 | 透明无侵入 | 隐式行为易混淆、无法处理 Direction C（Agent 侧无 ASP.NET 管道） |
| 自定义 `JsonConverter<T>` | 对 `ReadFromJsonAsync` 透明 | 无法处理 `Success=false` 抛异常、兼容性风险高 |

**理由**：扩展方法最显式、可控。可以在方法内处理 `Success=false` → 抛异常、非包装响应 → fallback 到直接反序列化。

### Decision 2: `Success=false` 时抛异常 vs 返回 default

**选择：抛 `ApiResponseException`**

- 当前系统对响应失败的处理是分散的：有些检查 `IsSuccessStatusCode`，有些完全不检查
- 统一抛异常让调用方能通过 `try/catch` 集中处理错误
- `ApiResponseException` 携带 `ErrorCode` 和 `ErrorMessage`，与 `ApiResponse<T>` 的字段一致
- 对 Direction B，`AgentProxyService.HandleResponse` 已在非 HTTP 200 时抛 `AgentException`，`ApiResponseException` 允许继承 `AgentException` 保持 catch 兼容

### Decision 3: 向后兼容 fallback

**选择：先用 `JsonDocument` 探测顶层是否有 `"success"` 和 `"data"` 字段**

- 如果探测到 `"success"` 且 `"data"` 存在 → 按 `ApiResponse<T>` 流程处理
- 如果探测不到 → 直接按 `T` 反序列化（兼容未包装的端点）
- 避免双重反序列化的性能开销（仅需一次字符串读取 + `JsonDocument.Parse`）

### Decision 4: `ApiResponseException` 继承关系

**选择：继承 `AgentException`（对 Direction B）但作为独立异常（对 Direction C）**

- Direction B 中 `AgentProxyService` 已抛 `AgentException`，`ApiResponseException : AgentException` 让现有 catch 兼容
- Direction C 中 Agent 服务不需要 `AgentException`，直接 catch `ApiResponseException` 即可
- 异常层次：`ApiResponseException : AgentException : Exception`

## Risks / Trade-offs

| 风险 | 缓解措施 |
|---|---|
| 已有端点的响应格式不统一（有些没包 `ApiResponse<T>`） | Fallback 探测机制：先检测 `"success"+"data"` 字段存在性 |
| `JsonDocument.Parse` 额外性能开销 | 仅解析元数据探测结构，不完整反序列化；相比当前错误的解析，这是正确的代价 |
| 双重包装 `ApiResponse<ApiResponse<T>>` | 在探测阶段检查 `data` 字段的值是否也是 `ApiResponse` 形状；设计上禁止在应用层双重包装 |
| Direction B 的 `HandleResponse` 已有错误处理逻辑（抛 `AgentException`） | `ApiResponseException : AgentException`，不影响现有 `catch (AgentException)` 块 |
| `HostedAgentService` 的心跳循环中抛异常 | 心跳循环已有 `catch (Exception ex)`，`ApiResponseException` 会被兜底捕获并计入连续失败计数 |
