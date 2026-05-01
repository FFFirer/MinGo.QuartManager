# Agent-Scheduler 平台 — 待实现项清单

本文档汇总 Agent-Scheduler 平台架构重构（v2.0.0）中所有尚未实现的待办项，作为后续工作跟踪依据。

---

## 1. 数据库迁移与清理

### 1.1 数据迁移验证

运行 EF Core Migration 和数据迁移脚本后，验证数据一致性：

```sql
-- 1. 运行 EF Core 迁移
dotnet ef database update --project src/MinGo.Qap.Platform

-- 2. 运行数据迁移脚本（PostgreSQL）
psql -d MinGoQap -f docs/migrations/v2_migrate_agent_instance_to_agent.sql
psql -d MinGoQap -f docs/migrations/v2_migrate_cluster_deprecate.sql

-- 3. 验证 Agent 数据完整
SELECT COUNT(*) AS agent_count FROM "Agents";
SELECT COUNT(*) AS instance_count FROM "AgentInstances" WHERE "DeletedAt" IS NULL;

-- 4. 检查是否有遗漏
SELECT a."Id", a."Name", a."Url", a."Status"
FROM "Agents" a
LEFT JOIN "AgentInstances" ai ON ai."Id" = a."Id"
WHERE ai."Id" IS NULL;
```

### 1.2 删除 Cluster 表

确认所有外部依赖已迁移后执行：

```sql
-- 确认无 JobDefinition 依赖
SELECT COUNT(*) FROM "JobDefinitions" WHERE "ClusterId" IS NOT NULL;

-- 删除废弃表
DROP TABLE IF EXISTS "Clusters" CASCADE;
```

### 1.3 删除旧实体代码

数据库确认无误后，删除源码中的旧实体文件：

| 文件 | 说明 |
|------|------|
| `src/MinGo.Qap.Platform/Data/Entities/Cluster.cs` | Cluster 实体 + JobDefinition 内嵌类 |
| `src/MinGo.Qap.Platform/Data/Entities/AgentInstance.cs` | AgentInstance 实体 |
| `src/MinGo.Qap.Shared/Enums/LegacyEnums.cs` | 迁移兼容枚举（ClusterStatus, SyncStatus） |

> ⚠️ 删除前需从 `PlatformDbContext.cs` 中移除对应的 `DbSet<>` 配置和实体映射。

### 1.4 生成清理 Migration

删除实体后生成新 Migration 移除数据库中对应的旧表。

---

## 2. 端到端验证（E2E）

需部署完整的 Agent + Platform 环境后逐项验证。

### 2.1 Agent 生命周期

| # | 场景 | 预期结果 | 验证状态 |
|---|------|---------|---------|
| 1 | Agent 首次启动 | 注册成功 → 收到 AgentId → 持久化到 `agent-identity.json` | ⏳ |
| 2 | Agent 重启 | 读取 `agent-identity.json` → 携带 AgentId 重连 → 成功 | ⏳ |
| 3 | Agent Token 无效 | 注册被拒绝 → 日志记录 Unauthorized → 不写入 identity 文件 | ⏳ |

### 2.2 Scheduler 上报

| # | 场景 | 预期结果 | 验证状态 |
|---|------|---------|---------|
| 4 | Agent 注册后自动上报 Scheduler | `POST /api/agents/{id}/schedulers` 被调用 → Platform 正确存储 | ⏳ |
| 5 | Platform 查询 Scheduler 信息 | `GET /api/schedulers` 返回完整列表 → `GET /api/schedulers/{name}` 返回详情 + 关联 Agents | ⏳ |
| 6 | Agent 查询自己的 Schedulers | `GET /api/agents/{id}/schedulers` 返回关联的 Scheduler 列表 | ⏳ |

### 2.3 Job 操作路由

| # | 场景 | 预期结果 | 验证状态 |
|---|------|---------|---------|
| 7 | 通过 Scheduler 创建 Job | `POST /api/schedulers/{name}/jobs` → 路由到正确 Agent → Job 创建成功 | ⏳ |
| 8 | 通过 Scheduler 触发/暂停/恢复 Job | 各操作按 schedulerName 路由到正确 Agent → Quartz 执行操作 | ⏳ |
| 9 | Scheduler 不存在 | 返回 404 → 清晰的错误信息 | ⏳ |

### 2.4 多 Scheduler 场景

