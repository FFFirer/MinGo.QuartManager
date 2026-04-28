## Context

`MinGo.Qap.Agent` 是一个 Quartz.NET Agent 库，宿主应用通过 `AddMinGoAgent()` 注册服务。目前 Agent 的生命周期管理存在以下问题：

- **注册（Registration）**：`IAgentRegistrationService` 已完整实现注册、注销功能，但必须由外部手动触发调用，Agent 启动后不会自动注册
- **心跳（Heartbeat）**：`HeartbeatService` 是一个 `BackgroundService`，但它**从未在 DI 中注册**（`AgentExtensions.cs` 中未添加），属于无效代码
- **生命周期**：没有统一的服务来协调注册→心跳→注销的完整流程

需要新增 `HostedAgentService` 作为 `BackgroundService`，将现有服务编排为自动化的生命周期管理。

### 现有代码分析

| 服务 | 状态 | 说明 |
|------|------|------|
| `IAgentRegistrationService`/`AgentRegistrationService` | ✅ 完整实现 | 注册/注销、重试、Quartz 实例 ID 生成 |
| `HeartbeatService` | ❌ 未注册的 BackgroundService | 心跳构建/发送逻辑可复用，需重构为非 BackgroundService 组件 |
| `HealthCheckService` | ✅ 独立服务 | Scheduler 健康检查，被 Platform 侧调用 |
| `AgentRegistrationInfo` | ✅ 内部状态 | 存储 AgentId、心跳间隔、阈值等注册响应信息 |

## Goals / Non-Goals

**Goals:**
- Agent 启动后**自动注册**到平台，重试直到成功或达到最大次数
- 注册成功后**自动启动心跳循环**，使用平台返回的心跳间隔
- 应用关闭时**优雅注销** Agent 实例
- 心跳失败（401/404/网络错误）时**自动触发重新注册**
- 兼容现有的 `IAgentRegistrationService`、`IQuartzService` 等接口
- `HeartbeatService` 的心跳构建逻辑复用（不变），只是移除其 BackgroundService 身份

**Non-Goals:**
- 不修改 Platform 侧 API（`AgentInstancesController` 等）
- 不修改 `HealthCheckService`（Platform 侧调用，与生命周期无关）
- 不修改 `LogCollectionService`（Timer 驱动，独立运作）
- 不修改配置模型（`AgentConfig`、`AgentSettings`）
- 不涉及 Quartz Scheduler 的创建/初始化（宿主应用负责）

## Decisions

### 1. 架构模式：单个 BackgroundService 编排 vs 两个 BackgroundService 协作

**选择：单个 `HostedAgentService` 统一编排**

| 方案 | 说明 | 结论 |
|------|------|------|
| A) 新增 `HostedAgentService` 独立运行，与现有 `HeartbeatService` 并存 | 两个 BackgroundService 需要协调，逻辑分散 | ❌ 不选 |
| B) 新增 `HostedAgentService` 统一管理注册+心跳，停用旧 `HeartbeatService` 的 BackgroundService | 单一职责、清晰的生命周期，心跳构建逻辑复用 | ✅ **选择** |

**理由**：
- 注册和心跳有强依赖关系（先注册才能心跳），放在一起避免状态同步问题
- 简化 DI 注册（只加一个 HostedService）
- 心跳错误时可直接触发重新注册，无需跨服务通信

### 2. 心跳逻辑复用策略

**选择：提取 `HeartbeatService` 中的心跳构建逻辑为可复用方法，`HostedAgentService` 直接调用**

`HeartbeatService` 中的核心逻辑（`SendHeartbeatAsync`、`BuildHeartbeatRequest`）将提取到 `HostedAgentService` 中内联实现。`HeartbeatService` 类保留但不再作为 `BackgroundService` 注册（可标记为 obsolete 或保留为兼容性引用）。

**理由**：
- 心跳构建逻辑约 50 行代码，提取到独立组件增加不必要的抽象层
- 内联实现让生命周期逻辑完整可见，易于理解和调试
- 旧 `HeartbeatService` 的 `BackgroundService` 基类移除即可断开心跳循环

