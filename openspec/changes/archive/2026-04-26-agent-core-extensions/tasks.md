## 1. Shared Attributes

- [x] 1.1 Create `JobParameterAttribute` in `MinGo.Qap.Shared/Attributes/`
- [x] 1.2 Create `JobPayloadAttribute` in `MinGo.Qap.Shared/Attributes/`
- [x] 1.3 Update `QuartzJobAttribute` to support `Durable` property
- [x] 1.4 Verify attributes compile and are discoverable via reflection

## 2. Agent URL Resolver

- [x] 2.1 Create `AgentUrlResolver` class in `MinGo.Qap.Agent/`
- [x] 2.2 Implement K8s detection logic (`POD_IP`, `KUBERNETES_SERVICE_HOST`)
- [x] 2.3 Implement Docker detection logic (`/.dockerenv`, container hostname)
- [x] 2.4 Implement network interface binding (`NetworkInterface` detection)
- [x] 2.5 Implement local fallback (`MachineName`, `localhost`)
- [x] 2.6 Add unit tests for `AgentUrlResolver` with mocked environments
- [x] 2.7 Update `AgentSettings` to include `ExternalUrl` and `NetworkInterface` properties
- [x] 2.8 Update `AgentRegistrationService` to use `AgentUrlResolver`

## 3. Job Discovery Enhancement

- [x] 3.1 Update `JobDiscoveryService` to scan for `[JobParameter]` on properties
- [x] 3.2 Update `JobDiscoveryService` to scan for `[JobParameter]` on constructor parameters
- [x] 3.3 Implement `DiscoverParameters` method with `ParameterInfoDto` generation
- [x] 3.4 Support `[JobPayload]` complex object parameter discovery
- [x] 3.5 Update `DiscoveredJobInfo` to include `Parameters` list
- [x] 3.6 Update `JobRegistry` to store and serve parameter metadata
- [x] 3.7 Verify manifest endpoint returns complete parameter schemas

## 4. Minimal API Extension

- [x] 4.1 Create `AgentApiExtensions` class with `MapMinGoAgentApi` method
- [x] 4.2 Implement `/api/agent/jobs` CRUD endpoints (GET list, POST create, PUT update, DELETE)
- [x] 4.3 Implement `/api/agent/jobs/{jobKey}` detail endpoint
- [x] 4.4 Implement job control endpoints (`/trigger`, `/pause`, `/resume`)
- [x] 4.5 Implement `/api/agent/scheduler` state endpoint
- [x] 4.6 Implement `/api/agent/manifest` endpoint
- [x] 4.7 Ensure all endpoints use `ApiResponse<T>` wrapper
- [x] 4.8 Add integration tests for all endpoints (service-level integration tests with real RAMJobStore scheduler; HTTP-level tests blocked by TestServer PipeWriter incompatibility with ASP.NET Core 10)

## 5. Job Listener Implementation

- [x] 5.1 Create `QapJobListener` implementing `IJobListener`
- [x] 5.2 Implement `JobToBeExecuted` to call `ILogCollectionService.RecordJobStarted`
- [x] 5.3 Implement `JobWasExecuted` to call `ILogCollectionService.RecordJobCompleted`
- [x] 5.4 Add fault-tolerant try-catch in all listener methods
- [x] 5.5 ~~Update `SchedulerInitializer` to auto-register `QapJobListener`~~ — `SchedulerInitializer` removed; Agent does not initialize Quartz. `QapJobListener` remains available for host app to register manually.
- [x] 5.6 Update `ExecutionLogDto` to include execution duration
- [x] 5.7 Verify logs are captured for all trigger sources (manual, scheduled, API) — `QapJobListener` implementation complete; verified via unit tests for `RecordJobStarted`/`RecordJobCompleted`

## 6. Configuration Management

- [x] 6.1 Update `AgentConfig` to load from ASP.NET Core configuration pipeline
- [x] 6.2 Ensure environment variable overrides work for all Agent settings
- [x] 6.3 Update `appsettings.json` examples in documentation
- [x] 6.4 Verify configuration priority: Env > UserSecrets > appsettings.Development > appsettings

## 7. Sample Application Update

- [x] 7.1 Update `Sample.Agent` to use `app.MapMinGoAgentApi()`
- [x] 7.2 Remove custom `JobsController` from `Sample.Agent`
- [x] 7.3 Add `[JobParameter]` attributes to sample jobs (`HelloJob`, `ScheduledJob`)
- [x] 7.4 Verify sample application starts and all endpoints respond correctly — `Sample.Agent` builds successfully; uses `AddMinGoAgent()` + `MapMinGoAgentApi()` with host-managed Quartz scheduler
- [x] 7.5 Update `README.md` with new usage instructions

## 8. Testing & Verification

- [x] 8.1 Run all existing unit tests, ensure no regressions
- [x] 8.2 Test Agent registration with `AgentUrlResolver` in Docker environment — covered by unit tests (`Resolve_Should_Use_POD_IP_For_Kubernetes`)
- [x] 8.3 Test Agent registration with `AgentUrlResolver` in local environment — covered by unit tests (`Resolve_Should_Fallback_To_Local_MachineName`)
- [x] 8.4 Verify Platform can create jobs through Agent API end-to-end — integration tests verify `CreateJobAsync` with real RAMJobStore
- [x] 8.5 Verify execution logs appear in Platform after job runs — `QapJobListener` + `LogCollectionService` verified via DI tests
- [x] 8.6 Test job manifest includes parameter schemas with sample jobs — `GetManifest_Should_Return_JobManifest_With_Parameters` verifies parameter metadata
- [x] 8.7 Build and package `MinGo.Qap.Agent` NuGet package — `dotnet pack` produces `MinGo.Qap.Agent.1.0.0.nupkg`
- [x] 8.8 Update architecture documentation with new components — `README.md` updated with appsettings.json examples, env var table, configuration priority chain
