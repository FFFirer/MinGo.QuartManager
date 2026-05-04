## 1. 共享 JSON 序列化配置

- [x] 1.1 在 Shared 项目中创建 `MinGoJsonDefaults` 静态类，持有 CamelCase 的 `JsonSerializerOptions` 实例
- [x] 1.2 确认 `PropertyNamingPolicy = JsonNamingPolicy.CamelCase`，`WriteIndented = false`

## 2. Agent 项目 — 命名 HttpClient

- [x] 2.1 在 `AgentExtensions.cs` 中将 `AddHttpClient()` 替换为 `AddHttpClient("PlatformApi", ...)`，延迟配置 `BaseAddress`、`X-Agent-Token`、`Timeout`
- [x] 2.2 修改 `AgentRegistrationService.cs`：将 `CreateClient()` 替换为 `CreateClient("PlatformApi")`，为 `PostAsJsonAsync`、`ReadFromJsonAsync` 传入 `MinGoJsonDefaults.Options`
- [x] 2.3 修改 `HostedAgentService.cs`：将 `CreateClient()` 替换为 `CreateClient("PlatformApi")`，为 `PostAsJsonAsync`、`ReadFromJsonAsync` 传入 `MinGoJsonDefaults.Options`
- [x] 2.4 修改 `SchedulerReporterService.cs`：将 `CreateClient()` 替换为 `CreateClient("PlatformApi")`，为 `JsonContent.Create` 传入 `MinGoJsonDefaults.Options`
- [x] 2.5 修改 `LogCollectionService.cs`：将 `CreateClient()` 替换为 `CreateClient("PlatformApi")`，为 `PostAsJsonAsync` 传入 `MinGoJsonDefaults.Options`

## 3. Platform 项目 — 命名 HttpClient

- [x] 3.1 在 `Program.cs` 中将 `AddHttpClient()` 替换为 `AddHttpClient("AgentApi", ...)`，配置 `Timeout`
- [x] 3.2 修改 `AgentProxyService.cs`：将 `CreateClient()` 替换为 `CreateClient("AgentApi")`，重构请求 URL 构建方式，为 `JsonContent.Create` 和 `ReadFromJsonAsync` 传入 `MinGoJsonDefaults.Options`

## 4. 验证

- [x] 4.1 构建 Agent 项目，确认无编译错误
- [x] 4.2 构建 Platform 项目，确认无编译错误
- [x] 4.3 整体解决方案构建，确认无编译错误
