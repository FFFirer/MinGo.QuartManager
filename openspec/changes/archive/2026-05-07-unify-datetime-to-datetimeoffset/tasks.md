## 1. 实体 DateTime 字段改为 DateTimeOffset

- [x] 1.1 修改 `Cluster.cs`：LastHeartbeat `DateTime?` → `DateTimeOffset?`，CreatedAt `DateTime` → `DateTimeOffset`，UpdatedAt `DateTime?` → `DateTimeOffset?`，DeletedAt `DateTime?` → `DateTimeOffset?`；默认值 `DateTimeOffset.UtcNow`
- [x] 1.2 修改 `AgentInstance.cs`：LastHeartbeat `DateTime?` → `DateTimeOffset?`，StartedAt `DateTime?` → `DateTimeOffset?`，CreatedAt `DateTime` → `DateTimeOffset`（默认值 `DateTimeOffset.UtcNow`），UpdatedAt `DateTime?` → `DateTimeOffset?`，DeletedAt `DateTime?` → `DateTimeOffset?`
- [x] 1.3 修改 `JobDefinition`（在 `Cluster.cs` 底部）：CreatedAt `DateTime` → `DateTimeOffset`，UpdatedAt `DateTime?` → `DateTimeOffset?`

## 2. DTO DateTime 字段改为 DateTimeOffset

- [x] 2.1 修改 `AgentInstanceDto.cs`：`AgentInstanceDto` 和 `AgentSummaryDto` 中所有 `DateTime` 字段（LastHeartbeat、StartedAt、CreatedAt、UpdatedAt）改为 `DateTimeOffset`
- [x] 2.2 修改 `JobDtos.cs`：`JobDefinitionDto` 中 CreatedAt `DateTime` → `DateTimeOffset`，UpdatedAt `DateTime?` → `DateTimeOffset?`
- [x] 2.3 修改 `DashboardDtos.cs`：`DashboardDto.LastUpdated` `DateTime` → `DateTimeOffset`，`ClusterDashboardDto.CreatedAt`/`LastUpdated` `DateTime` → `DateTimeOffset`，`UpcomingJobDto.NextFireTime` `DateTime` → `DateTimeOffset`
- [x] 2.4 修改 `ApiResponse.cs`：`Timestamp` `DateTime` → `DateTimeOffset`，默认值 `DateTimeOffset.UtcNow`

## 3. 服务层赋值和映射更新

- [x] 3.1 修改 `JobService.cs`：所有 `DateTime.UtcNow` 改为 `DateTimeOffset.UtcNow`（4 处）；`MapToDto` 中 CreatedAt/UpdatedAt 直接传递（类型已一致，无需转换）
- [x] 3.2 修改 `AgentService.cs`：`MapToSummary` 中 `.UtcDateTime` 调用改为直接传值（`agent.LastHeartbeat`、`agent.StartedAt`）
- [x] 3.3 修改 `DashboardController.cs`：`DateTime.UtcNow` → `DateTimeOffset.UtcNow`（2 处）+ `DateTime.MinValue` → `DateTimeOffset.MinValue`

## 4. 修复 UtcAuditInterceptor

- [x] 4.1 保留 `utcNow = DateTimeOffset.UtcNow`（所有实体已统一为 DateTimeOffset，类型匹配无需降级）
- [x] 4.2 修复 `CreatedAt` 自动填充的 null/default 检查：增加 `currentValue is DateTime dt && dt == default` 分支以兼容 DateTime/DateTimeOffset 两种类型

## 5. 数据库迁移

- [x] 5.1 运行 `dotnet ef migrations add UnifyDateTimeToDateTimeOffset --project src/MinGo.Qap.Platform` 生成迁移
- [x] 5.2 验证生成的迁移文件不包含不必要的 `ALTER COLUMN TYPE` 语句（列类型已是 `timestamptz`）

## 6. 验证

- [x] 6.1 运行 `dotnet build` 确认编译通过
- [x] 6.2 确认无测试项目（跳过）
- [x] 6.3 检查 `lsp_diagnostics`：无源文件诊断错误（仅 obj/ 目录预存重复特性问题）
