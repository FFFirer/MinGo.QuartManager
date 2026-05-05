## 1. Platform Manifest 转发

- [x] 1.1 ManifestController 注入 `IAgentProxyService` 依赖
- [x] 1.2 ManifestController.Get 增加缓存未命中时的 Agent 转发逻辑（捕获 AgentException 优雅降级）

## 2. 前端面板宽度调整

- [x] 2.1 CreateJobPanel width 从 `w-96` 改为 `w-full max-w-2xl`
- [x] 2.2 CreateJobModal width 确认已为 `max-w-2xl`（无需修改）

## 3. 样本 Job 参数标注

- [x] 3.1 EchoJob 添加 `Message` 属性并标注 `[JobParameter("message")]`
- [x] 3.2 DelayJob 添加 `DelaySeconds` 属性并标注 `[JobParameter("delaySeconds")]`（默认值 5）

## 4. 验证

- [x] 4.1 LSP diagnostics 验证无编译错误
- [x] 4.2 `dotnet build` 验证构建通过（Platform + Sample.Jobs 构建成功）
