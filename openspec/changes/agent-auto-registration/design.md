# Design: Agent 自动注册及相关能力

## Context

### Background

当前系统需要支持 Agent 自动注册到 Platform，实现无人工干预的弹性扩展。现有实现已具备基础能力，本设计文档用于记录技术决策和实现细节。

### Current State

**已实现组件**:

| Component | Status | Location |
|---|---|---|
| Platform 注册端点 | ✅ 完成 | `Controllers/AgentInstancesController.cs` |
| Platform 心跳端点 | ✅ 完成 | `Controllers/AgentInstancesController.cs` |
| AgentRegistrationService | ✅ 完成 | `Services/AgentInstanceService.cs` |
| Agent 注册服务 (Agent端) | ✅ 完成 | `Services/AgentRegistrationService.cs` |
| HeartbeatService (Agent端) | ✅ 完成 | `Services/HeartbeatService.cs` |
| Token 认证 | ✅ 完成 | SHA256 哈希验证 |

**当前数据流**:

```
Agent 启动 → Program.cs (ApplicationStarted)
        → AgentRegistrationService.RegisterAsync()
        → POST /api/clusters/{clusterId}/agents
        → Platform: AgentInstanceService.RegisterAgentAsync()
        ← 返回 AgentId, HeartbeatInterval
        → 启动 HeartbeatService (BackgroundService)
        → 每 30s POST /api/agents/{agentId}/heartbeat
        → Platform: UpdateLastHeartbeat()
        → 状态计算 (Online/Warning/Offline)
```

**约束**:

- Cluster Token 存储为 SHA256 哈希（不可逆）
- 心跳阈值硬编码: 30s (Warning), 60s (Offline)
- Agent 实例 URL 必须唯一（同集群内）

**Stakeholders**:

- 运维人员：配置 Token、启动 Agent
- Platform：接收注册、监控健康
- UI：展示 Agent 状态

## Goals / Non-Goals

### Goals

- [x] Agent 启动时自动注册到 Platform
- [x] 基于 Cluster Token 的认证
- [x] 周期性心跳上报
- [x] 健康状态计算 (Online/Warning/Offline)
- [x] 相同 URL 实例恢复

### Non-Goals

- 不实现 Token 过期时间
- 不实现 Webhook 告警通知
- 不实现复杂 RBAC
- 不实现跨集群调度

## Decisions

### 1. 注册认证方式

**Decision**: Cluster Token + SHA256 哈希存储

**Rationale**:
- 现有实现使用 SHA256 哈希存储 Token，Agent 端存储明文
- 简单易实现，无需额外密钥管理服务

**Alternatives Considered**:
- JWT Token: 需要额外库，增加复杂度
- mTLS: 需要证书管理，更适合生产环境

### 2. 心跳数据结构

**Decision**: 固定 JSON 结构 + 可扩展 Metrics

```json
{
  "agentId": "agt-xxxx",
  "quartzInstanceId": "cls-xxx-hostname-xxxx",
  "agentVersion": "1.0.0",
  "status": "Running",
  "metrics": "{ memory: xxx, cpu: yyy }"
}
```

**Rationale**:
- 简单易懂，便于调试
- Metrics 字段可扩展（JSON 字符串）

### 3. 健康状态计算

**Decision**: 固定阈值 (30s/60s)

| Threshold | Status | Color |
|---|---|---|
| ≤ 30s | Online | 🟢 |
| 30-60s | Warning | 🟡 |
| > 60s | Offline | 🔴 |

**Rationale**:
- 简单直接，无需配置
- 可后续改为可配置

### 4. 重复注册处理

**Decision**: 相同 URL + 未删除 → 拒绝；相同 URL + 已删除 → 恢复

**Rationale**:
- 防止意外创建重复实例
- 支持实例重启后的自动恢复

## Risks / Trade-offs

### Risk 1: Token 明文存储

[Risk] → Agent config.yaml 存储明文 Token，泄露风险
[Mitigation] → 生产环境使用 Vault 或加密配置

### Risk 2: 心跳数据泄露

[Risk] → Metrics JSON 可能包含敏感信息
[Mitigation] → 仅内部网络传输，配置 HTTPS

### Risk 3: 状态计算不准确

[Risk] → 时钟不同步导致误判
[Mitigation] → 使用 UTC 时间，NTP 同步

### Risk 4: 实例 URL 冲突

[Risk] → 不同机器相同 URL 导致注册失败
[Mitigation] → 使用 hostname 生成唯一 URL

## Migration Plan

### Phase 1: 注册端点扩展 (已完成)

- Platform: 注册/心跳端点
- Agent: 注册服务

### Phase 2: 完善 (待讨论)

- UI: Agent 状态卡片组件
- 配置化阈值

### Phase 3: 安全增强 (可选)

- Token 加密存储
- mTLS

## Open Questions

1. **Q**: 心跳遥测需要收集哪些指标？
   - **A**: 内存、CPU、Job 统计（当前实现）

2. **Q**: 是否需要支持 Agent 主动注销？
   - **A**: 已有 deregister 接口，可扩展

3. **Q**: 集群模式下 Token 如何同步？
   - **A**: 各 Agent 配置相同 Token即可

4. **Q**: 是否需要实现 Token 自动轮换？
   - **A**: 暂不在本次范围内