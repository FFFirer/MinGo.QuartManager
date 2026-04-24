# Specification: 生产单实例部署

## Overview

本文档描述 Agent 在生产环境单实例部署的使用场景。

**Target**: 生产环境运维人员

**Environment**: .NET 10.0 + PostgreSQL

---

## ADDED Requirements

### Requirement: 生产环境部署

运维人员 SHALL 能够在生产环境部署单个 Agent 实例。

#### Scenario: 部署准备

- **WHEN** 运维人员准备部署生产环境
- **THEN** 需要配置：
  - Cluster ID（在 Platform 创建）
  - API Token
  - Agent URL（可访问）
  - PostgreSQL 连接字符串

#### Scenario: 启动生产 Agent

- **WHEN** 运维人员启动 Agent 并配置生产模式
- **THEN** Agent 自动向 Platform 注册
- **AND** 心跳服务开始工作
- **AND** Job 状态持久化到数据库

#### Scenario: 验证部署成功

- **WHEN** Agent 启动完成
- **THEN** `/health` 端点返回健康
- **AND** Platform UI 显示 Agent 在线
- **AND** Job 可以正常创建/执行

### Requirement: 持久化 Job 配置

运维人员 SHALL 能够创建持久化的 Job 配置。

#### Scenario: 创建 Cron Job

- **WHEN** 运维人员创建 Cron 表达式 Job
- **THEN** Job 持久化到 PostgreSQL
- **AND** Agent 重启后 Job 仍然存在

#### Scenario: 创建 Interval Job

- **WHEN** 运维人员创建 Interval Job
- **THEN** 按指定间隔执行
- **AND** 持久化配置

### Requirement: 生产监控

运维人员 SHALL 能够在 UI 上监控 Agent 状态。

#### Scenario: 查看 Agent 状态

- **WHEN** 运维人员访问 Platform UI
- **AND** 选择对应 Cluster
- **THEN** 可以看到 Agent 状态（Online/Offline）

#### Scenario: 查看 Job 列表

- **WHEN** 运维人员在 UI 上查看 Jobs
- **THEN** 显示所有 Job 及状态
- **AND** 支持分页和过滤

---

## Implementation Notes

- 跳过 clusterMode（使用单实例）
- 建议使用 Docker 部署
- 生产环境建议配置日志持久化