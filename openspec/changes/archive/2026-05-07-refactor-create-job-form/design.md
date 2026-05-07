## Context

当前创建 Job 使用 `CreateJobPanel` 组件（SlidePanel 容器 + 4步向导），代码库中还有一份未使用的 `CreateJobModal` 副本。Agent 端已原生支持通过 `group.name` 格式解析 JobKey 来提取 Group，但前端表单未暴露 Group 字段，导致所有通过 UI 创建的 Job 都落入 "default" Group。

已有 `jobApi.get()` 接口可获取 Job 详情，可作为复制功能的底层支撑。

## Goals / Non-Goals

**Goals:**
- 全屏专用创建页面，独立路由 `/schedulers/:name/jobs/create`
- Group 字段支持（下拉选择已有 Group + 自定义输入）
- 动态参数表单：根据 manifest 类型渲染对应控件 + 必填校验
- Cron 预设 + 下一执行时间前端预览
- 复制已有 Job 功能（`?copyFrom=GROUP.name` 预填表单）
- JSON textarea 处理复杂参数
- Job Type 选择列表：单列垂直布局，四行展示（短名称 / 全称 / 描述 / 参数），固定高度滚动
- 删除死代码 `CreateJobModal.tsx`，废弃 `CreateJobPanel.tsx`

**Non-Goals:**
- 不修改后端 API 或 DTO（Agent 端已原生支持 `group.name`）
- 不做完整的 Cron 解析器（仅做基本格式校验 + 调用 Agent 验证）
- 不引入 CodeMirror 等外部编辑器（先用 textarea + JSON 校验）
- 不修改 Job 详情页（仅在创建流程中改进）

## Decisions

### 1. 全屏页面 vs 滑出面板
**决定**: 采用全屏页面，独立路由 `/schedulers/:name/jobs/create`
**理由**: 
- 滑出面板宽度有限（max-w-2xl），多字段表单体验差
- 全屏页面可展示所有字段，无需"上一步/下一步"切换
- 独立路由支持直接通过 URL 携带 `?copyFrom=` 参数
- 创建完成后 `navigate(-1)` 返回列表，符合用户操作预期

### 2. 扁平表单 vs 多步向导
**决定**: 扁平表单（所有字段在同一页面，滚动查看）
**理由**: 
- 减少交互层级，"所见即所得"
- 用户可全局预览所有配置项
- 适合快速创建场景（只填必填字段即可提交）

### 3. Group 字段实现
**决定**: 组合输入框（下拉选择已有 Group + 自定义输入），提交时组装 `{group}.{name}` 格式的 jobKey
**理由**: Agent 端已原生支持 `group.name` 格式，无需后端改动
- 从当前 Scheduler 的 Job 列表中提取去重 Group 列表
- 下拉提供 "DEFAULT" + 已有 Group
- 选择 "Create New" 时展开文本输入框
- 提交时将 `group` 和 `name` 组合为 `{group}.{name}` 写入 `jobKey`

### 4. 必填参数校验方案
**决定**: 前端根据 manifest 的 `ParameterInfoDto.required` 字段校验
    - 渲染时必填参数标红 `*`
    - "Create Job" 按钮点击时遍历所有 `required=true` 的参数
    - 空值/未填时显示内联错误提示 `"此字段为必填项"`
    - 聚焦到第一个错误字段

### 5. 复制功能
**决定**: 通过 URL query parameter `?copyFrom={GROUP.name}` 实现
- `CreateJobPage` 检测 `searchParams.copyFrom`
- 调用 `jobApi.get(schedulerName, copyFrom)` 获取 `JobDefinitionDto`
- `parseJobKey(copyFrom)` 拆分为 group + name
- `parseJSON(params)` 填充参数表单
- `parseJSON(schedule)` 填充调度配置
- `parseJSON(options)` 填充选项
- 用户修改后提交，创建新的 Job

### 6. JSON 编辑器
**决定**: 使用 textarea + JSON.parse 实时校验
- 参数类型为复杂对象（非 string/int/bool）时渲染 textarea
- 输入时实时尝试 `JSON.parse`，失败时显示红色边框 + 错误提示
- 提交前再次校验

### 7. Cron 预览
**决定**: 提供常用预设按钮 + 本地基本格式校验
- 预设按钮："每日午夜" `0 0 * * *`、"每6小时" `0 */6 * * *`、"每周一" `0 0 * * 1`
- 格式校验：检查 5-7 段用空格分隔
- 下一执行时间：读取时暂不做计算（依赖 quartz 库），仅展示格式验证结果

### 8. Job Type 选择列表布局
**决定**: 单列垂直列表 + 固定高度滚动容器 + 四行信息密度

每项采用四行布局：
```
┌──────────────────────────────────────────────────┐
│ SampleJob                          ← 短名称(粗体) │
│ [Assembly] Namespace.ClassName     ← JobTypeDisplay│
│ Description text..                 ← description   │
│ 3 parameters (1 required)          ← 参数信息      │
└──────────────────────────────────────────────────┘
```

**各行规则**:
- **Line 1 - 短名称**: 取 `fullName` 最后一段（`.` 后的部分），粗体展示，设置 `truncate` + `title` 完整值
- **Line 2 - 全称**: 复用 `JobTypeDisplay` 组件（`size="sm"`），显示 Assembly tag + namespace + className
- **Line 3 - 描述**: `job.description`，无描述时整行隐藏（`display: none`），设置 `truncate` + `title`
- **Line 4 - 参数信息**: 固定格式 `X parameters (Y required)`，无必填参数时只显示 `X parameters`

**滚动容器**:
- 外层 `<div>` 固定高度，CSS 类 `max-h-[340px] overflow-y-auto`（约容纳 3-4 项）
- 内部 `space-y-1` 排列各项
- 自定义滚动条样式匹配暗色主题（`scrollbar-thin`）

**交互**:
- 点击整行选中，蓝色边框 (`border-blue-500`) + 蓝色背景 (`bg-blue-500/10`)
- 选中项右侧显示 Check 图标
- 各项之间有 1px `border-b border-slate-700` 分割线

**理由**:
- 单列布局更适合信息密度高的场景，每项可展示更多信息
- 短名称（Line 1）是用户最关心的标识，应最突出
- 全称（Line 2）提供完整类型信息，便于区分同名不同程序集的 Job
- 固定高度避免表单过长，确保页面其他部分（参数/调度/选项）可见
- 溢出滚动保证 Job Type 较多时仍旧可用

## Risks / Trade-offs

- **Cron 无真实预览**：前端不做完整 Cron 解析，仅格式校验。真实下一执行时间依赖 Agent 返回。可通过后续迭代增加 cron-parser 前端库
- **JSON textarea 体验**：复杂的嵌套 JSON 在 textarea 中编辑体验不如 CodeMirror。标记为后续优化项
- **Group 组合键冲突**：如果 jobKey 本身包含 `.` 字符，Agent 解析可能出错。约束：Job Name 不允许包含 `.` 字符
