## Context

创建 Job 表单（`CreateJobPage.tsx`）是用户最常用的功能页面之一。当前实现存在三个独立的用户体验缺陷：

1. **Identity 布局**：Group 在左列 Name 在右列，与用户输入直觉（先关注 "做什么" 再关注 "归属"）不符。Name 只拦截了 `.` 字符，缺少完整的合法性校验。
2. **Parameters 默认值**：Manifest 返回的 `ParameterInfoDto.default` 只用于输入框的显示值回填，而未写入 `params` state。`validate()` 检查 `params[p.name] === undefined` 时报必填错误。
3. **storeNonDurableWhileAwaitingScheduling**：后端的 `QuartzService.CreateJobAsync` 对 Schedule=None 始终传递 `storeNonDurableWhileAwaitingScheduling: true`，与 `storeDurable` 选项状态无关。

三个问题各自独立、无耦合，可分别修复。

## Goals / Non-Goals

**Goals:**
- Job Identity 块 Name 在前 Group 在后，Name 输入仅允许 `[a-zA-Z0-9\-_]`
- 带默认值的参数在选完 Job Type 后自动填入默认值，不修改不报错
- 后端在 Schedule=None 且 storeDurable=false 时才传 `storeNonDurableWhileAwaitingScheduling: true`

**Non-Goals:**
- 不改变表单整体结构
- 不新增参数类型支持
- 不改动 API 契约（DTO 结构不变）
- 不改动 Quartz.NET 版本或调度行为
- 不改动已有 spec 的非相关场景（如 cron、interval、once schedule）

## Decisions

### Decision 1: Name/Group 布局交换 + 正则校验

**方案**：在 `CreateJobPage.tsx` 中交换 grid 两列的顺序，Name 输入框变为第一列。添加 `useMemo` 计算的 `nameError` 状态，在 `validate()` 中增加正则检查。

**为什么不是后端校验**：Name 合法性是 UI 约束（Group.Name 组合成 JobKey），前端拦截更快、体验更好。后端通过 AgentProxy 转发到 Quartz 时也会获得原生错误。

**正则规则**：`/^[a-zA-Z0-9\-_]+$/`。错误消息："Job name只能包含字母、数字、-和_"。

### Decision 2: 参数默认值预填

**方案**：在 `handleJobTypeChange` 中，选择新 JobType 时遍历 `selectedJob.parameters`，对每个有 `default` 值的参数写入 `params[name] = param.default`。

**细节**：
- 对 `bool` 类型，`default` 可能是 `"true"/"false"` 字符串或布尔值，按原样写入
- 对 `int` 类型，`default` 可能是数字或数字字符串，按原样写入
- 对 `json/object` 类型，`default` 可能是对象或字符串，按原样写入
- `validate()` 逻辑不变（`params[p.name]` 已有值，不再 undefined）

**边界**：
- 当 `param.default === undefined`，不写入，保持 params 中该 key 未设置
- copyFrom 场景：copySource 的 params 会覆盖默认值（useEffect 后执行），优先级正确

### Decision 3: storeNonDurableWhileAwaitingScheduling 条件化

**方案**：修改 `QuartzService.CreateJobAsync` 中 Schedule=None 的分支，从：
```csharp
await scheduler.AddJob(jobDetail, replace: true, storeNonDurableWhileAwaitingScheduling: true);
```
改为：
```csharp
var isDurable = request.Options?.StoreDurable == true;
if (isDurable)
{
    await scheduler.AddJob(jobDetail, replace: true);  // Job 已是 durable，无需特殊标志
}
else
{
    await scheduler.AddJob(jobDetail, replace: true, storeNonDurableWhileAwaitingScheduling: true);
}
```

**为什么**：`storeNonDurableWhileAwaitingScheduling` 是 Quartz.NET 中用于让非持久化 Job 在无 Trigger 时临时保留的标志。当 job 已标记 `StoreDurably(true)` 时这个标志不产生额外效果，但语义上更清晰地区分了两种场景。

## Risks / Trade-offs

| 风险 | 缓解措施 |
|------|----------|
| Name 正则可能过于严格，某些合法字符被拦截 | 正则规则与 Quartz 的 JobKey 命名规则对齐；用户可反馈放宽 |
| 参数默认值预填导致 copyFrom 场景下的预期行为混乱 | copyFrom useEffect 在 handleJobTypeChange 之后执行，会覆盖默认值，优先级正确 |
| storeNonDurableWhileAwaitingScheduling 条件化在 Quartz.NET 新版本中语义变化 | 当前版本 Quartz.NET 3.17.1，此参数语义稳定；升级时需回归测试 |
