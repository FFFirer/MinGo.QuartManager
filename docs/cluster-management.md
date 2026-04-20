# Agent Cluster Management Guide

本文档提供管理 MinGo Quarts Manager Agent 集群的完整指南。

## 概念

### 架构概览

```
Platform (Web UI + API)
       |
       | HTTP
       v
  [Cluster] ---------------- A gent Instances
       |        <one-to-many>
       v
  [Instance 1] [Instance 2] [Instance N]
       |          |           |
       v          v           v
    Quartz    Quartz      Quartz
    Scheduler  Scheduler   Scheduler
       |          |           |
       +----------+-----------+
                 |
                 v
            PostgreSQL
          (Shared JobStore)
```

### 核心概念

| 概念 | 描述 |
|---|---|
| Cluster | 逻辑分组，包含多个 Agent 实例 |
| Agent Instance | 单个 Agent 进程运行实例 |
| Quartz Instance | Quartz 调度器实例 ID |
| Instance Status | 实例健康状态 |

---

## 管理任务

### 1. 创建集群

**通过 UI:**
1. 导航到 Clusters 页面
2. 点击 "Create Cluster"
3. 填写集群名称和环境
4. 点击 "Save"

**通过 API:**
```bash
curl -X POST http://platform:5000/api/clusters \
  -H "Content-Type: application/json" \
  -d '{"name":"production","env":"prod","description":"Production cluster"}'
```

### 2. 添加 Agent 实例

**步骤 1: 配置 Agent**
编辑 `config.yaml`:
```yaml
agent:
  clusterId: "cls-xxx"  # 从平台创建的集群 ID
  clusterMode: false  # 或 true 用于集群模式
  
platform:
  url: "http://platform:5000"
  apiToken: "xxx"  # 集群的 Token
```

**步骤 2: 启动 Agent**
```bash
# 手动
./MinGo.Qap.Agent --config config.yaml

# Docker
docker run -d \
  -v $(pwd)/config.yaml:/app/config.yaml \
  -e QAP_CLUSTER_ID=cls-xxx \
  mingo.qap.agent
```

**步骤 3: 验证注册**
在 UI Clusters 页面查看实例数量，应显示 1。

### 3. 查看实例状态

**通过 UI:**
1. 点击集群名称
2. 查看 Instances 标签页

**通过 API:**
```bash
curl http://platform:5000/api/clusters/cls-xxx/agents
```

### 4. 监控集群健康

**状态指示:**
- 🟢 Online: 实例正常运行
- 🟡 Warning: 超过 30 秒无心跳
- 🔴 Offline: 超过 60 秒无心跳
- ⚪ Unknown: 无实例

**通过 UI:**
- Clusters 页面显示总体状态
- Instance 页面显示详细指标

### 5. 启用集群模式（高可用）

**步骤 1: 准备共享数据库**
```bash
# 创建 Quartz 数据库
createdb quartz

# 运行 Quartz 表脚本
psql -d quartz -f scripts/sql/quartz/tables_postgresql.sql
```

**步骤 2: 配置集群模式**
使用 `config-cluster.yaml`:
```yaml
agent:
  clusterMode: true
  
quartz:
  properties:
    quartz.jobStore.clustered: "true"
    quartz.dataSource.default.connectionString: "Host=postgres..."
```

**步骤 3: 启动多个实例**
```bash
# 实例 1
docker run -d --name agent-1 -v config1.yaml:/app/config.yaml mingo.qap.agent

# 实例 2  
docker run -d --name agent-2 -v config2.yaml:/app/config.yaml mingo.qap.agent
```

### 6. 删除实例

**通过 UI:**
1. 导航到集群的 Instances 页面
2. 点击实例行上的删除图标
3. 确认删除

**通过 API:**
```bash
curl -X DELETE http://platform:5000/api/agents/{agentId}
```

### 7. 更换集群 Token

**步骤 1: 生成新 Token**
```bash
# 通过 UI: Clusters -> Edit -> Regenerate Token
# 或通过 API
curl -X POST http://platform:5000/api/clusters/{clusterId}/rotate-token
```

**步骤 2: 更新 Agent 配置**
修改所有 Agent 的 `config.yaml`:
```yaml
platform:
  apiToken: "新token"
```

**步骤 3: 重启 Agents**
```bash
docker restart agent-1 agent-2
```

---

## 故障排除

### Agent 无法注册

1. 检查 Token 是否正确
2. 检查集群 ID 是否存在
3. 检查网络连通性
4. 查看 Agent 日志

```bash
# 测试连接
curl -v http://platform:5000/api/health

# 查看日志
docker logs agent -f
```

### 实例显示 Offline

1. 检查 Agent 进程是否运行
2. 检查心跳间隔配置
3. 检查网络
4. 重启 Agent

```bash
# 重启
docker restart agent

# 检查状态
docker ps | grep agent
```

### Quartz 集群不工作

1. 检查数据库连接
2. 检查所有实例使用相同数据库
3. 检查 `quartz.jobStore.clustered: true`
4. 检查防火墙允许数据库端口

```bash
# 测试数据库连接
psql -h postgres -U quartz -c "SELECT * FROM QRTZ_SCHEDULER_STATE"
```

---

## 最佳实践

### 开发环境
- 使用非集群模式 (RAMJobStore)
- 单实例部署

### 生产环境（高可用）
- 使用集群模式 (Quartz 集群)
- 至少 2 个实例
- 使用负载均衡器

### 监控
- 定期检查实例状态
- 设置心跳告警
- 记录实例数量变化

---

## 维护任务

### 备份

备份以下内容：
- Platform 数据库
- Quartz 数据库（如果使用集群）
- Agent 配置文件

### 升级 Agent

1. 备份配置
2. 停止旧版本
3. 部署新版本
4. 验证注册
5. 检查状态

### 迁移到集群模式

1. 确保共享数据库可用
2. 部署新配置
3. 启动额外实例
4. 验证 job 执行

---

## 容量规划

### 单实例容量

- 预估 Job 数: 50-100
- 执行频率: 每分钟数次
- 资源: 1 CPU, 512MB RAM

### 集群容量

- 增加实例数可处理更多 Job
- 2 实例: 2x 容量
- 建议不超过 10 个实例

---

## 安全

### 网络隔离

- Agent 使用内网
- 只暴露 HTTP 端口
- 使用防火墙规则

### Token 管理

- 定期轮换 Token
- 使用强随机 Token
- 安全存储配置

### 认证

所有 Agent 请求需要 `X-Agent-Token` 头。

---

## 性能

### 优化建议

1. 使用连接池
2. 减少心跳频率（如网络稳定）
3. 监控 Job 执行时间
4. 使用 Quartz 集群优化

### 监控指标

- 实例数量
- 在线实例比例
- Job 执行计数
- 心跳延迟