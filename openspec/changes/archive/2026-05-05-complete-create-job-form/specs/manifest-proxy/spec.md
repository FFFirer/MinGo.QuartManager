## ADDED Requirements

### Requirement: Platform GET manifest 转发 Agent
当 Platform ManifestController 收到 `GET /api/schedulers/{schedulerName}/manifest` 时，如果内存缓存中没有对应 scheduler 的 manifest 数据，SHALL 通过 `IAgentProxyService` 转发请求到 Agent 实时获取。

#### Scenario: 缓存缺失时转发到 Agent
- **WHEN** ManifestController.Get 被调用，且 `_manifestCache` 中无 `{schedulerName}` 的条目
- **THEN** 调用 `_agentProxy.GetAsync<JobManifestDto>(schedulerName, "agent/manifest")`
- **THEN** 如果 Agent 返回成功，将结果写入 `_manifestCache[{schedulerName}]`
- **THEN** 返回 Agent 返回的 manifest 数据

#### Scenario: 缓存命中时直接返回
- **WHEN** ManifestController.Get 被调用，且 `_manifestCache` 中已有 `{schedulerName}` 的条目
- **THEN** 直接返回缓存的 manifest，不转发到 Agent

#### Scenario: Agent 不可用时优雅降级
- **WHEN** ManifestController 转发到 Agent 且 `IAgentProxyService` 抛出 `AgentException`
- **THEN** 捕获异常，记录警告日志
- **THEN** 返回空的 `JobManifestDto`（与现有行为一致）

### Requirement: 注入 IAgentProxyService 依赖
ManifestController SHALL 通过构造函数注入 `IAgentProxyService` 依赖。

#### Scenario: 构造函数注入
- **WHEN** ManifestController 被实例化
- **THEN** `IAgentProxyService` 通过 DI 注入到 controller
- **THEN** `IAgentProxyService` 被存储为私有字段供 Get 方法使用

### Requirement: POST 上报逻辑不变
ManifestController 的 POST 端点 SHALL 保持现有行为不变，Agent 仍可通过 `POST /api/schedulers/{schedulerName}/manifest` 主动上报 manifest 到缓存。

#### Scenario: Agent 上报
- **WHEN** POST 端点被调用
- **THEN** manifest 存入 `_manifestCache`
- **THEN** 返回 200 OK
