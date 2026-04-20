# Quartz 集群配置指南

## 概述

MinGo.QuartzManager 支持 Quartz.NET 集群模式，允许多个 Agent 实例共享同一个数据库，实现高可用性和负载均衡。

## 配置步骤

### 1. 数据库准备

#### 1.1 创建 PostgreSQL 数据库
```sql
CREATE DATABASE quartz;
CREATE USER quartz_user WITH PASSWORD 'your_password';
GRANT ALL PRIVILEGES ON DATABASE quartz TO quartz_user;
```

#### 1.2 创建 Quartz 集群表
使用提供的 SQL 脚本创建 Quartz 所需的表结构：

```bash
# 从 scripts/sql/quartz/tables_postgresql.sql 执行
psql -h localhost -U postgres -d quartz -f scripts/sql/quartz/tables_postgresql.sql
```

### 2. Agent 配置

#### 2.1 启用集群模式
在 `config.yaml` 中设置 `agent.clusterMode: true`：

```yaml
agent:
  clusterId: "cls-001"
  id: ""  # 自动生成
  port: 80
  heartbeatIntervalSeconds: 30
  clusterMode: true  # 启用集群模式
```

#### 2.2 配置 Quartz 属性
有两种方式配置 Quartz 集群属性：

**方式一：在 config.yaml 中直接配置**
```yaml
quartz:
  assemblyPath: ./jobs
  jobTypes:
    - "Sample.Jobs.EchoJob"
  properties:
    quartz.scheduler.instanceName: "QapAgentCluster"
    quartz.jobStore.type: "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz"
    quartz.jobStore.driverDelegateType: "Quartz.Impl.AdoJobStore.StdAdoDelegate, Quartz"
    quartz.jobStore.tablePrefix: "QRTZ_"
    quartz.jobStore.dataSource: "default"
    quartz.jobStore.clustered: "true"
    quartz.jobStore.clusterCheckinInterval: "20000"
    quartz.dataSource.default.provider: "Npgsql"
    quartz.dataSource.default.connectionString: "Host=postgres;Port=5432;Database=quartz;Username=postgres;Password=${POSTGRES_PASSWORD}"
```

**方式二：使用环境变量**
```bash
export QAP_AGENT_CLUSTER_MODE=true
export QAP_QUARTZ_JOBSTORE_TYPE="Quartz.Impl.AdoJobStore.JobStoreTX, Quartz"
export QAP_QUARTZ_JOBSTORE_CLUSTERED="true"
export QAP_QUARTZ_DATASOURCE_DEFAULT_CONNECTIONSTRING="Host=postgres;Port=5432;Database=quartz;Username=postgres;Password=your_password"
```

### 3. 集群实例管理

#### 3.1 启动多个 Agent 实例
每个 Agent 实例需要：
- 相同的 `clusterId`
- 不同的 `agent.id`（或留空自动生成）
- 相同的数据库连接配置

示例启动两个实例：
```bash
# 实例 1
export QAP_AGENT_ID="agent-1"
export QAP_PORT=8080
dotnet run --project src/MinGo.Qap.Agent

# 实例 2
export QAP_AGENT_ID="agent-2"
export QAP_PORT=8081
dotnet run --project src/MinGo.Qap.Agent
```

#### 3.2 实例注册和心跳
- Agent 启动时会自动向 Platform 注册
- 每个实例使用独立的 Agent ID 和 Quartz 实例 ID
- 心跳使用实例级别端点：`POST /api/agents/{agentId}/heartbeat`

### 4. Quartz 实例 ID 生成

在集群模式下，系统会自动生成唯一的 Quartz 实例 ID，格式为：
```
{clusterId}-{hostname}-{timestamp}
```
例如：`cls-001-server1-20250418093045`

### 5. 监控和健康检查

#### 5.1 集群健康状态
Platform 会监控所有 Agent 实例的状态：
- **Online**: 实例正常，心跳在 30 秒内
- **Warning**: 心跳超过 30 秒但小于 60 秒
- **Offline**: 心跳超过 60 秒

#### 5.2 Quartz 集群状态
Agent 心跳包含 Quartz 调度器状态：
- `IsClustered`: 是否运行在集群模式
- `InstanceId`: Quartz 实例 ID
- `Status`: 调度器状态（running/standby）
- `JobCounts`: 作业统计信息

### 6. 故障转移和负载均衡

#### 6.1 请求路由
Platform 使用随机选择策略将作业请求路由到健康的 Agent 实例：
- 从在线的实例中随机选择
- 自动跳过不健康的实例

#### 6.2 故障检测
- Platform 定期检查实例心跳
- 超过阈值的实例标记为 Offline
- Offline 实例不再接收新请求

### 7. 迁移指南

#### 7.1 从单实例迁移到集群
1. 备份现有作业数据
2. 创建 Quartz 数据库表
3. 将现有作业数据导入 Quartz 表（如果需要）
4. 配置 Agent 使用集群模式
5. 启动新 Agent 实例，逐步停用旧实例

#### 7.2 回滚步骤
1. 停止所有集群实例
2. 将 Agent 配置恢复为 RAMJobStore 模式
3. 启动单实例 Agent

### 8. 常见问题

#### 8.1 连接数据库失败
- 检查数据库连接字符串
- 确认 PostgreSQL 服务运行正常
- 验证用户权限

#### 8.2 集群实例无法同步
- 确认所有实例使用相同的数据库
- 检查 `quartz.jobStore.clustered` 设置为 "true"
- 确认网络连接正常

#### 8.3 作业重复执行
- 检查 `quartz.jobStore.clusterCheckinInterval` 设置（建议 20000ms）
- 确认所有实例时间同步（使用 NTP）

#### 8.4 性能问题
- 调整 PostgreSQL 连接池设置
- 考虑增加 `quartz.threadPool.threadCount`
- 监控数据库锁竞争

### 9. 高级配置

#### 9.1 自定义表前缀
```yaml
quartz:
  properties:
    quartz.jobStore.tablePrefix: "MY_QRTZ_"
```

#### 9.2 调整集群检查间隔
```yaml
quartz:
  properties:
    quartz.jobStore.clusterCheckinInterval: "30000"  # 30秒
```

#### 9.3 使用其他数据库
支持任何 Quartz 兼容的数据库（MySQL、SQL Server 等），只需更改：
- `quartz.dataSource.default.provider`
- 相应的连接字符串
- 对应的数据库驱动包

### 10. 监控指标

集群模式下可监控的指标：
- 实例数量（在线/警告/离线）
- 数据库连接池使用情况
- 作业执行成功率
- 集群检查延迟
- 锁等待时间

## 附录

### A. 配置文件示例
完整示例见 `deploy/agent/config-cluster.yaml`

### B. SQL 脚本位置
- `scripts/sql/quartz/tables_postgresql.sql`

### C. 环境变量参考
| 变量名 | 说明 | 默认值 |
|--------|------|--------|
| QAP_AGENT_CLUSTER_MODE | 启用集群模式 | false |
| QAP_QUARTZ_JOBSTORE_CLUSTERED | Quartz 集群模式 | false |
| QAP_QUARTZ_DATASOURCE_DEFAULT_CONNECTIONSTRING | 数据库连接字符串 | - |
| QAP_QUARTZ_JOBSTORE_CLUSTERCHECKININTERVAL | 集群检查间隔（ms） | 20000 |

### D. 相关 API 端点
- `GET /api/clusters/{id}/agents` - 获取集群实例列表
- `POST /api/agents/{agentId}/heartbeat` - 实例心跳
- `GET /api/agents/{agentId}` - 获取实例详情