## Why

当前侧边栏的 Agents 菜单采用下拉菜单形式（折叠最近 Agent → "View All Agents"），是唯一一个使用二级导航的菜单项。Dashboard、Schedulers 等均使用一级直链。这种不一致性增加了用户点击路径（需先展开下拉再选择"View All"），同时侧边栏维护了独立的 Agent 查询、localStorage 持久化、下拉展开/关闭等约 80 行额外逻辑，增加了代码复杂度。

<!-- End of Why section -->

## What Changes

### 侧边栏导航简化
- Agents 菜单从"下拉菜单（最近 Agent 列表 + View All）"改为**一级菜单直链**，点击直接跳转 `/agents` 列表页
- 移除 Agents 下拉相关的全部逻辑：`recentAgents` 状态、`addRecentAgent` 回调、`sidebar-agents` API 查询、localStorage 持久化、点击外部关闭监听
- 移除 "View All Agents" 中间步骤
- 移除 `RecentAgent` 类型定义和 `MAX_RECENT_AGENTS` 常量
- Agents 菜单与 Dashboard、Schedulers 保持一致的 `NavItem` 组件使用模式

### 侧边栏其他调整
- （可选）Calendar 导航路径修复：指向正确的 `/schedulers/:schedulerName/calendar` 而非 `/schedulers`
- （可选）Settings 菜单项处理：当前 `/settings` 路由未注册，确认保留或移除

<!-- End of What Changes section -->

## Capabilities

### Modified Capabilities
- `sidebar-navigation`: Agents 菜单从"下拉+最近 Agent"改为一级直链，移除下拉面板、最近 Agent 列表、localStorage 持久化和独立 API 查询

### New Capabilities
（无新增 capability，仅修改现有 `sidebar-navigation`）

<!-- End of Capabilities section -->

## Impact

- **src/MinGo.Qap.UI/src/components/Sidebar.tsx**: 移除约 80 行下拉相关代码，替换为单行 NavItem，文件从 ~240 行减至 ~160 行
- **删除的代码**:
  - `RecentAgent` 接口
  - `MAX_RECENT_AGENTS` 常量
  - `recentAgents` 状态 + `localStorage` 读写
  - `sidebar-agents` React Query
  - `addRecentAgent` / `handleAgentSelect` 回调
  - `dropdownRef` + click-outside 监听
  - `agentsOpen` 展开状态
  - 所有与下拉面板相关的 JSX
- **openspec/specs/sidebar-navigation/spec.md**: 需更新，移除 Agents dropdown 相关需求
- **无后端变更、无依赖变更**

<!-- End of Impact section -->
