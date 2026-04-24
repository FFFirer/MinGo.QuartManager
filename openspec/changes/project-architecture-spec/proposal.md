# 项目整体架构设计规范

## 概述

本文档定义 MinGo.QuartzManager 项目的整体架构设计规范，作为项目开发的基础规范，防止后续实现超出定义范围。

### 项目目标

- 定时任务调度执行由 **Quartz.NET** 提供
- 实现定时任务的**可视化管理**
- **自动发现**应用中存在的 Job 定义
- 支持手动创建 Job
- 控制调度策略（cron/interval/once）
- 管理 Job 的启停
- Job 执行参数的设置
- Job 执行日志记录

### 核心组件

项目整体分为两部分：

| 组件 | 定位 |
|------|------|
| **Platform** | 集中管理所有操作及数据展示 |
| **Agent** | 集成在 Quartz.NET 的 Job Runner 中，提供扩展能力 |

---

## 架构设计

### 整体架构图

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           整体架构                                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                     │
│   ┌─────────────────────────────────────────────────────────────┐       │
│   │                      Platform                             │       │
│   │   • 用户 Web API (Controllers)                          │       │
│   │   • JobDefinition 元数据备份 (PostgreSQL)               │       │
│   │   • 执行日志聚合存储 (PostgreSQL)                         │       │
│   │   • Cluster/AgentInstance 管理                          │       │
│   │   • Agent 代理转发 (AgentProxyService)                  │       │
│   └────────────────────────┬──────────────────────────────┘       │
│                            │ HTTP/REST 契约                      │
│   ┌────────────────────────┼──────────────────────────────┐       │
│   │              Cluster (执行器组)                           │       │
│   │  ┌─────────┐  ┌─────────┐  ┌─────────┐             │       │
│   │  │ Agent 1 │  │ Agent 2 │  │ Agent N │  (相同能力) │       │
│   │  └────┬────┘  └────┬────┘  └────┬────┘             │       │
│   └──────┼─────────────┼─────────────┼────────────────────┘       │
│          │             │             │                              │
│          ▼             ▼             ▼                              │
│   ┌─────────────────────────────────────────────┐              │
│   │         Quartz.NET (各自独立的 DB)           │              │
│   │  • QRTZ_JOB_DETAILS                        │              │
│   │  • QRTZ_TRIGGERS                          │              │
���   │  • QRTZ_FIRED_TRIGGERS                   │              │
│   │  • QRTZ_SCHEDULER_STATE                  │              │
│   └─────────────────────────────────────────────┘              │
└─────────────────────────────────────────────────────────────────┘
```

### 设计原则

| # | 原则 | 说明 |
|---|------|------|
| 1 | **调度执行由 Quartz.NET 提供** | Agent 只做 wrapper，不修改 Quartz 核心 |
| 2 | **Agent 是 Quartz.NET 的扩展层** | 提供额外能力（Job发现、日志收集）的 NuGet 包/SDK |
| 3 | **数据隔离** | Quartz(DB) ⟂ Platform(DB)，两者独立，通过 REST 契约通信 |
| 4 | **Cluster 是执行器组** | 具有同一批任务执行能力的 Agent 实例组 |
| 5 | **Agent 是执行器实例** | 向 Platform 注册，提供数据契约标准及数据提供者 |

---

## 组件职责

### Platform

| 职责 | 说明 |
|------|------|
| 用户 Web API | 面向用户的 CRUD 操作接口 |
| 数据聚合 | JobDefinition 备份、执行日志聚合存储 |
| Agent 管理 | 注册、心跳监控、状态管理 |
| **不直接操作 Quartz** | 通过 Agent 代理转发 |

**核心约束**：Platform 不直接连接 Quartz Scheduler，所有调度操作通过 Agent REST API 代理。

### Agent（SDK/NuGet）

| 职责 | 说明 |
|------|------|
| Job 发现 | 程序集扫描，自动发现 IJob 实现 |
| Job 注册 | 将发现的 Job 注册到 Quartz |
| 调度执行 | 通过 Quartz.NET 执行 |
| 日志收集 | 标准收集逻辑，上报到 Platform |
| Quartz 数据 | 独立数据库，与 Platform 无关 |

**核心约束**：Agent 不提供用户 Web UI（只提供内部 API），不直接暴露给用户。

### Cluster

| 定义 | 说明 |
|------|------|
| 定位 | 具有同一批任务执行能力的 Agent 实例组 |
| 目的 | 负载均衡 + 故障 failover |
| 路由 | Platform 根据策略选择 Agent 执行 |

---

## 核心机制

### 1. Job 自动发现（程序集扫描）

```
应用程序引用 
    │
    ▼
