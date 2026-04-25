## Context

当前 Agent 项目 (`MinGo.Qap.Agent`) 是一个 .NET 10 类库，提供 Quartz.NET 的扩展能力和 Platform 集成。核心组件包括：

- `QuartzService`：封装 Scheduler CRUD 操作（已实现）
- `AgentRegistrationService`：Platform 注册/心跳（已实现，但 URL 解析简单）
- `JobDiscoveryService`：程序集扫描（已实现，但缺少参数元数据）
- `LogCollectionService`：日志缓冲上传（已实现，但缺少自动触发源）

宿主应用（如 `Sample.Agent`）需要自行实现 Controller 来暴露这些能力，导致重复代码和集成不一致。

## Goals / Non-Goals

**Goals:**
- 提供标准化的 Minimal API 扩展，一行代码启用完整 REST API
- 实现基于自定义 Attribute 的 Job 参数元数据自动发现
- 实现多环境自适应的 Agent URL 解析（K8s / Docker / VM）
- 实现 JobListener 驱动的执行日志自动收集
- 保持 Library 形态，不强制改变宿主应用的部署方式

**Non-Goals:**
- 不实现 Quartz Scheduler 初始化（由宿主应用负责）
- 不引入新的外部依赖（仅使用 ASP.NET Core 内置能力）
- 不修改 Platform 端的 API 契约或数据库 Schema
- 不实现复杂的负载均衡或 Agent 间协调

## Decisions

### 1. Minimal API 而非 Controller 基类

**选择**：使用 `IEndpointRouteBuilder.MapGroup()` 提供扩展方法。

**理由**：
- 宿主应用只需 `app.MapMinGoAgentApi()` 一行代码，零样板
- Minimal API 性能更优，中间件管道更轻量
- 避免继承污染，宿主应用可自由添加自定义端点

**替代方案**：提供 `AgentJobsControllerBase` 抽象类。
- 拒绝原因：增加宿主应用的 MVC 依赖，且无法与 Minimal API 混合使用。

### 2. 自定义 Attribute 而非 XML 注释

**选择**：引入 `[JobParameter]` 和 `[QuartzJob]` 特性。

**理由**：
- 编译时安全，支持重构重命名
- 可通过反射在运行时高效读取
- 与现有 `QuartzJobAttribute` 保持命名空间一致

**替代方案**：使用 XML 文档注释 + Source Generator。
- 拒绝原因：Source Generator 增加构建复杂度，XML 注释在运行时读取不可靠。

### 3. AgentUrlResolver 分层策略

**选择**：按优先级链式检测（配置 → 环境变量 → K8s → Docker → 网卡 → 本地）。

**理由**：
- 覆盖主流部署场景，无需手动配置
- 每层检测独立，可单独覆盖和测试
- 与现有 `AgentSettings` 配置对象兼容

**替代方案**：使用服务发现（Consul / Eureka）。
- 拒绝原因：引入运维复杂性，与项目轻量化原则冲突。

### 4. Agent 不初始化 Quartz Scheduler

**选择**：Agent 库仅提供扩展能力，不创建或启动 Quartz Scheduler。

**理由**：
- 保持 Library 形态，宿主应用完全控制 Scheduler 生命周期
- 避免与宿主应用现有的 Quartz DI 扩展（如 `Quartz.Extensions.DependencyInjection`）冲突
- 宿主应用可自行选择 JobStore（RAM / ADO.NET / 自定义）

**影响**：
- 宿主应用负责调用 `StdSchedulerFactory` 或 `Quartz.Extensions.Hosting` 初始化 Scheduler
- `QapJobListener` 仍由 Agent 提供，但需宿主应用手动注册到 `ListenerManager`
- `MapMinGoAgentApi()` 接收 `IScheduler` 依赖，假设已由宿主应用注册到 DI

### 5. JobListener 驱动日志收集

**选择**：实现 `IJobListener` 接口，由宿主应用在 Scheduler 初始化时注册。

**理由**：
- 完全解耦，不侵入 Job 业务代码
- 利用 Quartz 原生生命周期事件，无性能损耗
- 复用现有 `ILogCollectionService` 缓冲机制

**替代方案**：在 `QuartzService` 方法中手动记录。
- 拒绝原因：无法捕获 Trigger 直接触发或 Scheduler 内部触发的执行。

## Risks / Trade-offs

| Risk | Mitigation |
|------|-----------|
| Minimal API 与宿主应用现有路由冲突 | 使用 `/api/agent` 前缀，提供可选的 `groupPrefix` 参数 |
| Attribute 反射扫描影响启动性能 | 扫描结果缓存到 `IJobRegistry`，仅在配置变更时刷新 |
| K8s POD_IP 可能不可跨 Namespace 访问 | 优先检测 `AGENT_URL` 环境变量，允许显式覆盖 |
| JobListener 异常可能导致 Scheduler 不稳定 | Listener 内部 try-catch，异常仅记录不上抛 |
| 参数类型映射不完整（自定义类） | 支持 `[JobPayload]` 标记复杂对象，序列化为 JSON Schema |

## Migration Plan

1. **代码集成**：Agent 项目添加新文件，无破坏性变更
2. **宿主应用迁移**：
   - 移除自定义 Controller（如 `Sample.Agent.Controllers.JobsController`）
   - 在 `Program.cs` 添加 `app.MapMinGoAgentApi()`
3. **Job 类迁移**：为现有 Job 添加 `[JobParameter]` 特性（可选，不强制）
4. **配置迁移**：可选添加 `externalUrl` / `networkInterface` 配置项

## Open Questions

- `ExecutionLogDto` 是否需要扩展字段（如执行时长、触发器类型）？
- `MapMinGoAgentApi` 是否需要提供 `RouteGroupBuilder` 选项委托供宿主自定义？
