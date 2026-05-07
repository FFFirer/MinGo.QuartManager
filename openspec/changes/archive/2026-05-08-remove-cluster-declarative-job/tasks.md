## 1. Shared DTO 清理

- [x] 1.1 删除 `ClusterStatus` 枚举（LegacyEnums.cs）
- [x] 1.2 `DashboardDtos.cs`: 删除 `TotalClusters`、`Clusters`、`ClusterSummaryItem`、`ClusterDashboardDto`、`CalendarDto`、`CalendarJobDto`；`UpcomingJobDto` 删除 `ClusterId`/`ClusterName`
- [x] 1.3 `AgentInstanceDto.cs`: 删除 `ClusterId` 字段
- [x] 1.4 `AgentRegistrationResponse.cs`: 删除 `ClusterId` 字段
- [x] 1.5 `IAgentRegistry.cs`: `AgentRegistrationRequest` 删除 `ClusterId` 参数

## 2. Platform 实体和 DbContext 清理

- [x] 2.1 删除 `Entities/Cluster.cs`
- [x] 2.2 删除 `Entities/AgentInstance.cs`
- [x] 2.3 `PlatformDbContext.cs`: 移除 Cluster 和 AgentInstance 的 `DbSet` 和 `OnModelCreating` 配置

## 3. JobDefinition 重构

- [x] 3.1 `Entities/JobDefinition.cs`: `ClusterId` → `SchedulerName`，新增 `Group` 和 `ResultJson` 字段，更新注释
- [x] 3.2 `PlatformDbContext.cs`: 更新 JobDefinition 配置（属性名、唯一索引 `(SchedulerName, JobKey)`、`SchedulerName` 最大长度等）

## 4. JobService 声明式创建改造

- [x] 4.1 重写 `JobService.CreateAsync`: 添加去重检查逻辑（Synced→409, Pending→更新, Failed→重试）
- [x] 4.2 `JobService.cs`: 所有 `ClusterId` 引用改为 `SchedulerName`
- [x] 4.3 `JobService.cs`: Agent 成功后回写 `ResultJson`（序列化 `JobDetailDto`）

## 5. Controller 清理

- [x] 5.1 删除 `Controllers/Old/OldClustersRedirectController.cs`
- [x] 5.2 `DashboardController.cs`: 删除 `GetClusterDashboard`、`GetClusterCalendar`、`GenerateFireTimes` 方法

## 6. Agent 侧幂等替换改造

- [x] 6.1 `AgentApiExtensions.cs`: `POST /api/agent/jobs` → `PUT /api/agent/jobs`
- [x] 6.2 `QuartzService.cs`: `CreateJobAsync` 内部改为 `AddJob(jobDetail, replace: true)` + trigger 替换的幂等语义

## 7. 数据库迁移

- [x] 7.1 运行 `dotnet ef migrations add RemoveClusterAndDeclarativeJob` 生成迁移文件
- [x] 7.2 验证生成的迁移 SQL 包含：DROP Clusters 表、DROP AgentInstances 表、列重命名、索引重建

## 8. 杂项清理

- [x] 8.1 `README.md`: 架构图无 Cluster 引用，无需改动
- [x] 8.2 删除 `docs/migrations/v2_migrate_cluster_deprecate.sql`
