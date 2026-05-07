## Context

当前 `ResolveJobType()` 采用三层 fallback：JobDataMap > Registry(Key) > CLR Name > unknown。三层逻辑叠加了不同注册路径的特殊处理。

改用 `Type.AssemblyQualifiedName` 作为数据源，解析为结构化 `JobTypeQualifiedName` 模型，各部件拆分明确定义。读取路径简化，且显示能精确区分程序集与类名。

## Goals / Non-Goals

**Goals:**
- jobType 以结构化 `JobTypeQualifiedName` 传递，包含 `fullName`、`assembly`、`version`、`culture`、`publicKeyToken`
- 移除三层 fallback 逻辑，`ResolveJobType()` 解析 `Type.AssemblyQualifiedName` 为结构化对象
- 前端展示以 `assembly` 为灰色前缀，`fullName` 最后一段为亮色主体
- 注册表匹配只基于 `fullName`（不受 version/culture 变化影响）
- 反射创建 Job 时按需拼接回 `"fullName, assembly"` 传给 `Type.GetType()`

**Non-Goals:**
- 不改变现有 JobDataMap 内容（但新写入用 qualified name）
- 不引入额外的程序集加载逻辑

## Decisions

### 1. JobTypeQualifiedName 结构化模型

新增模型类，从 `Type.AssemblyQualifiedName` 解析而来：

```
输入: "Sample.Jobs.EchoJob, Sample.Jobs, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
       ↓ 解析
{
  FullName:       "Sample.Jobs.EchoJob",     // Type.FullName
  Assembly:       "Sample.Jobs",             // Assembly.GetName().Name
  Version:        "1.0.0.0",                 // 可选
  Culture:        "neutral",                 // 可选
  PublicKeyToken: null                       // 可选
}
```

提供两个静态方法：
- `ParseFrom(Type type)` — 从 CLR Type 创建
- `ParseFrom(string assemblyQualifiedName)` — 从字符串解析
- `ToAssemblyQualifiedName()` — 拼回 `"FullName, Assembly"` 供 `Type.GetType()` 使用（不含 version/culture，避免版本脆弱性）

### 2. 读取路径：ResolveJobType() 返回结构化对象

当前: `ResolveJobType` 16 行三层 fallback → `string`
改为: `jobDetail.JobType?.AssemblyQualifiedName` → 解析为 `JobTypeQualifiedName`
理由: Quartz `IJobDetail.JobType` 始终携带 CLR 类型信息，`AssemblyQualifiedName` 包含程序集名

### 3. 创建时 Registry 查找

当前: `_registry.Get(request.JobType)` 按 Key 匹配
改为: `_registry.GetByFullName(request.JobType)` 按 `fullName` 匹配
理由: 前端发送 JSON 时按需拼接，但注册表匹配只用 `fullName`，版本变化不影响

### 4. 反射类型解析

当前: `Type.GetType(jobType.JobTypeFullName)` 用 FullName 解析
改为: `Type.GetType(qualifiedName.ToAssemblyQualifiedName())` 拼接 `"fullName, assembly"`
理由: 带上程序集名后不依赖 AppDomain 扫描，解析无歧义

### 5. 前端数据结构

TypeScript 端对应模型：

```typescript
interface JobTypeQualifiedName {
  fullName: string;           // "Sample.Jobs.EchoJob"
  assembly: string;           // "Sample.Jobs"
  version?: string;
  culture?: string;
  publicKeyToken?: string;
}
```

所有 DTO 中的 `jobType: string` 字段改为 `jobType: JobTypeQualifiedName`。

### 6. 前端 UI 显示 — JobTypeDisplay 组件布局

#### 6.1 总体布局

使用 `flex w-full` 撑满横向空间，三部分组成：

```
┌──────────────────────────────────────────────────────────┐
│  [Sample.Jobs]              Sample.Jobs.EchoJob      [📋] │
│   tag(bg-slate-700)          typename(flex-1)         btn │
│   shrink-0 + left            right-ellipsis          shrink-0
└──────────────────────────────────────────────────────────┘
   justify-between
```

#### 6.2 Assembly Tag

- `assembly` 以深色标签展示：`bg-slate-700 text-slate-300 text-xs px-2 py-1 rounded`
- `shrink-0`，不参与省略，宽度由内容撑开
- 无论容器多窄，tag 始终完整可见

#### 6.3 TypeName 显示与省略

- `flex-1 min-w-0 overflow-hidden text-ellipsis whitespace-nowrap`，标准 CSS 右省略
- 内部结构利用已知格式拆分：`namespace.` + `className`
  - `namespace`：参与 text-overflow 右省略（自动被截断）
  - `className`：始终完整显示
- 效果：当容器变窄时，命名空间逐渐被省略，类名始终完整

| 容器 | 显示 |
|---|---|
| 宽 | `Sample.Jobs.EchoJob` |
| 中 | `Sample.J...EchoJob` |
| 窄 | `...EchoJob` |

#### 6.4 Hover tooltip

显示完整 `"fullName, assembly"` 拼接串（如 `Sample.Jobs.EchoJob, Sample.Jobs`）。

#### 6.5 复制按钮

复制 `"fullName, assembly"` 拼接串，`shrink-0` 始终显示在右侧。

#### 6.6 Props

```typescript
interface JobTypeDisplayProps {
  jobType: JobTypeQualifiedName;
  maxLength?: number;       // tooltip 截断阈值（默认 60）
  showCopy?: boolean;       // 是否显示复制按钮（默认 true）
  size?: 'sm' | 'md';      // 紧凑模式，sm 用于表格行
}

### 7. 创建 Job 时发送

`CreateJobPanel` 发送时需要拼接，后端预期接收到能通过 `Type.GetType()` 解析的格式：
- 拼接为 `"fullName, assembly"`（不含 version/culture）
- 后端 `_registry.GetByFullName()` 用 `fullName` 匹配

## Risks / Trade-offs

- **[Breaking] API 响应变化**：`JobType` 字段从 `string` 变为 `JobTypeQualifiedName` 对象，所有 API 客户端需适配
- **[Breaking] CreateJobRequest.JobType**：从简单字符串变为对象，前端/外部调用方需按新格式发送
- **[Low] 旧 JobDataMap**：已有 Job 的 `JobDataMap["jobType"]` 存的是旧 key，但读取路径不再使用此值，不影响
- **[Low] 版本变化**：`JobTypeQualifiedName` 保存 version 但不参与匹配，版本升级不影响注册表查找
