## Context

代码库中存在两种时间字段模式：新实体（`Agent`、`SchedulerInfo`、`AgentScheduler`）用 `DateTimeOffset` 映射 PostgreSQL `timestamptz`，旧实体（`Cluster`、`AgentInstance`、`JobDefinition`）用 `DateTime`/`DateTime?`。`PlatformDbContext` 已配置 `DateTimeOffset` 值转换器（写入强制 UTC、读取统一 UTC）和列类型（`timestamptz`），但旧实体不享受此保障。`UtcAuditInterceptor` 用 `DateTimeOffset.UtcNow` 统一赋值导致 `InvalidCastException`。

## Goals / Non-Goals

**Goals:**
- 旧实体的全部时间字段从 `DateTime`/`DateTime?` 改为 `DateTimeOffset`/`DateTimeOffset?`
- 对应 DTO 类型同步更新
- 服务层赋值统一使用 `DateTimeOffset.UtcNow`
- 修复 `UtcAuditInterceptor` CreatedAt 检查逻辑
- 新增 EF Core 迁移，确保数据库列类型与 CLR 类型一致
- 消除 `InvalidCastException` 根因

**Non-Goals:**
- 不改 `HeartbeatDto.Timestamp`（来自 Agent 上报，需 Agent 侧配合）
- 不改日历计算逻辑（`CalendarDto.FireTimes`、`GenerateFireTimes` 是纯展示计算）
- 不引入新的外部依赖
- 不改变外部 HTTP API 的行为语义（JSON 序列化时 `DateTimeOffset` 会输出带时区偏移的格式）

## Decisions

### 1. `DateTimeOffset` 而非 `DateTimeOffset?` 默认值策略

新建实体用 `DateTimeOffset.UtcNow` 作为 `CreatedAt` 默认值，与现有新实体模式一致。`UpdatedAt` 保留 `DateTimeOffset?`（可为 null，表示从未更新）。

### 2. JSON 序列化格式变化

`DateTime` → `DateTimeOffset` 后，System.Text.Json 默认序列化输出从 `"2026-05-07T22:29:06Z"` 变为 `"2026-05-07T22:29:06+00:00"`。这是期望行为——明确包含时区偏移信息。

注意 `ApiResponse<T>.Timestamp` 受此影响，但所有调用方都是 Platform 自己的前端，前端解析 ISO 8601 时两种格式都能处理。

### 3. 数据库迁移策略

PostgreSQL Npgsql Provider 的 `timestamptz` 列类型同时支持 `DateTime` 和 `DateTimeOffset` CLR 类型。因此无需 `ALTER COLUMN` 类型变更，生成迁移用 `migrationBuilder.AlterColumn` 调整 EF Core 元数据即可。

`UtcAuditInterceptor` 的值转换器和第一阶段 UTC 转换对已统一后的实体不产生额外影响——值转换器在写入/读取时自动强制 UTC，第一阶段仅修正非 UTC 值（防御性代码）。

### 4. AgentService 映射调整

现有代码：
```csharp
LastHeartbeat = agent.LastHeartbeat?.UtcDateTime,  // DateTimeOffset → DateTime?
StartedAt = agent.StartedAt.UtcDateTime,            // DateTimeOffset → DateTime
```

DTO 改为 DateTimeOffset 后，直接传值：
```csharp
LastHeartbeat = agent.LastHeartbeat,  // DateTimeOffset → DateTimeOffset?
StartedAt = agent.StartedAt,          // DateTimeOffset → DateTimeOffset
```

## Risks / Trade-offs

- **序列化格式变化风险**: API 输出 `DateTimeOffset` 会比 `DateTime` 多一个时区后缀 `+00:00`。前端 JSON 解析器通常兼容，但若有关键的正则匹配或字符串比较可能受影响 → 低风险、接受
- **AgentInstance 的 `DateTime CreatedAt = DateTime.UtcNow` 默认值**: 改为 `DateTimeOffset CreatedAt = DateTimeOffset.UtcNow`，兼容
- **JobService 中的 `CreatedAt = DateTime.UtcNow`**: 改为 `DateTimeOffset.UtcNow` 前，先移除（让拦截器自动填充），或直接改赋值。由于 `UtcAuditInterceptor` 的 CreatedAt 自动填充存在，可以删除 JobService 中的显式赋值
- **回滚策略**: 数据库迁移可安全回滚（不涉及数据重写），代码回退到之前版本即可
