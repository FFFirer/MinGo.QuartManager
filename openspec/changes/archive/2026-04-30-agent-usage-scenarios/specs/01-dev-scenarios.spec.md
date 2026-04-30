# Specification: Agent 开发环境使用场景

## Overview

本文档描述 Agent 在开发/测试环境下的使用场景。

**Target**: 开发人员本地调试

**Environment**: .NET 10.0 + SQLite/内存

---

## ADDED Requirements

### Requirement: 开发环境下快速启动 Agent

开发人员 SHALL 能够在本地快速启动 Agent 进行调试。

#### Scenario: 本地开发环境启动

- **WHEN** 开发人员运行 `dotnet run --project src/MinGo.Qap.Agent`
- **THEN** Agent 使用 RAMJobStore 启动
- **AND** HTTP 服务监听配置端口
- **AND** `/health` 端点返回健康状态

#### Scenario: 配置开发环境

- **WHEN** 开发人员配置 `config.yaml` 以开发模式
- **THEN** `quartz.properties` 使用 RAMJobStore
- **AND** 不需要外部数据库

### Requirement: 本地 Job 测试

开发人员 SHALL 能够本地创建和测试 Job。

#### Scenario: 创建测试 Job

- **WHEN** 开发者在 `config.yaml` 中配置 `jobTypes`
- **AND** 启动 Agent
- **THEN** Job 类型在 `/api/jobs/manifest` 中可见
- **AND** 可以通过 API 创建 Job

#### Scenario: 手动触发 Job

- **WHEN** 开发者调用 `POST /api/jobs/{jobKey}/trigger`
- **THEN** Job 立即执行
- **AND** 执行结果可从日志查看

### Requirement: 开发环境 UI 操作

开发人员 SHALL 能够在 UI 上操作 Job。

#### Scenario: UI 访问开发环境

- **WHEN** 开发者访问 `http://localhost:5173` (UI Dev Server)
- **AND** Platform 运行在 `http://localhost:5000`
- **THEN** UI 正常显示 Dashboard

#### Scenario: UI 创建 Job

- **WHEN** 开发者在 UI 上填写 Job 表单
- **AND** 点击 "Create"
- **THEN** Job 被创建到本地 Agent
- **AND** Job 列表更新显示新 Job

### Requirement: 开发调试日志

开发人员 SHALL 能够查看详细的调试日志。

#### Scenario: 查看 Agent 日志

- **WHEN** Agent 运行在 development 模式
- **THEN** 控制台输出详细调试日志
- **AND** 日志级别可配置

---

## Implementation Notes

- 使用 `appsettings.Development.json` 配置日志级别
- 默认端口 8080，可通过环境变量覆盖
- 不需要 Platform 注册（跳过注册步骤）