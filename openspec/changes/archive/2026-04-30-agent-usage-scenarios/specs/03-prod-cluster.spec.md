# Specification: 生产集群部署

## Overview

本文档描述 Agent 在生产环境集群模式下的使用场景。

**Target**: 生产环境运维人员

**Environment**: 多 Agent 实例 + PostgreSQL + Quartz 集群

---

## ADDED Requirements

### Requirement: 集群部署架构

运维人员 SHALL 能够部署高可用的 Agent 集群。

#### Scenario: 集群架构说明

- **WHEN** 运维人员配置集群模式
- **THEN** 多个 Agent 实例共享同一个 PostgreSQL
- **AND** Quartz 自动进行负载均衡
- **AND** 一个实例故障时自动转移

#### Scenario: 配置集群模式

- **WHEN** 运维人员配置 `agent.clusterMode: true`
- **AND** 配置 PostgreSQL 连接字符串
- **THEN** Quartz 使用 AdoJobStore
- **AND** `quartz.jobStore.clustered: true`

#### Scenario: 启动多个实例

- **WHEN** 运维人员启动多个 Agent 实例
- **AND** 使用相同的 Cluster ID
- **THEN** 所有实例注册到同一个 Cluster
- **AND** Platform 显示多个在线实例

### Requirement: 高可用故障转移

运维人员 SHALL 能够理解自动故障转移机制。

#### Scenario: 实例故障检测

- **WHEN** 一个 Agent 实例崩溃
- **THEN** Quartz 集群检测到实例离线
- **AND** 其他实例接管待执行的 Jobs

#### Scenario: 手动故障转移

- **WHEN** 运维人员需要重启实例
- **THEN** Job 自动转移到健康实例
- **AND** 无需手动干预

### Requirement: 集群监控

运维人员 SHALL 能够在 UI 上监控集群状态。

#### Scenario: 查看集群实例

- **WHEN** 运维人员在 UI 上查看 Cluster
- **THEN** 显示所有实例及状态
- **AND** 显示实例数量和健康状态

---

## Implementation Notes

- 至少需要 2 个实例保证高可用
- 建议使用负载均衡器
- 数据库连接池配置重要