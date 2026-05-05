## Context

Create Job 表单是用户创建 Quartz 调度任务的核心入口。当前表单存在三个独立问题：

1. **UI 宽度不足**：`SlidePanel` 固定 `w-96` (384px)，Step 2 参数配置和 Step 3 调度配置的内容较多，在宽屏下体验差
2. **Manifest 数据源断裂**：`ManifestController` 使用静态内存缓存，但没有 Agent 自动上报机制。前端 `manifestApi.get()` 总是返回空列表，导致 Step 1 看不到 Job 类型、Step 2 无参数可配
3. **Job 参数无元数据**：样本 `EchoJob` 和 `DelayJob` 从 `JobDataMap` 读取参数，但未标注 `[JobParameter]` 特性，Agent 的 `JobDiscoveryService` 无法发现参数，manifest 的 `parameters` 列表为空

## Goals / Non-Goals

**Goals:**
- ManifestController 在本地缓存未命中时，通过 AgentProxy 从 Agent 实时拉取 manifest
- CreateJobPanel 宽度在 `>=1024px` 视口下扩展至 `max-w-2xl` (672px)
- CreateJobModal 同步宽度更改
- EchoJob、DelayJob 的 `[JobParameter]` 标注生效，manifest 中包含参数元数据

**Non-Goals:**
- 不改变 ManifestController 的 POST 逻辑（Agent 主动上报仍可用）
- 不改变 JobConverter 或 QuartzService 的 CreateJobAsync 逻辑（JobDataMap 写入已正确）
- 不涉及 Agent 端的 manifest 主动上报至 Platform（作为后续优化，暂不实现）
- 不引入新的 UI 组件或重构表单步骤逻辑

## Decisions

### Decision 1: ManifestController 缓存优先 + Agent 转发降级

- **方案**: GET 时先查静态缓存，有则直接返回；无则调用 `IAgentProxyService.GetAsync<JobManifestDto>(schedulerName, "agent/manifest")` 转发到 Agent，结果写入缓存后返回
- **为什么**:
  - 最小改动，不改变现有 POST 上报逻辑
  - AgentProxy 已有完整的 Agent 选择、超时、错误处理
  - 缓存可避免重复转发、降低延迟
- **备选方案**:
  - Agent 主动上报 manifest 到 Platform（更优但改动大，需修改 HostedAgentService）
  - ManifestController 每次请求都转发（简单但延迟高，无缓存）

### Decision 2: SlidePanel 宽度使用 Tailwind 响应式类

- **方案**: `width="w-full max-w-2xl"`，小屏下占满宽度，`>=1024px` 时限制最大 672px
- **为什么**:
  - 纯 CSS 解决方案，无 JS 逻辑
  - SlidePanel 的 `width` prop 直接拼接进 className，Tailwind 类名可直接生效
  - `max-w-2xl`(42rem) 足够容纳表单内容，且与 Modal 版本保持一致

### Decision 3: [JobParameter] 标注在属性上而非构造函数参数

- **方案**: EchoJob 和 DelayJob 的属性添加 `[JobParameter]` 特性（而不是构造函数参数）
- **为什么**:
  - 当前 Job 在 Execute 方法中通过 `context.MergedJobDataMap["key"]` 读取参数，属性标注更直观
  - `JobDiscoveryService.DiscoverParameters()` 优先扫描属性上的特性
  - 构造函数参数通常用于 DI（如 ILogger），不适合标注 Job 参数

### Decision 4: CreateJobModal 同步修改

- **方案**: Modal 的 SlidePanel 宽度也改为 `w-full max-w-2xl`
- **为什么**: 保持两个 Create Job 入口的 UI 一致性。Modal 虽当前未在 JobsPage 中使用，但作为备用入口应保持同步

## Risks / Trade-offs

- **[延迟] Agent 不可用时 Manifest 转发会失败** → 缓存未命中时返回空 manifest（与现有行为一致），前端显示"无可用 Job 类型"。ManifestController 应捕获 AgentException 并优雅降级
- **[一致性] 缓存可能过时** → 当前粒度足够（session 级别），Agent 重启后下次 GET 请求会重新拉取。不做缓存失效策略
- **[侵入性] Job 开发者需手动标注 [JobParameter]** → 文档化推荐做法
