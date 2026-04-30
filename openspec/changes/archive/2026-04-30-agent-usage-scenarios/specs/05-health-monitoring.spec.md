# Specification: Agent 健康监控

## Overview

本文档描述 Agent 健康监控和告警配置。

**Target**: 运维人员

---

## ADDED Requirements

### Requirement: Agent 状态检测

平台 SHALL 能够检测 Agent 的健康状态。

#### Scenario: Online 状态

- **WHEN** Agent 心跳在 30 秒内
- **THEN** 状态显示为 Online (🟢)

#### Scenario: Warning 状态

- **WHEN** 心跳超过 30 秒但小于 60 秒
- **THEN** 状态显示为 Warning (🟡)

#### Scenario: Offline 状态

- **WHEN** 心跳超过 60 秒
- **THEN** 状态显示为 Offline (🔴)

### Requirement: 心跳服务

平台 SHALL 接收并处理 Agent 心跳。

#### Scenario: 接收心跳

- **WHEN** Agent 调用 `POST /api/agents/{agentId}/heartbeat`
- **AND** 提供状态和指标
- **THEN** 记录心跳时间
- **AND** 更新状态

#### Scenario: 心跳包含信息

- **WHEN** Agent 发送心跳
- **THEN** 包含：
  - Agent ID
  - Status (Running/Standby)
  - Metrics (内存、CPU、Job 统计)
  - Timestamp

### Requirement: UI 状态显示

平台 SHALL 在 UI 上显示 Agent 状态。

#### Scenario: Cluster 页面状态

- **WHEN** 运维人员查看 Cluster 概览
- **THEN** 显示实例数量和状态统计

#### Scenario: 实例详情页

- **WHEN** 运维人员查看实例详情
- **THEN** 显示：
  - 最后心跳时间
  - 运行时间
  - Job 执行统计

### Requirement: 告警配置

平台 SHALL 支持配置告警阈值。

#### Scenario: 配置 Warning 阈值

- **WHEN** 管理员设置 Warning 阈值（秒）
- **THEN** 超过阈值显示 Warning

#### Scenario: 配置 Offline 阈值

- **WHEN** 管理员设置 Offline 阈值（秒）
- **THEN** 超过阈值标记为 Offline

---

## Implementation Notes

- 告警可通过 Webhook 扩展
- 当前硬编码阈值（30s/60s）
- 考虑配置化告警阈值