| # | 场景 | 预期结果 | 验证状态 |
|---|------|---------|---------|
| 10 | Agent 持有多个 Scheduler | `IAgentSchedulerAccessor.GetAll()` 返回全部 → 上报全部 → Platform 全部存储 | ⏳ |
| 11 | 按名称路由到特定 Scheduler | `X-Scheduler-Name` Header → Agent 路由到正确的 IScheduler | ⏳ |
| 12 | 默认 Scheduler 路由 | 未指定 SchedulerName → 使用默认第一个 Scheduler | ⏳ |

### 2.5 Quartz 集群场景

| # | 场景 | 预期结果 | 验证状态 |
|---|------|---------|---------|
| 13 | 多 Agent 上报同一 Scheduler | SchedulerInfo 按 `(SchedulerName, InstanceId)` 去重 | ⏳ |
| 14 | 通过 SchedulerName 路由到任一 Agent | Platform 任选一个健康 Agent → 操作成功 | ⏳ |
| 15 | 某 Agent 下线 | 路由自动跳过离线 Agent → 路由到其他健康 Agent | ⏳ |

### 2.6 心跳与离线检测

| # | 场景 | 预期结果 | 验证状态 |
|---|------|---------|---------|
| 16 | Agent 正常心跳 | `POST /api/agents/{id}/heartbeat` → Status 保持 Online | ⏳ |
| 17 | 心跳中包含 Scheduler 状态摘要 | 心跳 body 中携带 `schedulerSummaries` 数组 | ⏳ |
| 18 | Agent 停止心跳 → 超时 → Warning | 超过 WarningThreshold → Agent 状态变为 Warning | ⏳ |
| 19 | 继续超时 → Offline | 超过 OfflineThreshold → Agent 状态变为 Offline | ⏳ |
| 20 | Agent 恢复心跳 → Online | 重新收到心跳 → 状态恢复 Online | ⏳ |

### 2.7 前端 UI 验证

| # | 场景 | 预期结果 | 验证状态 |
|---|------|---------|---------|
| 21 | Agent 列表页 | `/agents` → 显示所有 Agent，状态指示正确，点击跳转详情 | ⏳ |
| 22 | Agent 详情页 | `/agents/{id}` → 显示 Agent 信息 + 关联 Schedulers 列表，点击跳转 Scheduler | ⏳ |
| 23 | Scheduler 列表页 | `/schedulers` → 显示所有 Scheduler，状态指示正确 | ⏳ |
| 24 | Scheduler 详情页 | `/schedulers/{name}` → 显示运行时信息 + 关联 Agents + Job 按钮 | ⏳ |
| 25 | Scheduler → Job 操作 | `/schedulers/{name}/jobs` → 创建/查看/操作 Job | ⏳ |
| 26 | Sidebar 导航 | Agents / Schedulers 菜单 → 最近访问 Agent 下拉 → 正确导航 | ⏳ |
| 27 | Dashboard 统计 | 首页显示 Agent 总数 / Scheduler 总数状态 | ⏳ |

### 2.8 301 重定向

| # | 场景 | 预期结果 | 验证状态 |
|---|------|---------|---------|
| 28 | 旧 `/api/clusters/{id}` 端点 | 301 → `/api/agents/{id}` | ⏳ |
| 29 | 旧 `/api/clusters/{id}/jobs` 端点 | 301 → `/api/schedulers/{id}/jobs` | ⏳ |
| 30 | 旧 `/api/clusters/{id}/agents` 端点 | 301 → `/api/agents` | ⏳ |

---

## 3. 文档更新

| 项目 | 说明 | 状态 |
|------|------|------|
| `docs/migration-guide.md` | 补充 v1→v2 迁移步骤 | 📝 待更新 |
| `docs/quartz-cluster-setup.md` | 移除 Cluster 引用，更新为 Scheduler 集群说明 | 📝 待更新 |
| Agent README | `src/MinGo.Qap.Agent/README.md` 更新配置示例 | 📝 待更新 |
| Swagger 恢复 | NuGet 恢复后取消 Program.cs 中 Swagger 注释 | 📝 待还原 |

---

## 4. 构建待还原

| 项目 | 说明 | 状态 |
|------|------|------|
| Swashbuckle.AspNetCore | NuGet 包 `10.1.7` 待还原（当前使用离线缓存受限版本） | 📝 `MinGo.Qap.Platform.csproj` + `Program.cs` 中 Swagger 相关代码已注释 |
| MinGo.Qap.Agent 编译 | Agent 项目待 NuGet 恢复后验证编译 | 📝 NuGet restore blocked |

---

**说明**：本文档包含 30 个 E2E 验证场景 + 4 项待办工作，覆盖 Agent 生命周期、Scheduler 上报、Job 路由、心跳检测、UI 验证、301 重定向等。每一项均来自 `agent-scheduler-platform` Change 的 Phase 5 待完成任务。
