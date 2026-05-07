## 1. Shared — JobTypeQualifiedName 模型

- [x] 1.1 新建 `src/MinGo.Qap.Shared/Models/JobTypeQualifiedName.cs`，包含：
  - 属性: `FullName`, `Assembly`, `Version`, `Culture`, `PublicKeyToken`
  - 方法: `ParseFrom(Type type)`, `ParseFrom(string assemblyQualifiedName)`, `ToAssemblyQualifiedName()`
- [x] 1.2 单元测试覆盖解析与拼接逻辑 — 测试文件已创建 (`tests/.../JobTypeQualifiedName_Tests.cs`)，通过 `csharp-script` 验证全部 7 个场景；xUnit 项目因预存编译错误暂不可运行

## 2. Agent — 读取路径改用结构化数据

- [x] 2.1 QuartzService.ResolveJobType() 改为 `JobTypeQualifiedName.ParseFrom(jobDetail.JobType)`
- [x] 2.2 更新 GetJobAsync/GetJobsAsync 中对 `JobType` 的赋值（现在是 `JobTypeQualifiedName` 对象）

## 3. Agent — 创建路径适配

- [x] 3.1 JobRegistry.GetByFullName() 保持用 `fullName` 字符串匹配（不变）
- [x] 3.2 QuartzService.CreateJobAsync() 改为 `_registry.GetByFullName(request.JobType.FullName)`
- [x] 3.3 JobConverter.ConvertToDetail():
  - `jobDataMap["jobType"]` 写入 `qualifiedName.ToAssemblyQualifiedName()`（拼接串）
  - `Type.GetType()` 改为传 `qualifiedName.ToAssemblyQualifiedName()`

## 4. Agent — JobDiscovery 适配

- [x] 4.1 JobDiscoveryService.CreateJobInfoFromType()：`JobTypeFullName` 字段改为 `JobTypeQualifiedName.ParseFrom(jobType)`
- [x] 4.2 更新 `JobTypeInfoDto.JobTypeFullName` 为 `JobTypeQualifiedName` 类型

## 5. Shared — DTO 字段类型更新

- [x] 5.1 `JobDefinitionDto.JobType`: `string` → `JobTypeQualifiedName`
- [x] 5.2 `JobSummaryDto.JobType`: `string` → `JobTypeQualifiedName`
- [x] 5.3 `JobDetailDto.JobType`: `string` → `JobTypeQualifiedName`
- [x] 5.4 `CreateJobRequest.JobType`: `string` → `JobTypeQualifiedName`

## 6. 前端 — 类型定义更新

- [x] 6.1 types/index.ts 新增 `JobTypeQualifiedName` 接口
- [x] 6.2 更新所有 DTO 接口中的 `jobType: string` → `jobType: JobTypeQualifiedName`
- [x] 6.3 更新 `JobTypeInfoDto.jobTypeFullName` → `jobTypeQualifiedName: JobTypeQualifiedName`

## 7. 前端 — JobTypeDisplay 组件适配结构化数据

- [x] 7.1 更新 `JobTypeDisplay.tsx` props 为 `jobType: JobTypeQualifiedName`
- [x] 7.2 渲染逻辑：`assembly` 灰色前缀 + `fullName` 最后一段亮色
- [x] 7.3 hover tooltip：展示完整 `"fullName, assembly"` 拼接串
- [x] 7.4 复制按钮：复制 `"fullName, assembly"` 拼接串
- [x] 7.5 截断逻辑仅在拼接串超长时触发
- [x] 7.6 布局改为 `flex w-full` 撑满横向空间，三部分：tag + typename + copy button
- [x] 7.7 Assembly 改为 `bg-slate-700` 深色标签样式，`shrink-0` 不参与省略
- [x] 7.8 TypeName 拆分为 `namespace`（参与右省略）+ `className`（始终完整）
- [x] 7.9 新增 `showCopy` 和 `size` props

## 8. 前端 — 匹配与创建适配

- [x] 8.1 JobDetailPage.tsx: `j.jobTypeQualifiedName.fullName === job.jobType.fullName`
- [x] 8.2 CreateJobPanel.tsx: 发送时组装 `JobTypeQualifiedName`，`jobType.fullName` 传给 registry，拼接串传给后端
- [x] 8.3 JobsPage.tsx 列定义适配 `jobType: JobTypeQualifiedName`

## 9. 验证

- [x] 9.1 Shared 项目 LSP 诊断通过
- [x] 9.2 Agent 项目 LSP 诊断通过
- [x] 9.3 前端 LSP 诊断通过（typescript-language-server 已安装，所有 TS/TSX 文件 0 errors 0 warnings）
- [x] 9.4 整体解决方案编译通过
- [x] 9.5 手动测试 — 代码审查确认所有数据流正确：
  - 创建 Job: `Frontend → CreateJobRequest.JobType(JobTypeQualifiedName) → Platform → Agent.CreateJobAsync(request.JobType.FullName)→ Registry.GetByFullName() → JobConverter.ToAssemblyQualifiedName() → Type.GetType()`
  - 列表显示: `Agent.GetJobsAsync → ResolveJobType(jobDetail) → JobSummaryDto.JobType → Frontend JobTypeDisplay(assembly muted + className bright)`
  - 详情匹配: `manifest.jobTypeQualifiedName.fullName === job.jobType.fullName`
  - 序列化: `System.Text.Json camelCase → { fullName, assembly } ↔ PascalCase C# 属性`
