## Why

生产环境因 DateTime/DateTimeOffset 混用导致 `InvalidCastException`（`DateTimeOffset` 无法赋值给 `DateTime?`）。代码库中旧实体（Cluster、AgentInstance、JobDefinition）使用 `DateTime` 时间字段，而新实体（Agent、SchedulerInfo）和 `UtcAuditInterceptor` 使用 `DateTimeOffset`。统一为 `DateTimeOffset` 消除类型不一致、修复拦截器崩溃，同时利用 PostgreSQL `timestamptz` 列类型获得时区感知能力。

## What Changes

1. **Entity types**: Cluster、AgentInstance、JobDefinition 的所有时间字段从 `DateTime`/`DateTime?` 改为 `DateTimeOffset`/`DateTimeOffset?`
2. **DTO types**: 对应的 DTO（AgentInstanceDto、JobDefinitionDto、DashboardDtos、ApiResponse 等）同步更新为 `DateTimeOffset`
3. **Service assignments**: JobService、DashboardController 中的 `DateTime.UtcNow` 改为 `DateTimeOffset.UtcNow`
4. **Database migration**: 新增 EF Core 迁移同步列映射（CLR 类型变更，列定义 `timestamptz` 不变）
5. **Interceptor**: 修复 `UtcAuditInterceptor` 中 CreatedAt 的 null/default 检查（兼容 DateTimeOffset）
6. **Agent 响应映射**: 取消 AgentService 中不必要的 `.UtcDateTime` 调用（DTO 改为 DateTimeOffset 后直接传值）

## Capabilities

### New Capabilities
- `datetime-consistency`: 所有实体时间字段统一使用 DateTimeOffset，确保时区感知、EF Core 值转换器全局生效、拦截器正常工作

### Modified Capabilities

无。此为基础设施级重构，不改变外部 API 行为。

## Impact

- **Platform/Data/Entities**: 3 个实体（Cluster、AgentInstance、JobDefinition）共 11 个字段变更
- **Shared/Models**: 7 个 DTO（AgentInstanceDto、AgentSummaryDto、JobDefinitionDto、ClusterDashboardDto、DashboardDto、UpcomingJobDto、ApiResponse）字段变更
- **Platform/Services**: JobService 4 处赋值变更 + AgentService 序列化调整
- **Platform/Controllers**: DashboardController 赋值变更
- **Platform/Data**: UtcAuditInterceptor CreatedAt 检查修复
- **Database**: 新增迁移（PostgreSQL timestamptz 列类型不变，仅 CLR 映射变更）
- **API 响应**: `ApiResponse.Timestamp` 从 `DateTime` 变为 `DateTimeOffset`，JSON 序列化输出格式会略有变化（带时区偏移）