### 3. 状态管理与重试策略

**设计状态机**：

```
[START] → RegisterAsync()
    ├── 成功 → [REGISTERED] → heartbeat loop
    │       ├── 心跳成功 → [REGISTERED] (继续)
    │       ├── 心跳失败(401/404) → 重新注册
    │       ├── 心跳失败(网络) → 重试心跳
    │       └── Cancellation → DeregisterAsync() → [STOPPED]
    └── 失败 → 重试直到 maxAttempts，全部失败 → [FAILED]（记录错误，不阻止应用启动）
```

- 重试注册使用 `AgentSettings.RegistrationMaxAttempts` 和 `RegistrationRetryDelaySeconds`
- 心跳失败后不立即重试，等待下一个心跳周期
- 连续 N 次心跳失败后触发重新注册

### 4. 心跳间隔来源优先级

```
平台返回的 HeartbeatIntervalSeconds (来自注册响应)
    → 配置中的 HeartbeatIntervalSeconds (AgentSettings)
    → 默认值 30 秒
```

平台响应中 `AgentRegistrationResponse.HeartbeatIntervalSeconds` 优先级最高，支持平台动态调整心跳频率。

### 5. 组件依赖关系

```
HostedAgentService (BackgroundService)
  ├── IAgentRegistrationService  → 注册/注销
  ├── IQuartzService             → 获取 Scheduler 状态（心跳数据）
  ├── IHttpClientFactory         → HTTP 请求
  ├── IConfiguration             → 配置读取
  └── ILogger<HostedAgentService>
```

### 6. 生命周期钩子 (IHostedService)

`BackgroundService` 基类提供 `ExecuteAsync`（启动循环）和 `StopAsync`（取消令牌后清理）：

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    // Phase 1: Register
    await RegisterAsync(stoppingToken);
    
    // Phase 2: Heartbeat loop (only if registered)
    while (!stoppingToken.IsCancellationRequested && _state == AgentState.Registered)
    {
        await SendHeartbeatAsync(stoppingToken);
        await Task.Delay(_heartbeatInterval, stoppingToken);
    }
}

public override async Task StopAsync(CancellationToken cancellationToken)
{
    // Phase 3: Deregister gracefully
    await DeregisterAsync();
    await base.StopAsync(cancellationToken);
}
```

### 7. 重试退避策略

| 场景 | 策略 |
|------|------|
| 首次注册失败 | 固定间隔重试（`config.RegistrationRetryDelaySeconds`），最多 `config.RegistrationMaxAttempts` 次 |
| 心跳失败后重新注册 | 立即触发重新注册，同样使用上述重试参数 |
| 心跳网络超时 | 不重试，等待下一个周期（`HeartbeatIntervalSeconds`） |

### 8. 日志与可观测性

- 注册成功/失败、心跳成功/失败、注销等关键事件记录 `Information`/`Error` 级别日志
- 心跳成功记录 `Debug` 级别（避免日志淹没）
- 每次状态转换记录 `Warning` 级别

## Risks / Trade-offs

| 风险 | 缓解措施 |
|------|---------|
| 注册阻塞应用启动 | `ExecuteAsync` 执行注册，若注册失败（全部重试用完）则记录日志，不影响应用其他功能，心跳循环跳过 |
| 注销失败导致 Platform 侧留下僵尸 Agent | Platform 侧应通过心跳超时（OfflineThresholdSeconds）判断 Agent 离线，注销非必需 |
| 频繁心跳失败导致大量重新注册请求 | 重新注册使用指数退避（1x, 2x, 4x... 重试间隔，最多 maxAttempts） |
| 与宿主应用的 Quartz Scheduler 初始化时序冲突 | `HostedAgentService` 在应用启动后运行，Quartz Scheduler 已在 `Program.cs` 中初始化完成；若需确保顺序，可在 `ExecuteAsync` 开始时增加短暂延迟 |
| 提取心跳逻辑后 `HeartbeatService` 类用途模糊 | 保留类但标记 `[Obsolete]`，在 `HostedAgentService` 中内联心跳逻辑；后续版本可完全移除 |
