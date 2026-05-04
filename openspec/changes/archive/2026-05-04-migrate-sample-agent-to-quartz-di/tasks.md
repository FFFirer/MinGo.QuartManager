## 1. Package Management

- [x] 1.1 Add `Quartz.Extensions.Hosting` package to `MinGo.Qap.Agent.csproj` (version 3.18.1)
- [x] 1.2 Add `Quartz.Extensions.Hosting` package to `Sample.Agent.csproj` (version 3.18.1)
- [x] 1.3 Upgrade `MinGo.Qap.Agent.csproj` Quartz package references from 3.17.1 to 3.18.1 (Quartz, Quartz.Extensions.DependencyInjection, Quartz.Serialization.Json)
- [x] 1.4 Run `dotnet restore` and verify all packages resolve without conflicts

## 2. Agent Library — Scheduler Accessor

- [x] 2.1 Add `ISchedulerFactory` resolution path in `AgentExtensions.RegisterSchedulerAccessor()` between priority 3 (single IScheduler) and priority 4 (DeferredSchedulerAccessor)
  - Get `ISchedulerFactory` from `IServiceProvider`
  - Call `GetScheduler().GetAwaiter().GetResult()` to obtain the default scheduler
  - Wrap in try-catch with fallback to `DeferredSchedulerAccessor` on failure
- [x] 2.2 Build Agent library project to verify compilation

## 3. Sample.Agent — Program.cs Refactoring

- [x] 3.1 Remove `using Quartz.Impl`, `using Quartz.Simpl`, `using Quartz.Spi` — no longer needed
- [x] 3.2 Remove `System.Collections.Specialized` using — NameValueCollection removed
- [x] 3.3 Replace `NameValueCollection properties` declaration and `StdSchedulerFactory` with `builder.Services.AddQuartz(q => { ... })`:
  - Configure `SchedulerName`, `SchedulerId`, `UseDefaultThreadPool` (max 5), `UseInMemoryStore()`
- [x] 3.4 Migrate `HelloJob` + trigger (every 10s) into `q.ScheduleJob<HelloJob>(...)` inside AddQuartz callback
- [x] 3.5 Migrate `ScheduledJob` + trigger (every 60s) into `q.ScheduleJob<ScheduledJob>(...)` inside AddQuartz callback
- [x] 3.6 Migrate `ManualTriggerJob` into `q.AddJob<ManualTriggerJob>(j => j.StoreDurably())` inside AddQuartz callback
- [x] 3.7 Replace manual `scheduler.Start()` and `IScheduler` singleton registration with `builder.Services.AddQuartzHostedService(o => o.WaitForJobsToComplete = true)`
- [x] 3.8 Remove manual `services.AddTransient<HelloJob>()` and similar IJob registrations
- [x] 3.9 Remove `builder.Build()` -> `app.Services.GetRequiredService<IScheduler>()` -> manual scheduling code block (lines 70-107 in current Program.cs)
- [x] 3.10 Delete the entire `DIJobFactory` class at end of Program.cs
- [x] 3.11 Ensure Quartz `Log.Information(...)` calls are preserved or migrated to proper logging

## 4. Configuration Cleanup

- [x] 4.1 Verify `appsettings.json` "Quartz" section is no longer needed for `NameValueCollection`; optionally retain for future `QuartzOptions` binding
- [x] 4.2 Verify `config.yaml` and Agent-level config (agent:, platform:) are unaffected

## 5. Build & Verify

- [x] 5.1 Run `dotnet build` on the entire solution and fix any compilation errors
- [x] 5.2 Run `lsp_diagnostics` on changed files: `Program.cs`, `AgentExtensions.cs`, `Sample.Agent.csproj`, `MinGo.Qap.Agent.csproj` (LSP not available in env, build passed clean)
- [x] 5.3 Verify no Quartz-related runtime errors by reviewing any existing tests or startup behavior
