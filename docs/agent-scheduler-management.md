# Agent-Scheduler Management Guide (v2)

本文档提供管理 MinGo Quartz Manager Agent-Scheduler 平台的完整指南。

## 架构概览 (v2)

```
Platform (Web UI + API)
       |
       | HTTP (X-Scheduler-Name 路由)
       v
  [Agents] <---many-to-many---> [Schedulers]
       |                              |
       v                              v
  Agent 进程                      Quartz Scheduler
  (身份持久化)                    (运行时信息上报)
```

### 核心变更 (v2)

v2 架构移除了 Cluster 概念，以 **Agent** 和 **Scheduler** 为核心：

- **Agent**: 宿主程序中的 Quartz 代理，注册后由 Platform 分配唯一 AgentId
- **Scheduler**: Quartz.NET 调度器实例（宿主程序可注册多个）
- **Agent-Scheduler 关联**: 多对多关系，一个 Agent 可管理多个 Scheduler，一个 Scheduler 可被多个 Agent 共享（Quartz 集群）

### 关键改进

1. **Agent 身份持久化**: AgentId 首次注册由 Platform 分配，持久化到本地文件 `agent-identity.json`
2. **多 Scheduler 支持**: Agent 自动发现宿主程序中的所有 IScheduler 实例
3. **Scheduler 主动上报**: 注册成功后自动上报 Scheduler 运行时信息
4. **智能路由**: Platform 根据 SchedulerName 路由 Job 操作到正确的 Agent

## Agent 配置

### config.yaml

```yaml
agent:
  id: "my-agent"            # 可选显示名称
  heartbeatIntervalSeconds: 30
  registrationMaxAttempts: 5
  registrationRetryDelaySeconds: 5

platform:
  url: "http://platform:5000"
  apiToken: "your-api-token"

quartz:
  jobTypes:
    - "MyApp.Jobs.DataSyncJob"
  properties:
    quartz.scheduler.instanceName: "MyScheduler"
    quartz.threadPool.threadCount: 10
```

### Agent 启动序列

```
1. HostedAgentService Start
2.   读取 agent-identity.json
3.   POST /api/agents (携带 AgentId 或 null)
4. ← 获取 AgentId + Token
5.   写入 agent-identity.json (持久化)
6.   IAgentSchedulerAccessor.GetAll()
7.   POST /api/agents/{agentId}/schedulers (上报)
8.   进入心跳循环
```

## Scheduler 管理

### 单 Scheduler 模式（默认）

宿主注册一个 IScheduler，Agent 自动发现：

```csharp
builder.Services.AddSingleton<IScheduler>(sp => {
    var factory = new StdSchedulerFactory();
    var scheduler = factory.GetScheduler().GetAwaiter().GetResult();
    scheduler.Start();
    return scheduler;
});
```

### 多 Scheduler 模式

宿主注册多个 IScheduler：

```csharp
builder.Services.AddSingleton<IScheduler>("scheduler-a", sp => { ... });
builder.Services.AddSingleton<IScheduler>("scheduler-b", sp => { ... });
```

### 显式 IAgentSchedulerAccessor

宿主可自定义 Scheduler 访问逻辑：

```csharp
builder.Services.AddSingleton<IAgentSchedulerAccessor>(sp => {
    // 自定义发现逻辑
    return new CustomSchedulerAccessor();
});
```

## Job 操作

Job 操作通过 Scheduler 名称路由：

```
旧: POST /api/clusters/{clusterId}/jobs
新: POST /api/schedulers/{schedulerName}/jobs
```

Platform 根据 SchedulerName 自动选择健康的 Agent 转发。

### Scheduler 名称解析

Agent 端解析顺序：
1. `X-Scheduler-Name` HTTP Header
2. `?schedulerName=` Query 参数
3. 默认第一个 Scheduler

## 时间类型约定

所有时间字段使用 `DateTimeOffset`:
- 代码: `DateTimeOffset.UtcNow`
- 数据库: PostgreSQL `timestamptz`
- API: ISO 8601 UTC (如 `"2026-04-30T12:00:00Z"`)

## 迁移指南

从 v1 (Cluster 架构) 迁移到 v2 (Agent-Scheduler 架构)：

1. 运行 EF Core Migration 创建新表
2. 运行数据迁移脚本 (`docs/migrations/v2_migrate_agent_instance_to_agent.sql`)
3. 更新 Agent 配置，移除 `clusterId`
4. 部署新版本 Agent
5. 旧 API 端点保留 301 重定向

详细迁移说明见 [docs/migration-guide.md](migration-guide.md)。

## 监控与运维

### Agent 状态

| 状态 | 说明 |
|------|------|
| Pending | 刚注册，等待首次心跳 |
| Online | 正常在线 |
| Warning | 心跳超时（Warning < 30s） |
| Offline | 已离线（Offline > 60s） |

### Scheduler 状态

| 状态 | 说明 |
|------|------|
| running | 调度器正在运行 |
| standby | 调度器待机 |
| unknown | 状态未知 |

### 关键指标

- Agent 心跳间隔：30s（可配置）
- 离线判定：60s（可配置）
- Scheduler 上报：注册时 + 心跳检测到变化时
