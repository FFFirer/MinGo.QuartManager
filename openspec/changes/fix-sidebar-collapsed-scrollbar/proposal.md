## Why

侧边栏折叠到 w-16(64px) 后，导航区域的 `overflow-y-auto` 导致出现不必要滚动条。同时 NavItem 内边距+图标宽度(42px)超出容器可用宽度(40px)造成水平溢出。折叠态视觉效果受损。

## What Changes

- **Sidebar.tsx**: 折叠态下隐藏 nav 滚动条（`overflow-y-hidden`），防止滚动条轨道占用空间
- **Sidebar.tsx**: 折叠态下 `<aside>` 添加 `overflow-hidden`，裁剪溢出内容
- **Sidebar.tsx**: 折叠态下 NavItem 缩小水平 padding，防止图标水平溢出
- **App.tsx**: 主内容区域添加 `min-w-0` 确保 flex 子项正确收缩
- **sidebar-navigation spec**: 更新折叠态 overflow 行为要求

## Capabilities

### New Capabilities
*(none - this is a bugfix for existing capability)*

### Modified Capabilities
- `sidebar-navigation`: 折叠态下 nav 内容区域不显示滚动条，`<aside>` 裁剪溢出内容

## Impact

- **src/MinGo.Qap.UI/src/components/Sidebar.tsx**: 核心修改文件
- **src/MinGo.Qap.UI/src/App.tsx**: 可能的 layout 调整
- **openspec/specs/sidebar-navigation/spec.md**: spec 更新折叠态要求
