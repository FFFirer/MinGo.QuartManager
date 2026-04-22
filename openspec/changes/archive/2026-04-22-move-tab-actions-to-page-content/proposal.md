# Proposal: Move Tab Actions to Page Content

## Why

ClusterTabs 组件的 actions prop 将所有页面的操作按钮集中在 Tab 行右侧，这导致：
1. 各页面无法自定义自己的操作位置和样式
2. ClusterTabs 组件承担了不必要的职责
3. 操作按钮实际属于各页面内容的一部分，应该由页面自己管理

## What Changes

- **ClusterTabs 组件**：移除 `actions` prop 的渲染，仅保留 Tab 导航功能
- **JobsPage**：将 "Create Job" 按钮从 ClusterTabs actions 移至页面内容顶部右侧
- **AgentInstancesPage**：将 "Register Agent" 按钮从 ClusterTabs actions 移至页面内容顶部右侧
- **ClusterDashboardPage**：将 "View Agents" + "Create Job" 按钮从 ClusterTabs actions 移至页面内容顶部右侧

## Capabilities

### Modified Capabilities
- `cluster-tabs-navigation`: 行为不变，UI 上移除 actions 区域

### New Capabilities
- 无新功能

## Impact

- 代码：`ClusterTabs.tsx`（移除 actions prop）、4个页面文件
- 用户体验：操作按钮仍位于顶部右侧，位置微调