[MinGo.Agent.SDK] 
    │
    ├── IJob 实现类扫描（程序集扫描）
    │   ├── 扫描引用程序集中的所有 IJob 实现
    │   ├── 提取 Job 元数据（Group、Key、Description）
    │   └── 生成 JobManifest
    │
    ▼
JobRegistry (Manifest)
    │
    ├── Key: "PrintJob"
    ├── Type: "Sample.Jobs.PrintJob"
    ├── Parameters: [...]
    │
    ▼
Platform 发现可用的 Job 类型列表
```

### 2. 执行日志设计

```
Job 执行流程
    │
    ├── IJob.Execute(IJobExecutionContext)
    │
    ├── 执行前：记录开始时间
    │
    ├── 执行中：日志收集器收集
    │   ├── stdout/stderr
    │   ├── 自定义日志
    │   └── 异常信息
    │
    ├── 执行后：上报到 Platform
    │   POST /api/clusters/{clusterId}/agents/{agentId}/logs
    │   {
    │     "jobKey": "print-job",
    │     "fireInstanceId": "...",
    │     "startTime": "...",
    │     "endTime": "...",
    │     "status": "success|failed",
    │     "output": "...",
    │     "error": "..."
    │   }
    │
    ▼
Platform 存储执行日志
```

### 3. REST 契约接口

#### Platform → Agent

| 方法 | 路径 | 说明 |
|------|------|------|
| POST | /jobs | 创建 Job |
| PUT | /jobs/{key} | 更新 Job |
| DELETE | /jobs/{key} | 删除 Job |
| POST | /jobs/{key}/trigger | 手动触发 |
| POST | /jobs/{key}/pause | ��停 Job |
| POST | /jobs/{key}/resume | 恢复 Job |

#### Agent → Platform

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | /manifest | 获取 Job 列表 |
| POST | /logs | 上报执行日志 |
| POST | /heartbeat | 心跳 |

---

## 数据模型

### 数据库隔离

| 数据库 | 用途 | 拥有者 |
|--------|------|--------|
| Platform DB | JobDefinition 备份、执行日志聚合 | Platform |
| Quartz DB | 调度执行数据（QRTZ_* 表） | Agent（每个实例独立） |

**核心约束**：两个数据库完全独立，Platform 只能通过 REST API 获取调度状态，不能直接访问 Quartz 表。

---

## 边界约束（防止超出范围）

为防止后续实现超出定义范围，明确以下约束：

| # | 约束 | 说明 |
|---|------|------|
| 1 | Platform 不直接操作 Quartz | 所有操作通过 Agent 代理 |
| 2 | Agent 不提供用户 UI | 只有内部 REST API |
| 3 | Quartz DB 不暴露给 Platform | 只通过日志上报获取状态 |
| 4 | Platform DB 不用于调度执行 | 只是元数据备份 |
| 5 | 调度策略不自定义 | 只支持 cron/interval/once 三种 |
| 6 | Job 发现只支持程序集扫描 | 不支持运行时动态注册 |

---

## 技术选型

| 组件 | 技术 | 版本 |
|--------|------|------|
| 调度框架 | Quartz.NET | 3.17.1 |
| Web 框架 | ASP.NET Core | 10.0 |
| 持久化 | PostgreSQL | - |
| 目标框架 | .NET | 10.0 |

---

## 版本历史

| 版本 | 日期 | 变更 |
|------|------|------|
| 1.0.0 | 2026-04-24 | 初始版本 |