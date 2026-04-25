## Why

当前 Agent 项目作为 Library 提供 Quartz.NET 扩展能力，但缺少标准化的 HTTP API 暴露方式、Job 参数元数据发现机制、执行日志自动收集能力，以及多环境自适应的 Agent 地址解析。这导致：

1. 每个宿主应用需要自行实现 Controller 来暴露 Agent 能力，重复造轮子
2. Platform 无法通过统一的 API 契约与 Agent 交互，集成成本高
3. Job 执行历史无法自动收集和上报，运维可观测性不足
4. Agent 在容器/K8s 环境下的地址上报依赖手动配置，易出错

## What Changes

- **新增 Agent Minimal API 扩展**：提供 `MapMinGoAgentApi()` 扩展方法，一行代码启用完整的 REST API（Job CRUD、Trigger/Pause/Resume、Scheduler 状态、Manifest 查询）
- **新增 JobParameterAttribute**：自定义特性标记 Job 参数，支持名称、描述、必填、默认值、验证规则，用于参数 Schema 自动发现和 UI 动态生成
- **新增 AgentUrlResolver**：分层环境检测策略（K8s Downward API → Docker → 网卡绑定 → 本地回退），自动推导 Agent 可访问地址
- **新增 QapJobListener**：实现 `IJobListener` 接口，自动拦截 Job 执行生命周期事件，驱动 `ILogCollectionService` 完成执行日志收集
- **增强 JobDiscoveryService**：集成 Attribute 反射扫描，从 `[JobParameter]` 标记的属性/构造函数参数中提取完整的 `ParameterInfoDto` 列表

## Capabilities

### New Capabilities

- `agent-minimal-api`：Agent HTTP API 标准化暴露（Minimal API 风格）
- `job-parameter-discovery`：基于自定义 Attribute 的 Job 参数元数据自动发现
- `agent-auto-registration`：多环境自适应的 Agent 地址解析与 Platform 注册
- `execution-log-collection`：基于 JobListener 的执行日志自动收集与上报

### Modified Capabilities

- `configuration-management`：新增 Agent URL 相关配置项（`externalUrl`、`networkInterface`）

## Impact

- **Agent 项目**：新增 `AgentApiExtensions.cs`、`AgentUrlResolver.cs`、`QapJobListener.cs`，修改 `JobDiscoveryService.cs`、`AgentRegistrationService.cs`
- **Shared 项目**：新增 `JobParameterAttribute.cs`、`QuartzJobAttribute.cs` 到 `Attributes` 命名空间
- **Platform 项目**：无直接影响，沿用现有 `AgentProxyService` HTTP 调用契约
- **宿主应用**：可移除自定义 Controller，改为 `app.MapMinGoAgentApi()` 一行启用
- **依赖**：无新增 NuGet 包，仅使用 ASP.NET Core 内置 Minimal API 和 Quartz 已有接口
