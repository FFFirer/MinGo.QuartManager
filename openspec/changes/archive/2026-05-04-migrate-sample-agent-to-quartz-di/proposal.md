## Why

Sample.Agent 当前使用手动 `StdSchedulerFactory` + 自定义 `DIJobFactory` 的方式初始化 Quartz.NET，这与 Quartz 官方推荐的 Microsoft DI 集成模式严重偏离。项目中已引用 `Quartz.Extensions.DependencyInjection` 但完全未使用，同时缺失 `Quartz.Extensions.Hosting`。这种手工拼接方式导致：

1. Scheduler 生命周期游离于 ASP.NET Core 托管系统之外
2. 冗余的自定义代码（`DIJobFactory`）替代了 Quartz 3.3.2+ 内置的 DI 解析能力
3. 配置分散在 `NameValueCollection` 和 `appsettings.json` 之间，类型不安全
4. 无法利用 `AddQuartzHostedService()` 的优雅关停机制

迁移到官方推荐模式将消除技术债务，使 Sample.Agent 成为后续 Agent 开发者的正确参考实现。

## What Changes

- **添加 NuGet 包**: 为 Sample.Agent 和 MinGo.Qap.Agent 添加 `Quartz.Extensions.Hosting`
- **重构 Scheduler 初始化**: 将 `new StdSchedulerFactory(properties)` 手动模式替换为 `services.AddQuartz(q => { ... })` Fluent API
- **替换生命周期管理**: 将手动 `scheduler.Start()` 替换为 `services.AddQuartzHostedService()`
- **移除自定义 DIJobFactory**: 删除内联 `DIJobFactory` 类，利用 Quartz 3.3.2+ 内置的 DI 容器解析
- **迁移 Job 调度**: 将 `builder.Build()` 之后的手动 `JobBuilder.Create` + `scheduler.AddJob` 移入 `AddQuartz()` 回调
- **简化 Job DI 注册**: 移除冗余的 `services.AddTransient<HelloJob>()`（DI 容器自动解析）
- **适配 Agent Accessor**: 确保 `IAgentSchedulerAccessor` 能通过 `ISchedulerFactory` 获取 Scheduler 实例
- **代码清理**: 移除不再需要的 `Quartz.Impl`、`Quartz.Simpl`、`Quartz.Spi` using

## Capabilities

### New Capabilities
- _(无新 capability — 本次变更是现有 Sample.Agent 基础设施的现代化迁移)_

### Modified Capabilities
- `system-architecture`（可选）: Sample Agent 层级的 Quartz 初始化方式变更，属于架构模式更新
- `yaml-config-provider`: 确保 Quartz 配置的属性能正确从 `config.yaml` / `appsettings.json` 载入

## Impact

| 文件 | 变更类型 | 说明 |
|------|---------|------|
| `samples/Sample.Agent/Program.cs` | **重构** | 核心改造 — 替换整个 Quartz 初始化代码块 |
| `samples/Sample.Agent/Sample.Agent.csproj` | 修改 | 添加 `Quartz.Extensions.Hosting` 包引用 |
| `src/MinGo.Qap.Agent/MinGo.Qap.Agent.csproj` | 修改 | 版本对齐（3.17.1→3.18.1）+ 添加 `Quartz.Extensions.Hosting` |
| `src/MinGo.Qap.Agent/AgentExtensions.cs` | 评估 | `RegisterSchedulerAccessor()` 需确认能否通过 `ISchedulerFactory` 解析 |
| `samples/Sample.Agent/appsettings.json` | 无变更 | Quartz 配置段保留但不再被 `NameValueCollection` 读取 |
