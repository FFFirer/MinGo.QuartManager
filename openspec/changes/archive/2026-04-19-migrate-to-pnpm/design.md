## Context

当前前端项目位于 `src/MinGo.Qap.UI/` 目录，使用 npm 作为包管理器。项目使用 Vite + React + TypeScript 技术栈。

## Goals / Non-Goals

**Goals:**
- 将前端项目的包管理器从 npm 迁移到 pnpm
- 确保现有开发脚本 (dev, build, lint, preview) 正常工作
- 减少磁盘空间占用和依赖安装时间
- 保持与现有 CI/CD 工作流的兼容性

**Non-Goals:**
- 不修改任何业务代码
- 不升级或降级任何依赖版本
- 不改变项目的构建或部署流程

## Decisions

1. **使用 pnpm 替代 npm**
   - 理由：pnpm 使用硬链接和内容寻址存储，依赖安装速度更快，磁盘占用更少
   - 替代方案：yarn (速度和磁盘占用不如 pnpm)

2. **保留现有 package.json scripts**
   - 理由：pnpm 完全兼容 npm 的 scripts 语法，无需修改
   - 替代方案：使用 pnpm 自定义命令 (pnpm add vs npm install)

3. **配置 pnpm workspace (如需)**
   - 理由：当前为单体前端项目，暂不需要 workspace
   - 如后续有前端微前端或 multi-package 需求，可添加 pnpm-workspace.yaml

## Risks / Trade-offs

- [风险] 开发者本地未安装 pnpm
  - 解决方案：在 README 中添加 pnpm 安装说明

- [风险] 某些 npm 特有配置可能不兼容
  - 解决方案：检查并迁移 .npmrc 配置到 .npmrc 或 pnpm 相关配置

- [风险] CI/CD 流水线需要更新
  - 解决方案：更新 CI 脚本中的 npm install 为 pnpm install