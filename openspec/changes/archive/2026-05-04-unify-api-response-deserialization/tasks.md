## 1. Shared: 扩展方法与异常类

- [x] 1.1 在 `MinGo.Qap.Shared` 中新增 `HttpContentApiResponseExtensions.cs`，实现 `ReadFromApiResponseAsync<T>` 扩展方法（含 `JsonDocument` 探测 fallback）
- [x] 1.2 在 `MinGo.Qap.Shared` 中新增 `ApiResponseException` 异常类，继承 `Exception`，携带 `ErrorCode`

## 2. Direction B: 修复 AgentProxyService 响应解包

- [x] 2.1 修改 `AgentProxyService.HandleResponse<T>`，用 `ReadFromApiResponseAsync<T>` 替代 `ReadFromJsonAsync<T>`，捕获 `ApiResponseException` 转为 `AgentException`

## 3. Direction C: 修复 Agent 端响应解包

- [x] 3.1 修改 `AgentRegistrationService.RegisterAsync`，用 `ReadFromApiResponseAsync<RegisterAgentResponse>` 替代 `ReadFromJsonAsync<RegisterAgentResponse>`
- [x] 3.2 修改 `HostedAgentService.SendHeartbeatAsync`，用 `ReadFromApiResponseAsync<AgentHeartbeatResponseV2>` 替代 `ReadFromJsonAsync<AgentHeartbeatResponseV2>`

## 4. 验证

- [x] 4.1 运行 `dotnet build` 确认编译通过（0 errors, 0 new warnings）
- [x] 4.2 检查 LSP diagnostics 无错误（所有修改文件均通过）
