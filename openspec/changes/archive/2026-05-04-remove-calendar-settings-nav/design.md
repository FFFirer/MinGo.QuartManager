## Context

前端侧边栏（`Sidebar.tsx`）中存在两个无法正常工作的导航项：
- **Calendar**: 链接到 `/schedulers`，实际日历路由为 `/schedulers/:schedulerName/calendar`，且后端 Calendar API 未实现，页面数据无法加载
- **Settings**: 链接到 `/settings`，对应的路由和页面组件均不存在

`CalendarPage.tsx` 依赖于不存在的后端端点 `/api/schedulers/{name}/calendar`，整体页面不可用。

## Goals / Non-Goals

**Goals:**
- 从侧边栏中移除 Calendar 和 Settings 导航项，消除断裂链接
- 删除 Calendar 路由和 `CalendarPage.tsx` 组件
- 移除 Alt+C 键盘快捷键
- 保持现有路由和其他导航项不变
- 更新 `sidebar-navigation` 和 `cluster-calendar` spec 反映变更

**Non-Goals:**
- 不新增任何功能或页面
- 不修改后端代码
- 不修改 CalendarPage 以外的其他页面组件
- 不重构侧边栏布局或导航结构

## Decisions

### D1: 直接删除而非注释

**决策**: 从 Sidebar、App.tsx 和文件系统中彻底删除 Calendar 相关代码，而非注释保留。

**理由**: 
- Calendar 功能的后端 API 从未实现且有明确 TODO 标记，短期无实现计划
- 删除比注释更干净，避免死代码积累
- git 历史可追溯，需要时可还原

### D2: Settings 只移除导航项

**决策**: Settings 仅从 Sidebar 中移除 NavItem，无需额外操作。

**理由**: Settings 没有独立页面文件或路由配置，只有 Sidebar 中的一个链接入口。移除链接即解决问题。

### D3: Alt+C 快捷键移除

**决策**: 直接删除 App.tsx 中 `e.altKey && e.key === 'c'` 的条件分支。

**理由**: 该快捷键原先意图是打开日历页面，但当前指向 `/schedulers`，与功能不匹配。移除比保留误导性快捷键更合理。

## Risks / Trade-offs

| 风险 | 缓解措施 |
|------|---------|
| 用户可能习惯从侧边栏访问 Calendar | Calendar 页面从未正常工作过，且后端未实现，移除不会造成功能回退 |
| 未来需要恢复 Calendar 功能 | git 历史完整保留，可从 `CalendarPage.tsx` 和路由配置回滚 |
| Settings 页面未来需要开发 | 届时只需在 Sidebar 添加 NavItem + 创建 SettingsPage + 添加路由即可 |
