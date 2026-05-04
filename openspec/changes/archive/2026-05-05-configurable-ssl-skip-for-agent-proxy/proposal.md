## Why

Platform 通过 HTTP Client 调用 Agent API 时，如果 Agent 使用自签名 HTTPS 证书（开发/测试环境常见），请求会因为证书验证失败而抛出 `HttpRequestException`。当前 HttpClient 注册没有任何 SSL/TLS 配置能力，导致开发环境无法连接使用自签名证书的 Agent。

## What Changes

- 在 `appsettings.json` 中新增 `AgentProxy:SkipSslVerify` 配置项（默认 `false`）
- `Program.cs` 中改造 `"AgentApi"` 命名 HttpClient 的注册方式，通过 `SocketsHttpHandler` 条件性跳过 SSL 证书验证
- `appsettings.Development.json` 中设置 `AgentProxy:SkipSslVerify: true`，开发环境默认跳过验证

## Capabilities

### New Capabilities
- `configurable-agent-ssl`: 配置 Platform → Agent HTTP 调用是否跳过 SSL 证书验证，通过 `AgentProxy:SkipSslVerify` 配置项控制，默认安全（不跳过）

### Modified Capabilities

无

## Impact

- **src/MinGo.Qap.Platform/Program.cs**: HttpClient 注册方式变更，引入 `SocketsHttpHandler` + 条件性 `RemoteCertificateValidationCallback`
- **src/MinGo.Qap.Platform/appsettings.json**: 新增 `AgentProxy` 配置节
- **src/MinGo.Qap.Platform/appsettings.Development.json**: 开发环境覆盖为跳过验证
- **仅影响 Platform→Agent 方向**的 HTTP 调用（`AgentProxyService`），不影响 Agent→Platform 方向
