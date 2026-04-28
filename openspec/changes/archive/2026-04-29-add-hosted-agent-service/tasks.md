## 1. Create HostedAgentService

- [x] 1.1 Create `src/MinGo.Qap.Agent/Services/HostedAgentService.cs` with `BackgroundService` base class
- [x] 1.2 Implement startup phase: call `IAgentRegistrationService.RegisterAsync()` with retry
- [x] 1.3 Implement heartbeat loop: periodic `SendHeartbeatAsync()` using interval from registration response
- [x] 1.4 Implement shutdown phase: override `StopAsync` to call `IAgentRegistrationService.DeregisterAsync()`
- [x] 1.5 Implement heartbeat failure handling: re-registration on 401/404 or 3 consecutive network failures
- [x] 1.6 Implement heartbeat interval dynamic update from registration response

## 2. Register HostedAgentService in DI

- [x] 2.1 Add `services.AddHostedService<HostedAgentService>()` in `AgentExtensions.AddMinGoAgent()`
- [x] 2.2 Verify `HostedAgentService` resolves all constructor dependencies (IHttpClientFactory, IConfiguration, etc.)

## 3. Deprecate Standalone HeartbeatService

- [x] 3.1 Add `[Obsolete]` attribute to `HeartbeatService` class indicating `HostedAgentService` as replacement
- [x] 3.2 Verify no DI registration references `HeartbeatService` as a hosted service

## 4. Verify Build and Tests

- [x] 4.1 Build the solution with `dotnet build` — 0 errors in Agent project
- [x] 4.2 Run existing Agent tests — 20/20 pass, no regressions
- [x] 4.3 Code verified — no new diagnostics introduced (pre-existing warnings only)

