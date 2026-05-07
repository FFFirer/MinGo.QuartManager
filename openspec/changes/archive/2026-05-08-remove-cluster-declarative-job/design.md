## Context

当前系统存在 Cluster 遗留概念（实体、表、API 端点、枚举、DTO），在 v2 架构重构中已被 Agent+Scheduler 模型取代但仍未完全清理。JobDefinition 实体中的 `ClusterId` 字段实际存储 SchedulerName（通过注释 `// 用 SchedulerName 代替 ClusterId` 说明，但字段名和类型均未正名）。数据库外键约束已在最近迁移中移除，但表和代码引用仍在。

Job 创建流程（`JobService.CreateAsync`）当前为先存 Pending → 调 Agent → 回写 Synced/Failed。此流程方向正确，但需要：
- 去重检查（同 scheduler + jobKey 已 Synced 时拒绝）
- 声明式语义（JobDefinition 代表"声明意图"，而非"调度定义"）
- Agent 侧使用幂等 replace 语义

## Goals / Non-Goals

**Goals:**
- 彻底移除 Cluster 遗留代码（实体、表、API、DTO、枚举、配置文件）
- `JobDefinition.ClusterId` 正名为 `SchedulerName`
- 声明式 Job 创建：去重→存 Pending→调 Agent→回写结果
- Agent 侧 Job 创建改为幂等 replace 语义（`AddJob(replace:true)` + trigger 替换）
- 直接 DROP TABLE `Clusters` 和 `AgentInstances`

**Non-Goals:**
- 不改动 Agent 侧 `CreateJobAsync` 方法名
- 不涉及前端 UI 改造（仅后端清理）
- 不改变现有 Agent 心跳/注册/Scheduler 上报流程
- 不涉及 Quartz 集群模式本身（`SchedulerInfo.IsClustered` 字段保留）

## Decisions

### 1. Cluster 实体直接删除而非软弃用
- **原方案**: 保留 Clusters/AgentInstances 表做历史参考
- **选择**: 直接 DROP TABLE + 删除代码
- **理由**: Cluster 概念已废弃完整周期，v2_migrate_cluster_deprecate.sql 已存在，无活跃依赖（FK 已移除），直接清理更干净

### 2. JobDefinition 保留实体名，ClusterId 正名为 SchedulerName
- **选择**: 不改表名（仍为 `JobDefinitions`），只重命名列 `ClusterId → SchedulerName`
- **理由**: 减少数据库变动范围，避免不必要的表重命名。实体语义从"Cluster 下的定义"变为"Scheduler 上的声明"

### 3. 唯一索引改为 (SchedulerName, JobKey)
- **理由**: 自然匹配声明式创建的去重主键。当前索引 `(ClusterId, JobKey)` 已有唯一性，改列名后重建即可

### 4. 声明式流程：Agent 失败时保留 Failed 记录
- **选择**: Agent 调用失败时不回滚 Platform 侧的 JobDefinition 记录，而是标记为 Failed 保留
- **理由**: 失败记录可追踪、可重试，避免静默丢失创建意图

### 5. Agent API 端点 POST→PUT
- **选择**: 将 `POST /api/agent/jobs` 改为 `PUT /api/agent/jobs` 并内部用 `AddJob(replace:true)`
- **理由**: PUT 语义对应幂等替换，符合 REST 最佳实践。方法名 `CreateJobAsync` 保留不改为 `ReplaceJobAsync`（用户要求）

### 6. 已 Synced 声明重复时 409 Conflict
- **选择**: 当同 (schedulerName, jobKey) 的 JobDefinition 已 Synced，拒绝新声明返回 409
- **理由**: 声明式语义应明确：已同步成功的声明不可重复提交。如需要更新应走独立更新流程

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| DROP TABLE 可能导致数据丢失 | Cluster/AgentInstance 表数据已是遗留数据（Agent 已迁移到新 Agents 表），且已有数据迁移 SQL 作为备份 |
| 生产环境已有 JobDefinitions 数据含 ClusterId=SchedulerName | 列重命名不丢数据；唯一索引重建前确认无重复 |
| Agent 还在用 POST 创建 Job | 同步更新 Platform 侧 AgentProxyService 调用路径为 PUT，Agent 端同时放行 PUT 端点 |
| 前端 Cluster 页面可能未清理 | 暂不涉及前端改造，仅清理后端 API；前端页面无数据时自然降级 |
