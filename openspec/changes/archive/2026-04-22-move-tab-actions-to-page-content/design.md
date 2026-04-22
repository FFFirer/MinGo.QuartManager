# Design: Move Tab Actions to Page Content

## Context

当前 ClusterTabs 组件接收 `actions` prop 并将其渲染在 Tab 行右侧。各个页面（JobsPage, AgentInstancesPage, ClusterDashboardPage）通过传递 actions 来显示操作按钮。

期望将 actions 管理权交给各页面，让 ClusterTabs 专注于 Tab 导航。

## Goals / Non-Goals

**Goals:**
- 移除 ClusterTabs 的 actions prop
- 各页面在内容区域顶部右侧管理自己的操作按钮
- 保持按钮位置在视觉上靠右顶部

**Non-Goals:**
- 不改变现有按钮的功能逻辑
- 不修改其他 UI 样式

## Decisions

### D1: ClusterTabs actions prop 移除方式

**选项 A**: 完全移除 actions prop 和渲染逻辑
**选项 B**: 保留 prop 但不使用（向后兼容）

**选择 A**: 完全移除，保持组件职责清晰

### D2: 按钮容器布局

各页面使用 `flex justify-end` 将按钮靠右：
```tsx
<div className="flex justify-end mb-4">
  <button>...</button>
</div>
```

### D3: 文件修改顺序

1. ClusterTabs.tsx - 移除 actions 相关代码
2. 各页面 - 添加自己的按钮容器

## Implementation

| 文件 | 改动 |
|------|------|
| `ClusterTabs.tsx` | 移除 `actions` prop 和渲染 |
| `JobsPage.tsx` | 内容区添加 Create Job 按钮 |
| `AgentInstancesPage.tsx` | 内容区添加 Register Agent 按钮 |
| `ClusterDashboardPage.tsx` | 内容区添加 View Agents + Create Job 按钮 |
| `CalendarPage.tsx` | 无（无 actions）|

## Risks / Trade-offs

- 按钮位置微调：视觉上从 Tab 行移至内容区，但都在顶部右侧，用户感知变化小
- 无后端变化，纯前端改动