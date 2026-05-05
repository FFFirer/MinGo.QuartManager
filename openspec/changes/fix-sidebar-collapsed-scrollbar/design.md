## Context

当前 sidebar 在折叠态（w-16 = 64px）时，nav 区域的 `overflow-y-auto` 在 Windows「始终显示滚动条」设置下会显示滚动条轨道，占用 ~15px 水平空间，导致本就很窄的 40px 内容区进一步被挤压。同时 NavItem 的 `px-3` padding(12px) + icon(18px) = 42px 超出 nav p-3 后的可用宽度 40px，产生 2px 水平溢出。

Tech stack: React 19 + TypeScript + Vite + Tailwind CSS

## Goals / Non-Goals

**Goals:**
- 折叠态下 sidebar 不显示任何滚动条（垂直/水平）
- 折叠态下 nav 内容无视觉溢出
- 展开态行为不改变（保持现有的 overflow-y-auto 用于长列表滚动）
- 不影响主内容区域的滚动行为

**Non-Goals:**
- 不改变 sidebar 布局结构（w-16/w-64 不变）
- 不改变 NavItem 渲染逻辑或 tooltip 行为
- 不涉及 mobile/responsive 布局调整

## Decisions

### Decision 1: 折叠态 nav 改为 `overflow-y-hidden`

- **方案**: 根据 `collapsed` 状态条件切换 `overflow-y-auto` / `overflow-y-hidden`
- **替代方案**: 使用 CSS `scrollbar-width: none` 隐藏滚动条，但兼容性不完全；使用 `overflow: hidden` 统一处理双轴
- **理由**: 最直接的方案，折叠态 nav 只有 3 个 icon item，远不溢出垂直空间。展开态保留滚动能力

### Decision 2: 折叠态 `<aside>` 添加 `overflow-hidden`

- **方案**: 根据 `collapsed` 条件在 aside 上添加 `overflow-hidden`
- **理由**: 防 NavItem 或 tooltip 溢出 aside 容器

### Decision 3: 折叠态 NavItem padding 从 `px-3` 缩小为 `px-2`

- **方案**: `px-3` → `px-2`（12px → 8px），让 icon 有足够空间
- **计算**: 8 + 18 + 8 = 34px，nav 可用 40px → 安全
- **理由**: 解决了实际水平溢出问题

### Decision 4: `<main>` 添加 `min-w-0`

- **方案**: `flex-1 overflow-auto` → `flex-1 min-w-0 overflow-auto`
- **理由**: 确保 flex 子项在容器缩小时正确收缩，防横向溢出

## Risks / Trade-offs

- **[Low] 折叠态滚动能力丢失**: 如果将来折叠态 nav 内容增多（>10 item），需要重新评估。当前只有 3 个 item，风险极低
- **[Low] padding 变化感知**: `px-3` → `px-2` 在折叠态下视觉差异极小（两端各少 4px）
- **[Low] min-w-0 副作用**: 只影响 main 的 flex 收缩行为，不影响展开态布局
