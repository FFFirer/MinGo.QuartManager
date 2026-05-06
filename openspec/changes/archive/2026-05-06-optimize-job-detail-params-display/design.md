## Context

Job 详情页（`JobDetailPage.tsx`）目前的参数展示区域是纯 key:value 的 `<div>` 列表：

```
< div className="grid grid-cols-1 md:grid-cols-2 gap-2">
  {Object.entries(job.params).map(([key, value]) => (
    <div key={key} className="text-sm">
      <span>{key}: </span>
      <span>{String(value)}</span>
    </div>
  ))}
</div>
```

现有可用的数据源：
- **`job.params: Record<string, any>`** — 从 `GET /api/schedulers/{name}/jobs/{key}` 获取的运行时参数
- **`manifestApi.get(schedulerName)` → `JobManifestDto.jobs[].parameters[]`** — 参数定义元数据（name, type, label, required, default）
- **后端 Bug**: `JobService.GetAsync()` 对 `params` 做了二次序列化 (`JsonSerializer.Serialize(job.Params)`)，导致前端 `JobDefinitionDto.params` 是字符串而非对象。API 类型定义为 `JobDefinitionDto`，但前端实际消费为 `JobDetailDto` 类型

约束条件：
- 使用 Tailwind CSS（已配置）
- 使用 lucide-react 图标库（已引入）
- UI 风格保持现有暗色主题（slate 色系）
- 不做后端改动

## Goals / Non-Goals

**Goals:**
- 参数展示按类型区分渲染（bool/int/string/datetime/object/array）
- 接入 manifest 元数据，显示 label、type、required、default
- 修复 params 反序列化问题
- 提取独立组件 `JobParamsDisplay` 便于复用
- 支持参数搜索/过滤
- 支持参数值一键复制

**Non-Goals:**
- 不做参数在线编辑（后续独立 feature）
- 不改后端 API 和 DTO
- 不改其他页面（SchedulerDetailPage 等）
- 不做国际化（label 已由 manifest 提供，保持原始格式）

## Decisions

### D1. 提取独立组件 JobParamsDisplay

**决策**: 将参数渲染逻辑从 `JobDetailPage.tsx` 提取为 `components/JobParamsDisplay.tsx`

**理由**: 
- JobDetailPage 已有 224 行，参数逻辑占 30+ 行且将增长
- 组件可被未来可能的 EditJobPanel 或对比视图复用
- 职责分离，便于测试

**替代方案**: 内联在 JobDetailPage 中 — 被否，复杂度已足够独立

### D2. 类型感知渲染策略

对 `params` 中的每个值，按以下规则渲染：

| 检测逻辑 | 渲染方式 | 说明 |
|---------|---------|------|
| `typeof v === 'boolean'` | `<ParamBadge>` | ✓ 绿色 / ✗ 灰色 开关样式图标 |
| `typeof v === 'number'` | `<ParamNumber>` | 数字右对齐，字体 `tabular-nums` |
| `isValidDate(v)` | `<ParamDate>` | `toLocaleString()` 格式化 + 时间戳 tooltip |
| `typeof v === 'object' && v !== null` | `<ParamJson>` | 可折叠 JSON 树，默认折叠 |
| `typeof v === 'string' && v.length > 80` | `<ParamLongText>` | 截断 + 展开按钮 |
| 其他 | `<ParamText>` | 纯文本 + 复制按钮 |

### D3. 元数据匹配策略

**决策**: 用 `job.jobType` 匹配 manifest 中的 `JobTypeInfoDto`，获取对应的 `ParameterInfoDto[]`。以参数名 `name` 为 key 建立 Map，用于：
- 显示 `label` 替代原始 key（存在时）
- 标注 `required`（红色 * 标记）
- 标注 `default`（灰色角标）
- 标注 `type`（"int: " 前缀等）

**Fallback**: 未匹配 manifest 的参数，显示原始 key 并自动推断类型

### D4. 数据修复策略

**决策**: 在 `jobApi.get` 的 response handler 或组件内部，检测 `params` 是否为字符串并自动 `JSON.parse`

**理由**: 
- 后端短期内不改（需要协调 Agent 和 Platform 两端发布）
- 前端防御性处理成本极低
- `JSON.parse` 失败时回退到空对象 `{}`

**替代方案**: 改后端 `JobService.GetAsync` 去掉二次序列化 — 被否，范围超限

### D5. 搜索过滤机制

**决策**: 组件内部 state `searchQuery: string`，实时过滤参数列表
- 搜索范围：参数名、label、值的字符串表示
- 区分大小写：不区分
- 空搜索时展示全部
- 无匹配时显示 "No matching parameters"

## Component API

```typescript
interface JobParamsDisplayProps {
  params: Record<string, any>;
  paramDefinitions?: ParameterInfoDto[];  // optional, from manifest
  searchable?: boolean;   // default true
  collapsible?: boolean;  // default true
  maxInitialHeight?: number; // default none (for collapsible section)
}
```

## Component Tree

```
JobParamsDisplay
├── SearchBar (conditional: searchable=true)
├── ParamItem × N
│   ├── ParamLabel (key / label, required marker, type badge)
│   └── ParamValue (type-specific renderer)
│       ├── ParamBool
│       ├── ParamNumber
│       ├── ParamDate
│       ├── ParamJson
│       ├── ParamLongText
│       └── ParamText
└── EmptyState (when no match / no params)
```

## Risks / Trade-offs

| Risk | Mitigation |
|------|-----------|
| Manifest 数据未加载时参数 label 缺失 | 组件接收 optional 的 `paramDefinitions`，缺失时退化为原始 key |
| JSON.parse 失败导致页面白屏 | try-catch 包裹，回退到 `{}`，console.error 警告 |
| 参数值为复杂嵌套对象时性能 | JSON 树默认折叠，只展开用户点击的层级 |
| 类型推断错误（如数字字符串误判为 number） | 优先使用 manifest 中的 `type` 字段，仅在无 manifest 时启发式推断 |
