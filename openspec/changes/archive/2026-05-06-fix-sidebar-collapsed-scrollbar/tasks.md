## 1. Sidebar.tsx — 折叠态 overflow 修复

- [ ] 1.1 折叠态 nav 的 `overflow-y-auto` → `overflow-y-hidden`（条件切换）
- [ ] 1.2 折叠态 `<aside>` 添加 `overflow-hidden`（条件切换）
- [ ] 1.3 折叠态 NavItem 的 `px-3` → `px-2`（缩小水平 padding）

## 2. App.tsx — 主内容区 flex 收缩修复

- [ ] 2.1 `<main>` 添加 `min-w-0` class

## 3. 验证

- [ ] 3.1 LSP diagnostics 无错误
- [ ] 3.2 前端 build 通过（`pnpm build`）
