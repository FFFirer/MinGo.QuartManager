## ADDED Requirements

### Requirement: API URL 使用 `{name}/{group?}` 格式
所有涉及 JobKey 的 API 端点 SHALL 将 URL 路径从 `{jobKey}`（接收 `"Group.Name"` 字符串）改为 `{name}/{group?}`（group 可选，默认 `"DEFAULT"`）。

Platform JobsController 端点变更：

| 方法 | 当前路由 | 新路由 |
|------|---------|--------|
| GET 详情 | `/api/schedulers/{schedulerName}/jobs/{jobKey}` | `/api/schedulers/{schedulerName}/jobs/{name}/{group?}` |
| PUT 更新 | `/api/schedulers/{schedulerName}/jobs/{jobKey}` | `/api/schedulers/{schedulerName}/jobs/{name}/{group?}` |
| DELETE 删除 | `/api/schedulers/{schedulerName}/jobs/{jobKey}` | `/api/schedulers/{schedulerName}/jobs/{name}/{group?}` |
| POST 触发 | `/api/schedulers/{schedulerName}/jobs/{jobKey}/trigger` | `/api/schedulers/{schedulerName}/jobs/{name}/{group?}/trigger` |
| POST 暂停 | `/api/schedulers/{schedulerName}/jobs/{jobKey}/pause` | `/api/schedulers/{schedulerName}/jobs/{name}/{group?}/pause` |
| POST 恢复 | `/api/schedulers/{schedulerName}/jobs/{jobKey}/resume` | `/api/schedulers/{schedulerName}/jobs/{name}/{group?}/resume` |

Agent Minimal API 端点做同样变更。

#### Scenario: 请求使用默认 Group
- **WHEN** 前端请求 `GET /api/schedulers/sch1/jobs/myJob`
- **THEN** Controller 收到 `name="myJob"`, `group=null`，构造 `new JobKeyDto("myJob")` → Group="DEFAULT"

#### Scenario: 请求使用自定义 Group
- **WHEN** 前端请求 `GET /api/schedulers/sch1/jobs/myJob/CustomGroup`
- **THEN** Controller 收到 `name="myJob"`, `group="CustomGroup"`，构造 `new JobKeyDto("myJob", "CustomGroup")`

#### Scenario: Trigger/Pause/Resume 操作 URL
- **WHEN** 触发 DEFAULT group 的 Job
- **THEN** URL 为 `POST /api/schedulers/sch1/jobs/myJob/trigger`
- **WHEN** 触发自定义 group 的 Job
- **THEN** URL 为 `POST /api/schedulers/sch1/jobs/myJob/CustomGroup/trigger`

### Requirement: 删除所有 ParseJobKey() 方法
系统 SHALL 删除以下文件中的 `ParseJobKey()` 私有方法：
- `Platform/Services/JobService.cs`
- `Agent/Services/QuartzService.cs`
- `Agent/Services/JobConverter.cs`

这些方法的功能 SHALL 由直接构造 `JobKeyDto` 替代。

#### Scenario: QuartzService 直接构造 JobKey
- **WHEN** `QuartzService` 需要创建 Quartz `JobKey`
- **THEN** 使用 `new Quartz.JobKey(dto.Name, dto.Group)` 而非解析字符串

#### Scenario: JobService 直接使用 Group 和 Name
- **WHEN** `JobService` 需要查询 DB
- **THEN** 使用 `j.Name == name && j.Group == group` 而非 `j.JobKey == jobKey`

### Requirement: 前端 React Router 路由支持可选 group 段
前端路由 SHALL 支持 `:group?` 可选参数：

```tsx
<Route path="/schedulers/:schedulerName/jobs/:name" element={<JobDetailPage />} />
<Route path="/schedulers/:schedulerName/jobs/:name/:group" element={<JobDetailPage />} />
```

前端 API 调用层 SHALL 在 `group === "DEFAULT"` 时省略 URL 中的 group 段。

#### Scenario: 导航到 DEFAULT group 的 Job 详情
- **WHEN** 页面导航到 `/schedulers/sch1/jobs/myJob`
- **THEN** `useParams()` 返回 `{name: "myJob", group: undefined}`，在页面中视为 DEFAULT

#### Scenario: 导航到自定义 group 的 Job 详情
- **WHEN** 页面导航到 `/schedulers/sch1/jobs/myJob/CustomGroup`
- **THEN** `useParams()` 返回 `{name: "myJob", group: "CustomGroup"}`
