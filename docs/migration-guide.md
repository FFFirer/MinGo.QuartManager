# Agent 集群迁移指南

本文档提供从单实例部署迁移到多实例集群的完整指南。

## 概述

迁移流程分为三个阶段：

```
Phase 1: 平台更新 → Phase 2: Agent 更新 → Phase 3: 清理
```

每个阶段可以独立完成，支持回滚。

---

## Phase 1: 平台更新（必须先完成）

### 步骤 1.1: 运行数据库迁移

确保数据库迁移文件已执行：

```bash
# 使用 EF Core 迁移
dotnet ef database update
```

或手动执行 SQL 脚本：

```sql
-- 创建 AgentInstances 表（如果迁移脚本未自动执行）
CREATE TABLE IF NOT EXISTS "AgentInstances" (
    "Id" TEXT NOT NULL,
    "ClusterId" TEXT NOT NULL,
    "Name" TEXT,
    "Url" TEXT NOT NULL,
    "Status" INTEGER NOT NULL DEFAULT 0,
    "LastHeartbeat" TIMESTAMP,
    "QuartzInstanceId" TEXT,
    "TokenHash" TEXT,
    "AgentVersion" TEXT,
    "StartedAt" TIMESTAMP,
    "CreatedAt" TIMESTAMP NOT NULL,
    "UpdatedAt" TIMESTAMP,
    "DeletedAt" TIMESTAMP,
    CONSTRAINT "FK_AgentInstances_Clusters_ClusterId" FOREIGN KEY ("ClusterId")
        REFERENCES "Clusters" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AgentInstances_ClusterId" ON "AgentInstances" ("ClusterId");
CREATE INDEX "IX_AgentInstances_Status" ON "AgentInstances" ("Status");
CREATE INDEX "IX_AgentInstances_LastHeartbeat" ON "AgentInstances" ("LastHeartbeat");
CREATE UNIQUE INDEX "IX_AgentInstances_ClusterId_Url" ON "AgentInstances" ("ClusterId", "Url");
```

### 步骤 1.2: 迁移现有 AgentUrl

如果 `Clusters` 表中有 `AgentUrl` 数据，需要迁移到 `AgentInstances`：

```sql
INSERT INTO "AgentInstances" ("Id", "ClusterId", "Name", "Url", "Status", "LastHeartbeat", "CreatedAt")
SELECT 
    gen_random_uuid()::text,
    "Id",
    'agent-001',
    "AgentUrl",
    1,  -- Online
    "LastHeartbeat",
    NOW()
FROM "Clusters"
WHERE "AgentUrl" IS NOT NULL AND "DeletedAt" IS NULL;
```

### 步骤 1.3: 验证迁移

检查数据是否正确迁移：

```sql
-- 验证实例数量
SELECT c."Id", c."Name", COUNT(ai."Id") as InstanceCount
FROM "Clusters" c
LEFT JOIN "AgentInstances" ai ON c."Id" = ai."ClusterId" AND ai."DeletedAt" IS NULL
GROUP BY c."Id", c."Name";
```

### 步骤 1.4: 部署新平台

```bash
dotnet publish -c Release
# 部署到生产环境
```

---

## Phase 2: Agent 更新

### 步骤 2.1: 更新 Agent 配置

编辑 `config.yaml`：

```yaml
agent:
  clusterId: "cls-001"  # 与平台中的集群 ID 一致
  clusterMode: false  # 先使用非集群模式
  heartbeatIntervalSeconds: 30

platform:
  url: "http://platform:5000"
  apiToken: "your-token"  # 从平台获取
```

### 步骤 2.2: 启动 Agent

```bash
./MinGo.Qap.Agent
```

Agent 启动时会自动注册到平台。

### 步骤 2.3: 验证注册

在平台 UI 中查看集群的实例数量，应该显示 1 个实例。

### 步骤 2.4: 添加更多实例（可选）

在另一台服务器上重复 2.1-2.3 步骤：

```yaml
agent:
  # 使用不同的 id 或留空自动生成
  clusterId: "cls-001"
  id: ""  # 自动生成
```

### 步骤 2.5: 启用集群模式（可选）

如果需要 Quartz 集群功能：

1. 使用 `config-cluster.yaml` 配置
2. 配置共享数据库连接
3. 确保所有实例使用相同的数据库

```yaml
agent:
  clusterMode: true

quartz:
  properties:
    quartz.jobStore.clustered: "true"
    quartz.dataSource.default.connectionString: "Host=postgres;..."
```

---

## Phase 3: 清理

### 步骤 3.1: 确认所有 Agent 已迁移

验证旧的心跳端点不再使用：

```bash
# 检查日志中是否有旧端点调用
grep -r "heartbeat" logs/
```

### 步骤 3.2: 移除废弃字段（可选）

```sql
-- 只有在确认所有 Agent 都使用新端点后执行
ALTER TABLE "Clusters" DROP COLUMN IF EXISTS "AgentUrl";
```

### 步骤 3.3: 移除废弃 API（可选）

如果不再需要向后兼容，可以移除旧的端点。

---

## 回滚方案

### Phase 1 回滚

```bash
# 撤销数据库迁移
dotnet ef database update PreviousMigration
```

### Phase 2 回滚

1. 恢复旧的 Agent 配置
2. 部署旧版本 Agent
3. 旧端点仍然工作

---

## 常见问题

### Q: Agent 注册失败

A: 检查：
1. 平台 URL 是否正确
2. Token 是否有效
3. 集群 ID 是否存在

### Q: 实例状态显示 Offline

A: 检查：
1. Agent 是否正在运行
2. 心跳间隔是否正确（默认 30 秒）
3. 网络是否可达

### Q: Quartz 集群无法启动

A: 检查：
1. 数据库连接是否正确
2. 所有实例是否使用相同数据库
3. `quartz.jobStore.clustered` 是否为 `true`

---

## 配置参考

### 最小配置（单实例）

```yaml
agent:
  clusterId: "cls-001"
  clusterMode: false

platform:
  url: "http://platform:5000"
  apiToken: "token"

quartz:
  assemblyPath: ./jobs
  jobTypes:
    - "Sample.Jobs.EchoJob"
```

### 集群配置（多实例）

```yaml
agent:
  clusterId: "cls-001"
  clusterMode: true

platform:
  url: "http://platform:5000"
  apiToken: "token"

quartz:
  properties:
    quartz.jobStore.clustered: "true"
    quartz.dataSource.default.provider: "Npgsql"
    quartz.dataSource.default.connectionString: "Host=postgres;..."
```