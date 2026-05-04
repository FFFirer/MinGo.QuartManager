## 1. Configuration

- [ ] 1.1 Add `AgentProxy:SkipSslVerify` setting (default `false`) to `src/MinGo.Qap.Platform/appsettings.json`
- [ ] 1.2 Add `AgentProxy:SkipSslVerify: true` override to `src/MinGo.Qap.Platform/appsettings.Development.json`

## 2. Core Implementation

- [ ] 2.1 Modify `Program.cs`: replace bare `AddHttpClient("AgentApi")` registration with `ConfigurePrimaryHttpMessageHandler` using `SocketsHttpHandler`
- [ ] 2.2 Add conditional `RemoteCertificateValidationCallback` that reads `IConfiguration` value `AgentProxy:SkipSslVerify`

## 3. Verification

- [ ] 3.1 Run `dotnet build` on Platform project to verify compilation
- [ ] 3.2 Run `dotnet build` on solution to verify no regressions
