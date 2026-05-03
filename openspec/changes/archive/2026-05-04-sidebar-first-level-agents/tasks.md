## 1. Sidebar Agents 菜单重构

- [x] 1.1 将 Agents 下拉区块替换为 NavItem 直链 — `Sidebar.tsx` 中 `li ref={dropdownRef}` 替换为 `<NavItem to="/agents" icon={<Server size={18} />} label="Agents" ... isActive={checkActiveStart} />`
- [x] 1.2 移除 dropdown 相关 import — 移除 `ChevronDown`, `ChevronRight` 的 import（保留需使用的其他 lucide-react 图标）
- [x] 1.3 移除状态、副作用和回调 — 删除 `recentAgents`, `agentsOpen`, `dropdownRef`, `addRecentAgent`, `handleAgentSelect`，以及相关的 `useEffect`（click-outside 监听、路径自动添加、localStorage 持久化）
- [x] 1.4 移除类型和常量 — 删除 `RecentAgent` 接口和 `MAX_RECENT_AGENTS` 常量
- [x] 1.5 移除独立的 React Query — 删除 `sidebar-agents` query（`allAgents`）
- [x] 1.6 验证 LSP diagnostics 无错误 — 确保改后 `Sidebar.tsx` 无类型/编译错误

## 2. Spec 同步

- [x] 2.1 确认 delta spec 已正确标记 MODIFIED/REMOVED 需求
