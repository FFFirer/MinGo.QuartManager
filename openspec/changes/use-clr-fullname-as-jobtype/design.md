## Context

当前 `ResolveJobType()` 采用三层 fallback：JobDataMap > Registry(Key) > CLR Name > unknown。三层逻辑叠加了不同注册路径的特殊处理。改为统一用 CLR FullName 后，读取路径变为一行代码。

## Goals / Non-Goals

**Goals:**
- jobType 始终等于 `Type.FullName`，任何路径、任何阶段都一致
- 移除三层 fallback 逻辑
- 前端 UI 优雅展示长名并支持复制

**Non-Goals:**
- 不改变现有 JobDataMap 内容（但新写入用 FullName）
- 不修改 API 路由/响应结构（JobType 字段类型不变）

## Decisions

### 1. 读取路径简化
当前: `ResolveJobType` 16 行三层 fallback
改为: `jobDetail.JobType?.FullName ?? "unknown"` 一行
理由: Quartz `IJobDetail.JobType` 始终携带 CLR 类型信息，无需 fallback

### 2. 创建时 Registry 查找
当前: `_registry.Get(request.JobType)` 按 Key 匹配
改为: `_registry.GetByFullName(request.JobType)` 按 FullName 匹配
理由: 前端发送的是 FullName，需匹配 JobTypeFullName

### 3. 前端参数查找
当前: `manifest.jobs.find(j => j.key === job.jobType)`
改为: `manifest.jobs.find(j => j.jobTypeFullName === job.jobType)`

### 4. UI 显示优化
CLR FullName 可能很长（如 `Sample.Agent.Jobs.ScheduledJob`），需要：
- 默认截断显示（取 `.` 后最后一段 + 灰色前缀路径）
- hover tooltip 展示完整名称
- 复制按钮一键复制完整名称

## Risks / Trade-offs

- **[Breaking] API 直接调用者**：如果外部系统直接 POST `/api/schedulers/{name}/jobs` 发送 manifest key，会因 registry 查找失败报错。需要同步更新调用方
- **[Low] 旧 JobDataMap**：已有 Job 的 `JobDataMap["jobType"]` 存的是旧 key，但读取路径不再使用此值，不影响
- **[Low] 列宽**：UI 表格列可自适应
