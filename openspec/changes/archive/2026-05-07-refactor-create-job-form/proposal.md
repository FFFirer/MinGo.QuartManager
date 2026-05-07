## Why

当前创建 Job 表单存在多个问题：死代码重复、多步向导被塞进不适合的滑出面板、未利用 Quartz.NET 原生 Group 能力、参数编辑体验差且无必填校验、无快速复制已有 Job 的能力。重构后统一为全屏专用页面，提升 Job 创建效率和用户体验。

## What Changes

- **NEW** 全屏创建 Job 页面 (`/schedulers/:name/jobs/create`)，替代滑出面板
- **NEW** Group 字段 + 已有 Group 下拉 + 自定义输入
- **NEW** 参数可视化编辑（根据 manifest 类型渲染对应控件）
- **NEW** 必填参数表单校验
- **NEW** 从已有 Job 复制创建 (`?copyFrom=GROUP.name`)
- **NEW** Cron 预设按钮 + 下一执行时间预览
- **NEW** JSON 编辑器（textarea 方案）处理复杂参数
- **MODIFY** JobsPage.tsx "Create Job" 按钮改为页面跳转
- **MODIFY** JobsPage.tsx 列表行增加 "Copy" 快捷操作
- **DELETE** `CreateJobModal.tsx` 死代码
- **DELETE** `CreateJobPanel.tsx` 被新页面替代

## Capabilities

### New Capabilities
- `job-create-form`: 全屏创建 Job 页面，包含 Group 选择、参数可视化编辑、调度配置、选项配置，支持从已有 Job 复制

### Modified Capabilities
- `job-templates`: 移除旧的滑出面板模板选择方式，改为全屏页面 + copyFrom 查询参数方式

## Impact

- **Frontend**: 新增 `CreateJobPage.tsx`，修改 `App.tsx`（路由）、`JobsPage.tsx`（入口按钮 + Copy 操作）、`api/index.ts`（获取 Job 详情 API）、`types/index.ts`
- **Backend**: 无改动 — Agent 端已原生支持 `group.name` 格式的 JobKey
- **Dead Code**: 删除 `CreateJobModal.tsx`，废弃 `CreateJobPanel.tsx`
