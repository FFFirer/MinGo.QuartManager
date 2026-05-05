## Why

Create Job 表单目前存在三个问题：右侧面板在宽屏下过窄(384px)；Scheduler 的 Job 类型定义（manifest）未能在表单中展示，因为 Platform 的 manifest 接口仅依赖可能为空的内存缓存；样本 Job 未标注 `[JobParameter]` 特性，导致 Agent 无法自动发现参数元数据，表单中无法动态渲染参数输入、参数也无法正确写入 JobDataMap。

## What Changes

- **ManifestController 增加 Agent 转发**：Platform `GET /api/schedulers/{name}/manifest` 在缓存未命中时，通过 `IAgentProxyService` 转发到 Agent 实时获取 manifest，结果写入缓存后返回
- **CreateJobPanel 宽度调整为响应式**：从固定 `w-96`(384px) 改为 `w-full max-w-2xl`(最大 672px)，适配大屏
- **样本 Job 补充 [JobParameter] 特性**：`EchoJob.message` 和 `DelayJob.delaySeconds` 添加 `[JobParameter]` 标注，使 Agent 能自动发现参数元数据，前端表单 Step 2 动态渲染对应输入控件
- **CreateJobModal 同步宽度**：保持与 Panel 一致的大屏宽度

## Capabilities

### New Capabilities
- `manifest-proxy`: Platform 端 ManifestController 转发请求到 Agent 实时获取 Job 类型定义，作为内存缓存的补充/降级策略
- `job-parameter-annotation`: 通过 `[JobParameter]` 特性标注 Job 参数，使 Agent 自动发现参数元数据并生成 manifest，驱动前端表单动态渲染

### Modified Capabilities

无。本项目首次建立 specs。

## Impact

- **ManifestController.cs**: 新增 `IAgentProxyService` 依赖，GET 逻辑增加缓存未命中时的 Agent 转发
- **CreateJobPanel.tsx**: 修改 width prop 值
- **CreateJobModal.tsx**: 同步修改 width prop 值（可选，但建议保持一致性）
- **EchoJob.cs / DelayJob.cs**: 属性或构造函数参数添加 `[JobParameter]` 特性
- **文档**: 建议补充 Job 编写规范，说明参数标注方式
