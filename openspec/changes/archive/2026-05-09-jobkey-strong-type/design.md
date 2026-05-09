## Context

MinGo.Qap 是一个 Quartz.NET 分布式 Agent 管理平台，JobKey/TriggerKey 在整个系统中作为标识符传递。当前设计中，JobKey 以 `"Group.Name"` 字符串格式存在，从前端表单到后端 Controller、Service、DB、Agent 代理、最终到 Quartz.NET 原生 `JobKey` 对象。这条链路上存在 3 个不同实现的 `ParseJobKey()` 方法，解析逻辑不一致（Platform 返回 `(group, name)`，Agent 返回 `(name, group)`）。

本次变更引入强类型 `JobKeyDto` 来消除所有字符串解析，统一数据传递方式。

## Goals / Non-Goals

**Goals:**
- 新增 `JobKeyDto` 值类型，包含 `Name`（必填）+ `Group`（默认 `"DEFAULT"`）
- 删除所有 `ParseJobKey()` 方法
- API URL 改为 `{name}/{group?}` 格式（不再使用 `{jobKey}`）
- 前后端所有 DTO 使用 `JobKeyDto` 代替 `string JobKey`
- TriggerKey 由 JobKey 确定性派生，保持内部实现

**Non-Goals:**
- 不改动其他 Quartz 原生概念（如 Schedule、MisfirePolicy 等）
- 不改动 Agent 的 Scheduler 管理逻辑
- 不做完整 API 版本化（v2）

## Decisions

### D1: JobKeyDto 类型选择 — `readonly record struct`

**选择**: `public readonly record struct JobKeyDto(string Name, string Group = "DEFAULT")`

**理由**:
- 值语义：两个 JobKeyDto 相等当且仅当 Name 和 Group 都相等
- 不可变：创建后不能修改，适合作为标识符
- 紧凑：struct 在栈上分配，无堆分配开销
- `Group = "DEFAULT"` 默认值使绝大多数调用点不需要指定 group

**替代方案**: class → 引用类型需要重写 Equals/GetHashCode，且堆分配有额外开销

### D2: URL 路由模式 — `{name}/{group?}`

**选择**: ASP.NET Core 单路由模板 `[HttpGet("{name}/{group?}")]`，在 Controller 层 `group ?? "DEFAULT"`

**理由**:
- ASP.NET Core 原生支持可选路由参数（`?` 后缀），URL 中无 group 段时参数为 null
- 无需双路由定义，减少模板代码
- React Router v6 同样支持 `:group?` 可选段

**URL 示例**:
| 场景 | URL |
|------|-----|
| DEFAULT group | `GET /api/schedulers/sch1/jobs/myJob` |
| 自定义 group | `GET /api/schedulers/sch1/jobs/myJob/CustomGroup` |

### D3: DB 迁移策略 — 添加 Name 列，保留 JobKey 为过渡

**选择**: 
1. 添加 `Name` 列（string, required）
2. 从现有 `JobKey` 数据回填 `Name`（`SPLIT_PART(JobKey, '.', 2)`）
3. JobKey 列保留为普通列（非 computed），代码不再写入
4. 新增唯一索引 `(SchedulerName, Group, Name)`
5. 后续清理：择机删除 JobKey 列

**理由**:
- 零停机迁移：现有数据完整保留
- 渐进式：代码优先切换到新列，DB 列延迟清理
- 不使用 computed column：EF Core 迁移更简单，且 computed column 在 Postgres 中行为受限

### D4: 前端批次选择 — 复合 key 字符串

**选择**: `Set<string>` 使用 `\x1F`（Unit Separator）分隔 Name 和 Group，如 `"myJob\x1FDEFAULT"`

**理由**:
- Set<string> 是 React 中常见的多选模式，改动最小
- `\x1F` 是 ASCII 控制字符，不可能出现在用户输入的 name/group 中
- 展示时只需 `key.split('\x1F')` 还原

**替代方案**: `Map<string, JobKeyDto>` → 按引用比较而非值比较，React 中更新麻烦

### D5: TriggerKey 处理 — 保持内部，不引入 DTO

**选择**: TriggerKey 继续在 Agent 内部由 JobKey 派生生成，不暴露到 API 层

**理由**:
- Trigger 与 Job 在系统中是 `1:1` 关系（每个 Job 最多一个 Trigger）
- TriggerKey 格式为 `{Name}_trigger` + `{Group}`，是确定性算法
- 没有任何 API 端点需要按 TriggerKey 查询或操作
- 引入 TriggerKeyDto 会增加复杂度但无实际收益

### D6: JSON 序列化 — 自定义 JsonConverter

**选择**: 自定义 `JobKeyDtoJsonConverter`，序列化为 `{"name":"...", "group":"..."}` 对象

**理由**:
- 默认 `readonly record struct` 序列化为 `{"name":"...", "group":"..."}` 已满足需求
- 自定义 converter 可选：用于统一 null 处理、空字符串保护等
- 前端接收到的 JSON 直接匹配 TypeScript `JobKeyDto` 接口

### D7: 前端 URL 构造

**选择**: API 调用层（`api/index.ts`）负责 URL 构造，`group === "DEFAULT"` 时省略

```typescript
// api/index.ts
const buildJobUrl = (schedulerName: string, name: string, group?: string) => {
  const base = `/api/schedulers/${encodeURIComponent(schedulerName)}/jobs/${encodeURIComponent(name)}`;
  return group && group !== 'DEFAULT' ? `${base}/${encodeURIComponent(group)}` : base;
};
```

## Risks / Trade-offs

| 风险 | 缓解措施 |
|------|---------|
| [R1] name 中包含 `/` 导致路由歧义 | 前端限制 name 为 `[a-zA-Z0-9\-_]`，后端也做校验 |
| [R2] 现有 API 客户端在部署期间因 URL 变化而失败 | 这是 BREAKING change，需协调前后端同时部署 |
| [R3] DB 迁移回填 Name 列可能出错（JobKey 格式不规范） | 迁移脚本处理边界：`SPLIT_PART` 第二个部分可能为 NULL，此时使用整个 JobKey 作为 Name |
| [R4] Platform 和 Agent 服务热更新不同步 | Agent API 与 Platform API 同时改，确保兼容窗口为 0 |
| [R5] Quartz 原生 `JobKey.ToString()` 返回 `"Group.Name"` 用于日志 | 日志中使用 `jobKey.ToString()` 保留可读性 |

## Migration Plan

1. **Step 1**: 新增 `JobKeyDto`、JsonConverter、前端接口 — 纯新增，无影响
2. **Step 2**: 更新所有 Shared DTO — 编译期破坏，确保所有引用处一起改
3. **Step 3**: DB migration — 添加 Name 列、索引，回填数据
4. **Step 4**: Platform 和 Agent 同时改 Controller/Service — 保持一致的 URL 路由
5. **Step 5**: 前端同步改 — API 调用、路由、页面组件
6. **Step 6**: 验证 — 构建 + LSP diagnostics
