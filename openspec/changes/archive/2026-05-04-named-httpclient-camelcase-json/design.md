## Context

### Current State

Agent 与 Platform 两个项目都使用未命名 HttpClient 进行 HTTP 通信：

```
Agent项目:
  AgentExtensions.cs:    services.AddHttpClient()               // 无命名
  AgentRegistration:     _httpClientFactory.CreateClient()       // 未命名
  HostedAgentService:    httpClientFactory.CreateClient()        // 未命名
  SchedulerReporter:     _httpClientFactory.CreateClient()       // 未命名
  LogCollectionService:  _httpClientFactory.CreateClient()       // 未命名

Platform项目:
  Program.cs:            services.AddHttpClient()               // 无命名
  AgentProxyService:     _httpClientFactory.CreateClient()       // 未命名, 每次手动设 BaseAddress
```

JSON 序列化使用 `System.Text.Json` 默认选项（`JsonSerializerDefaults.General` = PascalCase），与 ASP.NET Core 默认的 CamelCase 不一致。

### 核心问题
1. **连接池浪费**: `CreateClient()` 创建的是轻量级客户端，但未命名客户端无法集中配置，导致超时、重试等策略分散在各服务中
2. **序列化不一致**: HttpClient 通信使用 PascalCase，ASP.NET Core 使用 CamelCase，存在潜在字段无法匹配风险
3. **配置分散**: BaseAddress、认证头、超时等在每个调用点重复设置

## Goals / Non-Goals

**Goals:**
- Agent 项目注册名为 `"PlatformApi"` 的命名 HttpClient
- Platform 项目注册名为 `"AgentApi"` 的命名 HttpClient
- 所有服务改用命名 HttpClient
- 统一 JSON 序列化选项为 CamelCase（`PropertyNamingPolicy = JsonNamingPolicy.CamelCase`）
- 向后兼容：不改变 DTO 结构，仅改变序列化配置

**Non-Goals:**
- 不引入第三方 JSON 库（如 Newtonsoft.Json）
- 不修改 ASP.NET Core 侧的 `AddJsonOptions` 配置（已为 CamelCase，无需变更）
- 不涉及重试策略、熔断等高级 HttpClient 功能（后续可通过 `AddHttpMessageHandler` 扩展）
- 不修改数据库持久化逻辑中的 `JsonSerializer.Serialize` 调用（仅 HTTP 通信层）
- 不修改仓储、服务接口或 DTO 定义

## Decisions

### Decision 1: 命名 HttpClient vs 自定义 JsonSerializerOptions 参数

**选择**: 命名 HttpClient + `ConfigureHttpClientDefaults` / `Services.AddHttpClient("name", configure)` 的集中配置方式

**替代方案**: 在每个调用点传入 `JsonSerializerOptions` 参数
- 优点：精确控制
- 缺点：侵入性大，容易遗漏，代码冗余

**理由**: 命名 HttpClient 通过 DI 集中配置，一处修改全局生效，代码改动量更小，且受益于 IHttpClientFactory 的连接池复用

### Decision 2: Agent 端命名 HttpClient 的配置范围

Agent 向 Platform 发起请求，涉及 5 个服务：
- 所有请求目标 URL 相同（`Platform.Url`）
- 所有请求都需要 `X-Agent-Token` 头
- 共享同一 JSON 序列化配置

**方式**: 在 `AgentExtensions.RegisterAgentServices` 中通过 `AddHttpClient("PlatformApi")` 集中配置 `BaseAddress`、默认头、`JsonSerializerOptions`。

但问题是 `BaseAddress` 在注册时来自 `IOptions<AgentConfig>`，需要延迟获取。解决方案：

```csharp
services.AddHttpClient("PlatformApi", (sp, client) =>
{
    var config = sp.GetRequiredService<IOptions<AgentConfig>>();
    client.BaseAddress = new Uri(config.Value.Platform.Url.TrimEnd('/'));
    client.DefaultRequestHeaders.Add("X-Agent-Token", config.Value.Platform.ApiToken);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigureHttpMessageHandlerBuilder(builder =>
{
    // 默认 handler 配置
});
```

