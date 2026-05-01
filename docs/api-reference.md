# API Reference - Agent-Scheduler Platform v2

本文档描述 Agent-Scheduler 平台重构后的 API 端点。

## 概述

v2 架构以 **Agent** 和 **Scheduler** 为核心，移除了 Cluster 概念。
- Agent 自持身份（首次注册由 Platform 分配 AgentId，本地持久化，重启携带）
- Agent 启动后自动上报所有 Scheduler 运行时信息
- Job 操作面向 Scheduler 而非 Cluster

---

## Agent API

### 1. 注册 Agent

首次注册或重连。首次注册不带 agentId，Platform 分配新 ID；重连时携带已有 agentId。

```http
POST /api/agents
Headers:
  X-Agent-Token: <api-token>
Body:
{
  "agentId": null,                  // null=首次注册, "agt-xxx"=重连
  "name": "agent-myhost",           // 可选
  "url": "http://agent:8080",       // 必填
  "agentVersion": "1.0.0",         // 可选
  "startedAt": "2026-04-30T00:00:00Z"  // UTC 时间
}
Response (200):
{
  "success": true,
  "data": {
    "agentId": "agt-a1b2c3d4e5f6",
    "token": "<api-token>",
    "heartbeatIntervalSeconds": 30,
    "warningThresholdSeconds": 30,
    "offlineThresholdSeconds": 60
  }
}
```

### 2. 获取 Agent 列表

```http
GET /api/agents
Response (200):
{
  "success": true,
  "data": [
    {
      "id": "agt-a1b2c3d4e5f6",
      "name": "agent-myhost",
      "url": "http://agent:8080",
      "status": "Online",
      "agentVersion": "1.0.0",
      "lastHeartbeat": "2026-04-30T00:00:00Z",
      "startedAt": "2026-04-30T00:00:00Z",
      "schedulerCount": 2
    }
  ]
}
```

### 3. 获取 Agent 详情

```http
GET /api/agents/{agentId}
Response (200):
{
  "success": true,
  "data": {
    "id": "agt-a1b2c3d4e5f6",
    "name": "agent-myhost",
    "url": "http://agent:8080",
    "status": "Online",
    "agentVersion": "1.0.0",
    "lastHeartbeat": "2026-04-30T00:00:00Z",
    "lastReportedAt": "2026-04-30T00:00:00Z",
    "startedAt": "2026-04-30T00:00:00Z",
    "createdAt": "2026-04-30T00:00:00Z",
    "updatedAt": "2026-04-30T00:00:00Z",
    "schedulers": [
      {
        "schedulerInfoId": "sch-xxx",
        "schedulerName": "DefaultQuartzScheduler",
        "schedulerInstanceId": "HOSTNAME1234567890",
        "status": "running",
        "isClustered": true,
        "runningSince": "2026-04-30T00:00:00Z",
        "reportedAt": "2026-04-30T00:00:00Z"
      }
    ]
  }
}
```

### 4. 删除 Agent（软删除）

```http
DELETE /api/agents/{agentId}
Headers:
  X-Agent-Token: <api-token>
Response (200): { "success": true, "data": {} }
```

### 5. Agent 心跳

```http
POST /api/agents/{agentId}/heartbeat
Body:
{
  "agentId": "agt-a1b2c3d4e5f6",
  "status": "Online",
  "timestamp": "2026-04-30T00:00:00Z",
  "schedulerSummaries": [
    {
      "schedulerName": "DefaultQuartzScheduler",
      "status": "running",
      "jobCount": 10,
      "runningJobCount": 2
    }
  ]
}
Response (200):
{
  "success": true,
  "data": {
    "serverTime": "2026-04-30T00:00:00Z",
    "shouldReportSchedulers": false
  }
}
```

### 6. 上报 Scheduler 信息

```http
POST /api/agents/{agentId}/schedulers
Body:
{
  "schedulers": [
    {
      "schedulerName": "DefaultQuartzScheduler",
      "schedulerInstanceId": "HOSTNAME1234567890",
      "status": "running",
      "isClustered": true,
      "jobStoreType": "Quartz.Impl.AdoJobStore.JobStoreTX",
      "threadPoolType": "Quartz.Simpl.SimpleThreadPool",
      "threadPoolSize": 10,
      "runningSince": "2026-04-30T00:00:00Z",
      "version": "3.17.1.0",
      "numberOfJobsExecuted": 42,
      "jobCounts": {
        "totalJobs": 10,
        "runningJobs": 2,
        "pausedJobs": 1,
        "blockedJobs": 0,
        "waitingJobs": 7
      },
      "properties": { "key": "value" }
    }
  ]
}
Response (200): { "success": true, "data": {} }
```

### 7. 查询 Agent 关联的 Schedulers

```http
GET /api/agents/{agentId}/schedulers
Response (200):
{
  "success": true,
  "data": [
    {
      "schedulerInfoId": "sch-xxx",
      "schedulerName": "DefaultQuartzScheduler",
      "schedulerInstanceId": "HOSTNAME1234567890",
      "status": "running",
      "isClustered": true,
      "runningSince": "2026-04-30T00:00:00Z",
      "reportedAt": "2026-04-30T00:00:00Z"
    }
  ]
}
```

