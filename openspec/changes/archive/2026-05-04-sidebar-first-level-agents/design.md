## Context

当前 `Sidebar.tsx` 中 Agents 菜单是唯一使用二级导航（下拉菜单）的项。它维护了：
- `recentAgents` 状态 + localStorage 持久化（`sidebar-recent-agents`）
- 独立的 React Query `sidebar-agents` 调用
- `dropdownRef` + click-outside 事件监听
- `agentsOpen` 展开/关闭状态
- `addRecentAgent` / `handleAgentSelect` 回调
- `RecentAgent` 接口 + `MAX_RECENT_AGENTS` 常量

这些逻辑约 80 行，占 Sidebar 组件 ~1/3 的代码量。而 Dashboard、Schedulers 等菜单均使用 `NavItem` 组件直链。

参考已归档的 `2026-05-04-ui-ux-redesign-phase1` 中的 tasks.md：`1.1` 要求"提取 Sidebar 为独立组件，包含所有现有逻辑（最近 Agent 下拉、快捷键、active 高亮）"，当时选择保留下拉是阶段性决策。本次将下拉改直链是符合整体"简化导航"方向的后续演进。

<!-- End of Context section -->

## Goals / Non-Goals

**Goals:**
- Agents 菜单从下拉改为一级直链，点击直接跳转 `/agents` 列表页
- 移除全部下拉相关代码（~80 行）：状态、API 查询、持久化、事件监听
- Agents 与 Dashboard、Schedulers 等使用一致的 `NavItem` 模式
- 更新 `sidebar-navigation` spec 移除已废弃的需求

**Non-Goals:**
- 不修改其他菜单项（Dashboard、Schedulers、Calendar、Settings）
- 不修改 Agents 列表页（`AgentsPage.tsx`）或详情页（`AgentDetailPage.tsx`）
- 不新增功能或组件
- 不涉及后端变更
- 暂不修复 Calendar 导航路径或 Settings 路由缺失（属于独立问题）

<!-- End of Goals section -->

## Decisions

### 1. 用 NavItem 替换整个 Agents 下拉区块

**决策**: 直接将 Agens 区域的 `li ref={dropdownRef}` > `button` + dropdown 替换为 `<NavItem to="/agents" icon={<Server size={18} />} label="Agents" collapsed={collapsed} isActive={checkActiveStart} />`

**理由**:
- NavItem 已经是其他菜单的统一模式，零额外代码
- `checkActiveStart('/agents')` 能正确处理 `/agents` 和 `/agents/:agentId` 都高亮
- 单行替换 ~80 行，改动集中、风险低

**替换对照**:
```
// 当前（~80行）
<li ref={dropdownRef} className="relative">
  <button onClick={...} className={...}>
    <Server size={18} />
    <span>Agents</span>
    {agentsOpen ? <ChevronDown /> : <ChevronRight />}
  </button>
  {!collapsed && agentsOpen && (
    <div className="mt-1 w-56 bg-slate-800...">
      // recent agents list + View All
    </div>
  )}
</li>

// 改后（1行）
<NavItem to="/agents" icon={<Server size={18} />} label="Agents" collapsed={collapsed} isActive={checkActiveStart} />
```

### 2. 移除全部相关状态和副作用

**决策**: 从 Sidebar 组件中移除以下内容（无替代）：
- `import { ChevronDown, ChevronRight, Keyboard } from 'lucide-react'` → 可移除 ChevronDown、ChevronRight
- `recentAgents` state + localStorage 读写
- `allAgents` React Query (`sidebar-agents`)
- `addRecentAgent` useCallback
- `handleAgentSelect`
- `useEffect` 自动添加到 recentAgents
- `useEffect` 关闭 dropdown（路径 `/agents` 时）
- `useEffect` 持久化 recentAgents 到 localStorage
- `useEffect` click-outside 监听
- `dropdownRef`
- `agentsOpen` state
- `RecentAgent` interface
- `MAX_RECENT_AGENTS` constant

**理由**: 所有这些代码只为 dropdown 功能服务。改直链后全部不再需要。

### 3. 保留 collapse toggle 和 brand 区域不变

**决策**: Sidebar 底部（collapse toggle + version）和顶部（brand logo + keyboard hints）不做任何改动。

**理由**: 本次只涉及菜单区域的简化，不影响布局骨架。

<!-- End of Decisions section -->

## Risks / Trade-offs

| 风险 | 缓解措施 |
|------|---------|
| 用户失去"最近 Agent 快速跳转"功能 | Agents 列表页本身提供了完整的 Agent 列表和搜索，多一次点击换取整体导航一致性；如果后续需要快速跳转，可在 Agent 列表页或 Dashboard 上提供 |
| Spec 与实现不同步 | 同步创建 delta spec 标记移除的需求，后续 archive 时会合并到主 spec |

<!-- End of Risks section -->
