## Why

Calendar 页面因后端 API（`/api/schedulers/{name}/calendar`）未实现，实际上不可用；Settings 页面未实现且无对应路由。两者都导致侧边栏导航断裂（点击后 404/空页面），影响用户体验。

## What Changes

- 从 `Sidebar.tsx` 中移除 **Calendar** 导航项
- 从 `Sidebar.tsx` 中移除 **Settings** 导航项
- 从 `App.tsx` 中移除 Calendar 路由（`/schedulers/:schedulerName/calendar`）
- 从 `App.tsx` 中移除 Alt+C 键盘快捷键
- **删除** `CalendarPage.tsx` 文件（对应的后端 API 尚未实现，页面不可用）

## Capabilities

### New Capabilities

无新增能力。

### Modified Capabilities

- `sidebar-navigation`: 移除 Calendar 和 Settings 导航项。不再在侧边栏展示这两个入口。
- `cluster-calendar`: **已废弃**。删除整个 calendar 页面和相关代码。**BREAKING**

## Impact

- **前端**: 删除 1 个页面组件（`CalendarPage.tsx`），修改 2 个文件（`Sidebar.tsx`, `App.tsx`）
- **后端**: 无影响。`/api/schedulers/{name}/calendar` 从未实现，删除前端依赖后无任何影响
- **路由**: `/schedulers/:schedulerName/calendar` 路由被移除