---

## Scheduler API

### 8. 获取全局 Scheduler 列表

```http
GET /api/schedulers
Response (200):
{
  "success": true,
  "data": [
    {
      "id": "sch-xxx",
      "schedulerName": "DefaultQuartzScheduler",
      "schedulerInstanceId": "HOSTNAME1234567890",
      "status": "running",
      "isClustered": true,
      "runningSince": "2026-04-30T00:00:00Z",
      "lastReportedAt": "2026-04-30T00:00:00Z",
      "agentCount": 2
    }
  ]
}
```

### 9. 获取 Scheduler 详情

```http
GET /api/schedulers/{schedulerName}
Response (200):
{
  "success": true,
  "data": {
    "id": "sch-xxx",
    "schedulerName": "DefaultQuartzScheduler",
    "schedulerInstanceId": "HOSTNAME1234567890",
    "status": "running",
    "isClustered": true,
    "jobStoreType": "Quartz.Impl.AdoJobStore.JobStoreTX",
    "threadPoolType": "Quartz.Simpl.SimpleThreadPool",
    "threadPoolSize": 10,
    "runningSince": "2026-04-30T00:00:00Z",
    "version": "3.17.1.0",
    "numberOfJobsExecuted": 42,
    "firstReportedAt": "2026-04-30T00:00:00Z",
    "lastReportedAt": "2026-04-30T00:00:00Z",
    "agents": [
      {
        "agentId": "agt-a1b2c3d4e5f6",
        "agentName": "agent-myhost",
        "agentUrl": "http://agent:8080",
        "agentStatus": "Online",
        "reportedAt": "2026-04-30T00:00:00Z"
      }
    ]
  }
}
```

### 10. 获取 Scheduler 关联的 Agents

```http
GET /api/schedulers/{schedulerName}/agents
Response (200):
{
  "success": true,
  "data": [ { "agentId": "agt-xxx", "agentName": "...", ... } ]
}
```

---

## Job API (通过 Scheduler 路由)

### 11. 获取 Job 列表

```http
GET /api/schedulers/{schedulerName}/jobs?page=1&pageSize=20&status=&group=&keyword=
Response (200): { "success": true, "data": [ ... ] }
```

### 12. 创建 Job

```http
POST /api/schedulers/{schedulerName}/jobs
Body:
{
  "jobKey": "MyJob",
  "jobType": "SampleJob",
  "params": {},
  "schedule": { "type": "cron", "cronExpression": "0 0/5 * * * ?" },
  "options": { "disallowConcurrentExecution": false, "misfirePolicy": "FireAndProceed" }
}
Response (200): { "success": true, "data": { ... } }
```

### 13. 其他 Job 操作

```http
GET    /api/schedulers/{schedulerName}/jobs/{jobKey}         # 详情
PUT    /api/schedulers/{schedulerName}/jobs/{jobKey}          # 更新
DELETE /api/schedulers/{schedulerName}/jobs/{jobKey}          # 删除
POST   /api/schedulers/{schedulerName}/jobs/{jobKey}/trigger  # 触发
POST   /api/schedulers/{schedulerName}/jobs/{jobKey}/pause    # 暂停
POST   /api/schedulers/{schedulerName}/jobs/{jobKey}/resume   # 恢复
```

---

## Manifest API

```http
GET  /api/schedulers/{schedulerName}/manifest    # 获取 Job 类型清单
POST /api/schedulers/{schedulerName}/manifest    # 上报 Job 类型清单
```

---

## 废弃端点（301 重定向）

以下旧端点仍可用，但返回 301 永久重定向：

| 旧端点 | 新端点 |
|--------|--------|
| `POST /api/clusters/{id}/agents` | `POST /api/agents` |
| `GET /api/clusters/{id}/agents` | `GET /api/agents` |
| `POST /api/clusters/{id}/jobs` | `POST /api/schedulers/{id}/jobs` |
| `GET /api/clusters/{id}/jobs` | `GET /api/schedulers/{id}/jobs` |
| `GET /api/clusters/{id}/jobs/{key}` | `GET /api/schedulers/{id}/jobs/{key}` |
| `POST /api/clusters` | 已删除，返回 301 |

---

## 时间字段约定

所有时间字段使用 ISO 8601 UTC 格式（`DateTimeOffset`）：
- **写入**: `DateTimeOffset.UtcNow`，偏移量 +00:00
- **存储**: PostgreSQL `timestamptz` 类型
- **读取**: 统一返回 UTC 时间
- **示例**: `"2026-04-30T12:00:00Z"`

---

## 状态枚举

### Agent Status
| 值 | 说明 |
|-----|------|
| `Pending` | 刚注册，等待首次心跳 |
| `Online` | 正常在线 |
| `Warning` | 心跳超时（Warning 阈值内） |
| `Offline` | 已离线 |

### Scheduler Status
| 值 | 说明 |
|-----|------|
| `running` | 调度器正在运行 |
| `standby` | 调度器待机 |
| `unknown` | 状态未知 |
