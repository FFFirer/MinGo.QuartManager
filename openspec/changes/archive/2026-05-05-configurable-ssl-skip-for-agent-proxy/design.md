## Context

Platform 的 `AgentProxyService` 使用 `IHttpClientFactory` 创建的命名 HttpClient `"AgentApi"` 向远程 Agent 发送 HTTP 请求。当前在 `Program.cs` 中仅配置了超时，没有任何 SSL/TLS 配置能力。

当 Agent 使用自签名 HTTPS 证书（开发/测试环境常见）暴露 API 时，`HttpClient` 默认走系统证书链验证，导致 `AuthenticationException` 并最终表现为 `HttpRequestException`，使得 Platform 无法与该 Agent 通信。

## Goals / Non-Goals

**Goals:**
- Platform → Agent 的 HTTP 调用可通过配置跳过 SSL 证书验证
- 默认行为安全（不跳过），不影响生产环境
- 开发环境开箱即用，无需额外证书配置
- 修改范围最小化，只涉及 HttpClient 注册层，不侵入业务代码

**Non-Goals:**
- 不支持 Agent→Platform 方向的 SSL 跳过（超出本变更范围）
- 不做细粒度证书校验（如按域名白名单、证书指纹匹配）— 这些是后续增强
- 不改动 AgentProxyService.cs 或其他业务代码

## Decisions

### 1. 技术方案：`SocketsHttpHandler` + `RemoteCertificateValidationCallback`

选择 `.ConfigurePrimaryHttpMessageHandler()` 注入 `SocketsHttpHandler`，而非全局 `ServicePointManager`。

| 方案 | 优点 | 缺点 |
|------|------|------|
| **SocketsHttpHandler**（选） | 仅影响 `"AgentApi"` HttpClient；.NET 5+ 推荐方式 | 需要额外 handler 配置代码 |
| `ServicePointManager.ServerCertificateValidationCallback` | 一行代码 | 全局生效，影响所有 HTTP 请求；.NET Core 3+ 不再推荐用于 `SocketsHttpHandler` |
| 自定义 `HttpClientHandler` | 兼容性好 | `SocketsHttpHandler` 是 .NET 5+ 默认后端，更推荐 |

### 2. 配置方式：`IConfiguration` + `appsettings.json`

通过标准的 ASP.NET Core 配置系统注入，而非独立的 Options 类。原因：
- 零额外依赖（不需要注册新的 Options 类型）
- 与环境变量、命令行参数天然兼容
- 保持简单，这个场景不需要复杂的配置验证

### 3. 默认值：`false`（不跳过）

生产安全优先。开发环境通过 `appsettings.Development.json` 明确启用，避免误打开。

## Risks / Trade-offs

| 风险 | 缓解措施 |
|------|----------|
| 开发环境误设为 `true` 后无法发现真实的证书问题 | 仅作用于开发环境；生产环境严格要求 `false` |
| 跳过验证降低了 HTTPS 连接的安全性 | 这是自签名证书场景的固有权衡；具备配置显式打开即有意识承担此风险 |
| `SocketsHttpHandler` 配置错误导致 HttpClient 创建失败 | 改动代码极小，仅在 handler 配置中增加条件分支；已有 LSP 和编译验证 |
