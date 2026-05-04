## Context

Sample.Agent (`samples/Sample.Agent/`) 是 MinGo 平台的参考 Agent 实现，演示宿主程序如何集成 Quartz.NET Scheduler 与 Agent 库。当前它使用 Quartz.NET 3.0 时代的初始化方式（手动 `StdSchedulerFactory` + `NameValueCollection` + 自定义 `IJobFactory`），与 Quartz 3.3.2+ 推荐的 Microsoft DI 集成模式严重脱节。

Quartz.NET 自 3.3.2 起内置了完整的 DI 集成：
- 通过 `AddQuartz()` 在 `IServiceCollection` 上配置 Scheduler
- Job 类型由 DI 容器自动解析（或 `ActivatorUtilities` 回退）
- 通过 `AddQuartzHostedService()` 将 Scheduler 生命周期绑定到 ASP.NET Core 托管系统

Agent 库 (`MinGo.Qap.Agent`) 当前通过 `IAgentSchedulerAccessor` 接口从 DI 容器中查找 `IScheduler` 实例，这需要与官方 DI 模式兼容。

## Goals / Non-Goals

**Goals:**
- Sample.Agent 的 Quartz 初始化完全迁移到官方 `AddQuartz() + AddQuartzHostedService()` 模式
- 移除所有冗余代码（`DIJobFactory`、手动 `AddTransient<IJob>`、`builder.Build()` 后调度）
- 确保 Agent 库的 `IAgentSchedulerAccessor` 能正确发现通过官方模式注册的 Scheduler
- Package 版本在 Agent 库和 Sample.Agent 之间对齐
- 保留现有 Quartz 行为语义（RAMScheduler, 5 threads, 相同 Job/Trigger 定义）

**Non-Goals:**
- 不改变 Agent 库的 `IAgentSchedulerAccessor` 公共接口契约
- 不涉及 Platform 端的变更
- 不迁移到持久化 JobStore（继续使用 RAMStore）
- 不引入多 Scheduler 模式
- 不改动 Job 类本身（HelloJob, ScheduledJob, ManualTriggerJob）

## Decisions

### Decision 1: 使用 `AddQuartz()` Fluent API 替换 `StdSchedulerFactory`

**选择**: 用 `services.AddQuartz(q => { ... })` 替换 `new StdSchedulerFactory(NameValueCollection)` + `AddSingleton<IScheduler>`。

**理由**:
- Fluent API 类型安全，有 IntelliSense 支持
- 自动注册 `ISchedulerFactory` 到 DI 容器供注入
- 配置在编译期即可验证，而非运行时从 `NameValueCollection` 解析
- 与官方文档和社区实践完全对齐

**替代方案**: 继续使用 `StdSchedulerFactory` 但通过 `services.Configure<QuartzOptions>()` 绑定配置。但这仍是二等公民，无法利用 `ScheduleJob<T>()` 等便捷 API。

### Decision 2: 使用 `AddQuartzHostedService()` 替换手动 `scheduler.Start()`

**选择**: 用 `services.AddQuartzHostedService(o => o.WaitForJobsToComplete = true)` 替换 `IScheduler` 单例中的手动 `scheduler.Start()`。

**理由**:
- Scheduler 生命周期与 `IHostedService` 绑定，应用停止时自动触发优雅关停
- `WaitForJobsToComplete = true` 确保执行中的 Job 完成后才退出
- 移除 Program.cs 中的命令式启动代码

### Decision 3: 移除自定义 `DIJobFactory` 类

**选择**: 删除内联定义的 `DIJobFactory` 类，依赖 Quartz 3.3.2+ 内置的 DI 解析机制。

**理由**:
- Quartz 3.3.2 起默认 JobFactory 会优先从 DI 容器解析 Job 类型
- 未在 DI 注册的 Job 类型通过 `ActivatorUtilities` 构造，自动注入构造函数依赖
- 不再需要 `UseMicrosoftDependencyInjectionJobFactory()` — 这也是旧 API，文档已标为过时

### Decision 4: Job 调度移入 `AddQuartz()` 回调

**选择**: 将 `builder.Build()` 之后的 `JobBuilder.Create<HelloJob>()` + `scheduler.ScheduleJob()` 移入 `AddQuartz(q => { ... })` 中使用 `q.ScheduleJob<T>()` / `q.AddJob<T>()`。

**理由**:
- Quartz 会在 `AddQuartzHostedService` 启动时自动调度所有配置的 Job
- 消除了 `app.Services.GetRequiredService<IScheduler>()` 这个反模式
- 代码集中在 DI 配置阶段，职责清晰

**注意**: `ManualTriggerJob` 使用 `StoreDurably()` 保持为无触发器的持久 Job，通过 API 手动触发。

### Decision 5: Agent Accessor 增加 `ISchedulerFactory` 解析路径

**选择**: 在 `AgentExtensions.RegisterSchedulerAccessor()` 中，在优先级 3（单 `IScheduler`）之后、优先级 4（`DeferredSchedulerAccessor`）之前，插入新的解析路径：尝试从 DI 获取 `ISchedulerFactory`，若成功则调用 `GetScheduler().GetAwaiter().GetResult()` 获取 `IScheduler`。

```csharp
// 优先级 3.5：宿主使用 ISchedulerFactory（官方 DI 模式）
var schedulerFactory = sp.GetService<ISchedulerFactory>();
if (schedulerFactory != null)
{
    try
    {
        var scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();
        return new AgentSchedulerAccessor(
            new Dictionary<string, IScheduler>
            {
                [scheduler.SchedulerName] = scheduler
            });
    }
    catch
    {
        // Scheduler 尚未就绪，继续走延迟发现
    }
}
```

