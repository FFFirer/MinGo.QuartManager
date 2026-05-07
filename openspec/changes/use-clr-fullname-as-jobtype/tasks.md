## 1. Agent — 读取路径简化

- [ ] 1.1 QuartzService.ResolveJobType() 简化为 `jobDetail.JobType?.FullName ?? "unknown"`
- [ ] 1.2 移除 GetJobAsync/GetJobsAsync 中对 ResolveJobType 的调用（已替换为一行）

## 2. Agent — 创建路径适配

- [ ] 2.1 JobRegistry 新增 `GetByFullName(string)` 方法
- [ ] 2.2 QuartzService.CreateJobAsync() 改为按 FullName 查 Registry
- [ ] 2.3 JobConverter 写入 FullName 到 JobDataMap

## 3. 前端 — 类型定义与参数查找

- [ ] 3.1 types/index.ts 补充 `jobTypeFullName` 字段
- [ ] 3.2 JobDetailPage.tsx 参数查找改为匹配 `jobTypeFullName`
- [ ] 3.3 CreateJobPanel.tsx 发送 FullName

## 4. 前端 — JobTypeDisplay 组件

- [ ] 4.1 创建 `JobTypeDisplay.tsx` 组件（截断+tooltip+复制）
- [ ] 4.2 JobsPage.tsx 和 JobDetailPage.tsx 集成新组件

## 5. 验证

- [ ] 5.1 Agent 项目 LSP 诊断通过
- [ ] 5.2 整体解决方案编译通过