同时，可通过 `ConfigureHttpClientDefaults` 全局设置 JSON 序列化选项：

```csharp
services.ConfigureHttpClientDefaults(http =>
{
    http.Services.Configure<HttpClientFactoryOptions>(options =>
    {
        options.HttpClientActions.Add(client =>
        {
            // 默认配置
        });
    });
});
```

但更好的做法是在每个命名客户端上显式配置 JSON 选项。考虑到 `System.Net.Http.Json` 的 `ReadFromJsonAsync`/`PostAsJsonAsync` 使用 `JsonSerializerDefaults.Web` 的静态选项，我们需要确保命名客户端使用正确的 `JsonSerializerOptions`。

实际上，`ReadFromJsonAsync` 和 `PostAsJsonAsync` 是扩展方法，它们默认使用 `JsonSerializerOptions.Default`（PascalCase）。如果我们在命名客户端上配置了 `JsonSerializerOptions`，这些扩展方法不会自动使用。

**更可靠的方案**：在调用处显式传递 `JsonSerializerOptions` 参数。

**或者**：使用 `System.Text.Json` 的 `JsonSerializerDefaults.Web`，其默认即为 CamelCase。

方案选型：

| 方案 | 描述 | 优点 | 缺点 |
|------|------|------|------|
| A | 在命名 HttpClient configure 中设置 | 集中配置 | `ReadFromJsonAsync` 不自动使用 |
| B | 在每个调用处传 `JsonSerializerOptions` 参数 | 精确控制 | 侵入性大，容易遗漏 |
| C | 自定义扩展方法包装 `ReadFromJsonAsync`/`PostAsJsonAsync` | 集中控制，使用方便 | 需要额外抽象层 |
| D | 使用静态 `JsonSerializerOptions` 单例 + 工具类 | 一处定义，全局使用 | 需替换所有调用点 |

**最终选择: D** — 创建 `MinGoJsonDefaults` 静态类持有共享的 camelCase `JsonSerializerOptions` 实例，在所有 `ReadFromJsonAsync`、`PostAsJsonAsync`、`JsonContent.Create` 调用中显式传入。这样命名 HttpClient 主要负责 URL 和头配置，JSON 选项通过显式参数确保准确。

### Decision 3: Platform 端 AgentProxyService 重构

当前 `AgentProxyService` 手动创建 HttpClient、设置 BaseAddress 和 Timeout，且每次调用重新创建。

重构为注入命名 HttpClient `"AgentApi"`，但特殊之处在于 AgentProxyService 的 BaseAddress 是**动态的**（通过 `PickAgentForSchedulerAsync` 获取），无法在 DI 注册时固定。

**方案**: 在 `AgentProxyService` 中注入 `IHttpClientFactory` 并使用 `CreateClient("AgentApi")`，在每次请求前：
1. 通过 `PickAgentForSchedulerAsync` 获取 Agent URL
2. 将 BaseAddress 设置为该 Agent URL
3. 设置 `X-Scheduler-Name` 头
4. 发送请求

这样命名 HttpClient 主要负责 JSON 序列化配置和连接池，URL 在运行时动态设置。

## Risks / Trade-offs

| 风险 | 缓解措施 |
|------|----------|
| AgentRegistrationService 在 `AddHttpClient` 配置时 `IOptions<AgentConfig>` 可能尚未就绪 | 使用工厂委托 `(sp, client) =>` 延迟解析；确认配置在 AddMinGoAgent 中已注册 |
| `JsonSerializerOptions` 单例并发安全 | `JsonSerializerOptions` 是不可变的，配置后不应修改；或使用 `new JsonSerializerOptions { ... }` 初始化 |
| AgentProxyService 的 BaseAddress 需要在运行时动态设置 | 不在 DI 配置中设 BaseAddress，改为在每个请求前在 `HttpRequestMessage.RequestUri` 中构造完整 URL |
| 更改 JSON 序列化策略可能导致已序列化存储的数据（数据库）与新序列化的数据不一致 | 数据库存储使用独立的 `JsonSerializer.Serialize` 调用（`JobService.cs`），不受影响 |