**理由**: `AddQuartz()` 在 DI 中注册的是 `ISchedulerFactory`，不直接暴露 `IScheduler` 实例。Agent 需要一个新的路径来通过 Factory 获取 Scheduler。

**替代方案 A**: 修改 `DeferredSchedulerAccessor` 也尝试 `ISchedulerFactory`。但 `DeferredSchedulerAccessor` 是容错退路，把 Factory 解析放在主路径更合理。

**替代方案 B**: 在宿主侧（Sample.Agent）额外注册 `IScheduler` 桥接：
```csharp
services.AddSingleton<IScheduler>(sp => 
    sp.GetRequiredService<ISchedulerFactory>().GetScheduler().GetAwaiter().GetResult());
```
这不需要改 Agent 库，但额外在 DI 中桥接了一层，增加了歧义。

### Decision 6: 简化 Job DI 注册

**选择**: 移除 `services.AddTransient<HelloJob>()` 等显式注册。

**理由**: `AddQuartz()` 中的 `ScheduleJob<HelloJob>()` 已经引用了 Job 类型。Quartz 的 DI JobFactory 在解析 Job 时会检查 DI 容器，若未注册则使用 `ActivatorUtilities` 自动构造（注入 `ILogger<HelloJob>` 等构造函数依赖）。显式的 `AddTransient` 注册是冗余的。

### Decision 7: 版本对齐

**选择**: 将 `MinGo.Qap.Agent` 的 Quartz 包版本从 3.17.1 升级到 3.18.1，与 Sample.Agent 一致。

**理由**: 避免同一解决方案内多版本冲突。3.17.1 → 3.18.1 是 patch 级别变更，无 Breaking Changes。

## 迁移后架构

```
┌─ Program.cs (Sample.Agent) ──────────────────────────────────┐
│                                                                │
│  builder.Services.AddQuartz(q =>                               │
│  {                                                             │
│      q.SchedulerName = "SampleAgentScheduler";                 │
│      q.SchedulerId = "AUTO";                                   │
│      q.UseDefaultThreadPool(tp => tp.MaxConcurrency = 5);      │
│      q.UseInMemoryStore();                                     │
│                                                                │
│      q.ScheduleJob<HelloJob>(opts => opts                      │
│          .WithIdentity("HelloJob", "sample")                   │
│          .StartNow()                                           │
│          .WithSimpleSchedule(x => x                            │
│              .WithIntervalInSeconds(10).RepeatForever()));     │
│                                                                │
│      q.AddJob<ManualTriggerJob>(opts => opts                   │
│          .WithIdentity("ManualTriggerJob", "sample")           │
│          .StoreDurably());                                     │
│  });                                                           │
│                                                                │
│  builder.Services.AddQuartzHostedService(q =>                  │
│      q.WaitForJobsToComplete = true);                          │
│                                                                │
│  // Program.cs 中不再需要:                                       │
│  //   StdSchedulerFactory                                      │
│  //   IScheduler Singleton                                     │
│  //   DIJobFactory                                             │
│  //   AddTransient<IJob>                                       │
│  //   builder.Build() 后手动调度                                │
│                                                                │
└────────────────────────────────────────────────────────────────┘
         │                        ▲
         │ ISchedulerFactory      │ IAgentSchedulerAccessor
         ▼                        │
┌─ Agent Library ─────────────────┴──────────────────────────┐
│                                                             │
│  AgentExtensions.RegisterAgentServices():                    │
│    → 优先级 1: IAgentSchedulerAccessor 显式注册              │
│    → 优先级 2: IScheduler[] 集合                             │
│    → 优先级 3: 单 IScheduler                                │
│    → 优先级 3.5: ISchedulerFactory  ← 新增                   │
│    → 优先级 4: DeferredSchedulerAccessor                    │
│                                                             │
│  QuartzService → IAgentSchedulerAccessor → IScheduler       │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## Risks / Trade-offs

| Risk | Mitigation |
|------|-----------|
| `DeferredSchedulerAccessor` 在 `ISchedulerFactory` 路径前被命中 | 调整优先级顺序：Factory 路径在 Fallback 之前。`DeferredSchedulerAccessor` 只应作为最后兜底 |
| `ISchedulerFactory.GetScheduler()` 在 DI 构建阶段可能尚未就绪 | 使用 try-catch 包裹，失败时降级到 `DeferredSchedulerAccessor` |
| `AddQuartzHostedService` 与 `HostedAgentService` 启动顺序 | `HostedAgentService` 可容忍 Scheduler 未就绪（通过 `DeferredSchedulerAccessor` 重试），无需强制依赖顺序 |
| `appsettings.json` 的 `"Quartz"` 段不再被 `NameValueCollection` 读取 | 不再需要 `NameValueCollection`，Fluent API 直接配置；`"Quartz"` config 段可保留用于未来通过 `QuartzOptions` 绑定 |
| Job 类的 `[QuartzJob]` 和 `[JobParameter]` 属性仍有效 | 这些是 Agent 发现机制，不影响 Quartz 自身的 Job 调度 |

## Open Questions

1. `HostedAgentService` 当前在 `ExecuteAsync` 开始时立即注册。Scheduler 在 `AddQuartzHostedService` 启动后才创建。注册时 Scheduler 可能尚未就绪。需要验证首次 `SchedulerReporterService.ReportAsync()` 调用时 `IAgentSchedulerAccessor` 能否获取到 Scheduler。
   - 预期：`DeferredSchedulerAccessor` 会重试发现，等 `AddQuartzHostedService` 启动后就能解析到 Scheduler。
