# Proposal: Agent 自动注册及相关能力

## Why

当前 Agent 需要在启动时自动注册到 Platform，以实现：
- **无人工干预的弹性扩展** - 新增 Agent 实例可自动加入集群
- **动态扩容** - 容器化部署时快速上线
- **快速上手** - 降低配置复杂度

通过本次变更，实现完整的 Agent ↔ Platform 注册与心跳流程，建立可信的集群管理模式。

## What Changes

- **新增** Agent 自动注册流程（启动时触发）
- **新增** 基于 Cluster Token 的注册认证
- **新增** 心跳上报与健康状态计算
- **新增** Agent 生命周期管理（注册 → 心跳 → 注销）
- **可选** Token 轮换能力

### Data Flow

```
Agent 启动 → POST /api/clusters/{clusterId}/agents (Token认证)
        → Platform: 创建/恢复 AgentInstance
        → 返回 AgentId, HeartbeatInterval, Thresholds
        → Agent: 启动 HeartbeatService (每 N 秒)
        → Platform: 更新 LastHeartbeat, 计算状态 (Online/Warning/Offline)
```

### Key Changes

1. **注册认证**: Cluster Token (SHA256 哈希) 验证
2. **AgentInstance 持久化**: Platform DB 存储身份 + 健康信息
3. **心跳协议**: Status + Metrics (JSON) 上报
4. **状态计算**: 30s 内 Online, 30-60s Warning, >60s Offline

## Capabilities

### New Capabilities

- **agent-auto-registration**: Agent 自动注册流程、API 契约、错误处理
- **cluster-token-auth**: Token 生成、绑定、校验、轮换
- **heartbeat-telemetry**: 心跳上报格式、遥测数据、健康计算
- **agent-lifecycle**: 注册 → 心跳 → 注销 → 恢复
- **registration-recovery**: 相同 URL 实例恢复逻辑
- **token-rotation**: Token 轮换策略（可选）

### Modified Capabilities

（无现有能力修改）

## Impact

### Affected Components

- **Platform**: 注册/心跳端点、AgentInstanceService 扩展
- **Agent**: AgentRegistrationService、HeartbeatService
- **UI**: 新增 Agent 状态展示（已有基础）

### Data Contracts

| Request | Response | 说明 |
|---|---|---|
| `CreateAgentRequest` | `AgentRegistrationResponse` | 注册 |
| `AgentHeartbeatRequest` | `AgentHeartbeatResponse` | 心跳 |

## Non-Goals

- 不实现跨集群调度
- 不实现 Agent 间的点对点通信
- 不实现复杂的 RBAC（当前为集群级 Token）

## Migration Plan

1. **Phase 1**: 扩展 Platform 注册/心跳端点 → 已实现
2. **Phase 2**: Agent 注册 + Heartbeat 服务 → 已实现
3. **Phase 3**: 前端 UI 状态展示 → 待完善
4. **Phase 4**: 测试/文档 → 待补充

## Open Questions

- **Q**: Token 是否需要支持过期时间？
Token不需要支持过期时间

- **Q**: 心跳遥测字段是否标准化？
需要标准化

- **Q**: 是否需要 Webhook 告警通知？
暂时不需要