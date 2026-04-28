## Why

Agent 实例在启动后需要自动向平台注册，并在运行期间持续发送心跳以维持在线状态。目前注册（`AgentRegistrationService`）和心跳（`HeartbeatService`）是独立的服务，缺乏统一的生命周期管理：注册需要外部手动触发，心跳服务无法感知注册状态，且没有优雅的注销机制。这导致 Agent 在部署后必须通过额外步骤完成注册，降低了自动化程度和可靠性。

## What Changes

- **新增 `HostedAgentService`**：一个 `BackgroundService`，作为 Agent 生命周期管理的统一入口，按顺序执行：启动注册 → 定时心跳 → 关闭注销
- **自动注册流程**：Agent 启动时自动调用 `AgentRegistrationService.RegisterAsync()`，成功后持有注册信息
- **定时心跳循环**：注册成功后，按平台返回的心跳间隔定时发送心跳，支持心跳间隔动态调整
- **优雅注销**：应用程序关闭时自动调用 `AgentRegistrationService.DeregisterAsync()` 完成注销
- **故障恢复**：心跳失败时自动触发重新注册，确保 Agent 在网络恢复后可重新上线
- **注册集成**：将 `HostedAgentService` 注册到 DI 容器，替换（或协调）现有的 `HeartbeatService` BackgroundService
- **不破坏现有 API**：保持 `IAgentRegistrationService`、`HeartbeatService`、`AgentApiExtensions` 等现有接口不变

## Capabilities

### New Capabilities
- `agent-lifecycle`: Agent 生命周期管理，包含自动注册、定时心跳、优雅关闭和故障恢复

### Modified Capabilities

*(无现有 spec 需要修改)*

## Impact

- **新增文件**: `src/MinGo.Qap.Agent/Services/HostedAgentService.cs`
- **修改文件**: `src/MinGo.Qap.Agent/AgentExtensions.cs` — 在 `AddMinGoAgent` 中注册 `HostedAgentService`
- **修改文件**: `src/MinGo.Qap.Agent/AgentExtensions.cs` — 不再自动注册 `HeartbeatService` 为托管服务（由 `HostedAgentService` 协调）
- **影响项目**: 仅 `MinGo.Qap.Agent` 项目
- **影响示例**: `samples/Sample.Agent/` — 无需修改，升级 Agent NuGet 即可获得自动注册能力
