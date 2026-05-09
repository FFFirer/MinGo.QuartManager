## 1. Shared — JobKeyDto 定义与 DTO 更新

- [x] 1.1 新建 `MinGo.Qap.Shared/Models/JobKeyDto.cs` — `readonly record struct JobKeyDto(string Name, string Group = "DEFAULT")`，包含 `ToString()` 和 `JsonConverter`
- [x] 1.2 更新 `CreateJobRequest`: `JobKey: string` → `JobKeyDto`
- [x] 1.3 更新 `JobDefinitionDto`: `JobKey: string` → `JobKeyDto`，移除独立 `Group` 字段
- [x] 1.4 更新 `JobSummaryDto`: `JobKey: string` → `JobKeyDto`，移除独立 `Group` 字段
- [x] 1.5 更新 `JobDetailDto`: `JobKey: string` → `JobKeyDto`，移除独立 `Group` 字段
- [x] 1.6 更新 `ExecutionLogDto`: `JobKey: string` → `JobKeyDto`

## 2. DB 迁移 — JobDefinition 实体

- [x] 2.1 更新 `JobDefinition` entity：添加 `Name` 属性，更新索引
- [x] 2.2 更新 `PlatformDbContext.OnModelCreating`：添加唯一索引 `(SchedulerName, Group, Name)`
- [x] 2.3 生成并应用 EF Core migration：添加 Name 列、回填数据、新索引

## 3. Platform — Controller + Service

- [x] 3.1 更新 `JobsController.cs`：所有端点路由 `{jobKey}` → `{name}/{group?}`，签名 `string jobKey` → `string name, string? group`
- [x] 3.2 更新 `IJobService` / `JobService.cs`：所有方法签名 `string jobKey` → `JobKeyDto jobKey`
- [x] 3.3 删除 `JobService.ParseJobKey()`，DB 查询改为 `j.Name == name && j.Group == group`
- [x] 3.4 更新 `AgentProxyService` URL 构造：`jobs/{jobKey}` → `jobs/{name}` 或 `jobs/{name}/{group}`

## 4. Agent — API + Services

- [x] 4.1 更新 `AgentApiExtensions.cs`：所有 handler 路由 `{jobKey}` → `{name}/{group?}`，签名使用 `JobKeyDto`
- [x] 4.2 更新 `IQuartzService` / `QuartzService.cs`：方法签名 `string jobKey` → `JobKeyDto jobKey`，删除 `ParseJobKey()`，直接 `new JobKey(dto.Name, dto.Group)`
- [x] 4.3 更新 `IJobConverter` / `JobConverter.cs`：`ConvertToDetail` 参数 `string jobKey` → `JobKeyDto jobKey`，删除 `ParseJobKey()`
- [x] 4.4 更新 `QapJobListener.cs`：日志中使用 `jobKey.ToString()`（保留 "Group.Name" 可读性）

## 5. 前端 — Types + API

- [x] 5.1 更新 `types/index.ts`：添加 `JobKeyDto` 接口，更新所有 DTO 接口，删除 `parseJobKey()`，添加 `formatJobKey()` 辅助
- [x] 5.2 更新 `api/index.ts`：所有 API URL 构造改为 `/${name}` 或 `/${name}/${group}`，添加 `buildJobUrl()` 辅助

## 6. 前端 — Routes + Pages

- [x] 6.1 更新 `App.tsx`：路由 `:jobKey` → `:name/:group?`
- [x] 6.2 更新 `CreateJobPage.tsx`：提交时使用 `{name, group}` 对象，不拼接字符串
- [x] 6.3 更新 `JobsPage.tsx`：选中 key 使用复合 key 字符串，URL 导航使用 `/${name}/${group}`
- [x] 6.4 更新 `JobDetailPage.tsx`：`useParams` 改为 `{name, group}`，URL 构造改为新格式

## 7. 验证

- [x] 7.1 运行 `dotnet build` 验证后端编译通过
- [x] 7.2 运行 LSP diagnostics 检查无错误
- [x] 7.3 前端 `pnpm build` 验证编译通过
