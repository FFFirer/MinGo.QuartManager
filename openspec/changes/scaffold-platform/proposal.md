# Proposal: MinGo.Qap 平台搭建

## Summary

搭建 **MinGo.Qap (Quartz Admin Platform)** 多集群 Quartz.NET 可视化管理平台，实现：
- 多集群统一管理
- Job 配置可视化（Cron/Interval/Once）
- 运维操作（Trigger/Pause/Resume/Delete）
- 心跳健康检测

## Goals

### V1 (MVP)
- [ ] 项目脚手架（Platform + Agent + Shared + UI）
- [ ] Cluster 管理（注册/注销/状态）
- [ ] Job Manifest（可用 Job 类型清单）
- [ ] Job 实例管理（创建/更新/删除/触发/暂停/恢复）
- [ ] 心跳与健康检测
- [ ] 基础前端 UI

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                     MinGo.Qap Platform                          │
│                  (ASP.NET Core + React)                         │
└─────────────────────────────┬───────────────────────────────────┘
                              │ HTTP
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                       MinGo.Qap Agent                           │
│                  (每 Cluster 一个独立进程)                       │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐             │
│  │ JobRegistry │  │JobConverter │  │Heartbeat   │             │
│  └─────────────┘  └─────────────┘  └─────────────┘             │
└─────────────────────────────┬───────────────────────────────────┘
                              │ ScheduleJob / TriggerJob / etc.
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Quartz Scheduler (Cluster)                     │
│              JobDetail + Trigger + JobStore (DB)                │
└─────────────────────────────────────────────────────────────────┘
```

## Projects

| Project | Description |
|---------|-------------|
| MinGo.Qap.Platform | 平台服务（API + UI） |
| MinGo.Qap.Agent | Agent 服务（独立进程） |
| MinGo.Qap.Shared | 共享模型/协议/工具 |
| MinGo.Qap.UI | React 前端 |
| Sample.Jobs | 示例 Job 实现 |

## Design Decisions

### Cluster-First
所有操作基于当前选中的 Cluster，用户先选 Cluster 再操作。

### Quartz-Native
调度逻辑完全由 Quartz 决定，平台只做可视化、配置、转发。

### Agent 无状态
Agent 本身无状态，数据存储：
- Quartz DB（Job 真正数据）
- Platform DB（Cluster 元数据 + JobDefinition 备份）

### Job 类型
V1 使用预定义 Job：
- 业务系统实现 `IJob` 接口
- Agent 配置声明可用 Job 类型
- 用户选择 Job + 填写参数 + 配置调度

### 幂等保证
JobKey 唯一 + `replace: true` 覆盖，重复请求安全。

### 认证
- V1: Basic Auth 或无认证（依赖网络隔离）
- Platform ↔ Agent: 网络隔离，内网可信

## Data Models

### Cluster (Platform)
```csharp
class Cluster
{
    string Id;           // cls-xxx
    string Name;         // 用户可见名称
    string Env;          // prod/staging/dev
    string AgentUrl;     // Agent 地址
    ClusterStatus Status; // Pending/Online/Warning/Offline/Deleted
    DateTime? LastHeartbeat;
}
```

### JobDefinition (Platform - 备份)
```csharp
class JobDefinition
{
    string Id;
    string ClusterId;
    string JobKey;       // Name + Group
    string JobType;      // manifest key
    string Params;       // JSON
    string Schedule;     // JSON
    string Options;      // JSON
    SyncStatus Status;    // Pending/Synced/Failed/Timeout
    DateTime CreatedAt;
}
```

### JobManifest (Agent → Platform)
```csharp
class JobManifest
{
    string ClusterId;
    List<JobTypeInfo> Jobs;
}

class JobTypeInfo
{
    string Key;          // manifest key
    string Description;
    List<ParameterInfo> Parameters;
}
```

## API Design

### Platform API

| Method | Path | Description |
|--------|------|-------------|
| POST | /api/clusters | 注册 Cluster |
| GET | /api/clusters | 查询 Cluster 列表 |
| GET | /api/clusters/{id} | Cluster 详情 |
| DELETE | /api/clusters/{id} | 注销 Cluster |
| POST | /api/clusters/{id}/heartbeat | Agent 心跳 |
| POST | /api/clusters/{id}/manifest | 上报 Manifest |
| GET | /api/clusters/{id}/manifest | 获取 Manifest |
| GET | /api/clusters/{id}/jobs | Job 列表 |
| GET | /api/clusters/{id}/jobs/{jobKey} | Job 详情 |
| POST | /api/clusters/{id}/jobs | 创建 Job |
| PUT | /api/clusters/{id}/jobs/{jobKey} | 更新 Job |
| DELETE | /api/clusters/{id}/jobs/{jobKey} | 删除 Job |
| POST | /api/clusters/{id}/jobs/{jobKey}/trigger | 手动触发 |
| POST | /api/clusters/{id}/jobs/{jobKey}/pause | 暂停 |
| POST | /api/clusters/{id}/jobs/{jobKey}/resume | 恢复 |
| GET | /api/jobs | 跨集群聚合查询 |

### Agent API

| Method | Path | Description |
|--------|------|-------------|
| GET | /health | 健康检查 |
| GET | /api/jobs/manifest | 获取可用 Job |
| GET | /api/jobs/{jobKey} | Job 详情 |
| GET | /api/jobs | Job 列表 |
| POST | /api/jobs | 创建 Job |
| PUT | /api/jobs/{jobKey} | 更新 Job |
| DELETE | /api/jobs/{jobKey} | 删除 Job |
| POST | /api/jobs/{jobKey}/trigger | 手动触发 |
| POST | /api/jobs/{jobKey}/pause | 暂停 |
| POST | /api/jobs/{jobKey}/resume | 恢复 |

## Heartbeat

```
Interval: 30s
Timeout Warning: 60s (2次间隔)
Timeout Offline: 90s (3次间隔)

Heartbeat Content:
{
    timestamp, agentVersion, uptimeSeconds,
    schedulerStatus, jobs: { total, normal, paused, blocked, executing },
    system: { memoryUsedMb, cpuPercent }
}
```

## UI Design

### Principles
- 简洁、高密度、现代化
- 暗色主题优先
- 运维友好

### Tech Stack
- React + TypeScript
- TailwindCSS + shadcn/ui
- TanStack Table + TanStack Query
- Lucide Icons

### Pages
1. **Clusters** - Cluster 卡片列表 + 状态
2. **Jobs** - 高密度表格 + 操作
3. **Job Detail** - 左右分栏，编辑表单

## Deployment

### Agent
- 独立进程部署
- config.yaml 配置
- 每个 Cluster 一个 Agent 实例
- 优雅关闭（SIGTERM）

### Config
```yaml
agent:
  clusterId: cls-xxx
  port: 8080

platform:
  url: http://platform:5000

quartz:
  assemblyPath: /app/jobs/
  jobTypes:
    - Sample.Jobs.InventorySyncJob
```

## Risks & Mitigations

| Risk | Mitigation |
|------|------------|
| 演变为调度系统 | 不做 DSL/编排，明确边界 |
| Quartz 能力误用 | UI 严格映射 Quartz，不自定义调度 |
| 状态不一致 | 操作时同步 + 查询从 Agent 获取权威数据 |

## Out of Scope (V1)

- 执行日志收集
- Job Type 扩展机制
- RBAC 权限系统
- 告警通知
- 跨集群 Job 调度

## Next Steps

1. 创建项目脚手架
2. 实现 Shared 共享模型
3. 实现 Agent 核心功能
4. 实现 Platform API
5. 实现前端 UI
6. 集成测试
