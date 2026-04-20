# API Reference - Agent Cluster Support

本文档描述新增的 Agent 实例管理 API 端点。

## 概述

新架构支持一个 Cluster 包含多个 Agent 实例，提供高可用性和水平扩展能力。

---

## 新增端点

### 1. 注册 Agent 实例

注册新的 Agent 实例到指定集群。

```http
POST /api/clusters/{clusterId}/agents
Headers:
  X-Agent-Token: <token>
Body:
{
  "name": "agent-001",           // 可选，默认自动生成
  "url": "http://agent:80",      // 必填，Agent 服务地址
  "quartzInstanceId": "cls-001-host1-001"  // 可选
}
Response (200):
{
  "success": true,
  "data": {
    "agentId": "uuid",
    "quartzInstanceId": "cls-001-host1-001"
  }
}
Response (401):
{
  "success": false,
  "error": "Invalid token"
}
Response (409):
{
  "success": false,
  "error": "Agent with this URL already exists"
}
```

### 2. 获取集群的所有实例

列出集群的所有 Agent 实例。

```http
GET /api/clusters/{clusterId}/agents?includeDeleted=false
Response (200):
{
  "success": true,
  "data": [
    {
      "id": "uuid",
      "clusterId": "cls-001",
      "name": "agent-001",
      "url": "http://agent1:80",
      "status": 1,
      "statusText": "Online",
      "lastHeartbeat": "2024-01-01T00:00:00Z",
      "quartzInstanceId": "cls-001-host1-001",
      "agentVersion": "1.0.0",
      "startedAt": "2024-01-01T00:00:00Z",
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ]
}
```

### 3. Agent 心跳

Agent 实例定期发送心跳以表明其可用性。

```http
POST /api/agents/{agentId}/heartbeat
Body:
{
  "status": 1,                    // 必填，0=Pending, 1=Online, 2=Warning
  "quartzInstanceId": "...",
  "schedulerStatus": "Running",
  "jobCount": 10,
  "executingJobCount": 2
}
Response (200):
{
  "success": true,
  "data": {
    "instanceId": "uuid",
    "lastHeartbeat": "2024-01-01T00:00:00Z",
    "status": 1
  }
}
Response (404):
{
  "success": false,
  "error": "Agent instance not found"
}
```

### 4. 获取单个实例

获取特定 Agent 实例的详细信息。

```http
GET /api/agents/{agentId}
Response (200):
{
  "success": true,
  "data": {
    "id": "uuid",
    "clusterId": "cls-001",
    "name": "agent-001",
    "url": "http://agent1:80",
    "status": 1,
    "statusText": "Online",
    "lastHeartbeat": "2024-01-01T00:00:00Z",
    "quartzInstanceId": "cls-001-host1-001",
    "agentVersion": "1.0.0",
    "startedAt": "2024-01-01T00:00:00Z",
    "createdAt": "2024-01-01T00:00:00Z",
    "metrics": {
      "jobCount": 10,
      "executingJobCount": 2,
      "uptime": "1d 2h 30m"
    }
  }
}
```

### 5. 删除实例

软删除 Agent 实例（标记为已删除）。

```http
DELETE /api/agents/{agentId}
Response (200):
{
  "success": true,
  "data": { "success": true }
}
```

---

## 修改的端点

### 1. 集群列表

`GET /api/clusters` 响应增加 `instanceCount` 字段：

```json
{
  "success": true,
  "data": [
    {
      "id": "cls-001",
      "name": "Production",
      "status": 1,
      "statusText": "Online",
      "instanceCount": 3,
      "onlineInstanceCount": 2,
      "warningInstanceCount": 1,
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ]
}
```

### 2. 集群详情

`GET /api/clusters/{id}` 响应增加实例列表：

```json
{
  "success": true,
  "data": {
    "id": "cls-001",
    "name": "Production",
    "status": 1,
    "statusText": "Online",
    "instances": [...],
    "createdAt": "2024-01-01T00:00:00Z"
  }
}
```

---

## 状态码

| 值 | 名称 | 描述 |
|---|---|---|
| 0 | Pending | 待注册 |
| 1 | Online | 在线 |
| 2 | Warning | 心跳延迟 |
| 3 | Offline | 离线 |
| 4 | Deleted | 已删除 |

状态计算规则：
- `Warning`: 超过 30 秒无心跳
- `Offline`: 超过 60 秒无心跳

集群状态计算：
- `Online`: 至少 1 个实例 Online
- `Warning`: 无 Online，至少 1 个 Warning
- `Offline`: 无在线实例

---

## 认证

所有 Agent 相关端点需要 `X-Agent-Token` 头：

```
X-Agent-Token: <cluster-token>
```

平台使用集群的 Token 进行验证。

---

## 错误响应

所有错误响应格式：

```json
{
  "success": false,
  "error": "错误消息"
}
```

常见 HTTP 状态码：
- `200`: 成功
- `400`: 请求参数错误
- `401`: 认证失败
- `404`: 资源不存在
- `409`: 冲突
- `429`: 超过实例限制
- `500`: 服务器错误