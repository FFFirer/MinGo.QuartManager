## Why

当前前端项目使用 npm 作为包管理工具，存在以下问题：
1. 依赖安装速度较慢
2. node_modules 目录体积过大，占用大量磁盘空间
3. 缺乏更好的依赖版本锁定机制

pnpm 作为现代包管理器，通过硬链接和符号链接共享依赖，性能更优且磁盘占用更少。

## What Changes

- 将前端项目的包管理器从 npm 迁移到 pnpm
- 更新项目文档中的包管理命令说明
- 添加 .npmrc 或 pnpm 相关配置文件
- 保留与现有工作流兼容的脚本配置

## Capabilities

### New Capabilities

- **pnpm-migration**: 将项目包管理从 npm 迁移到 pnpm，包括配置调整和依赖安装验证

### Modified Capabilities

- 无

## Impact

- 影响的代码：`src/MinGo.Qap.UI/` 目录下的前端项目
- 配置文件：package.json, 可能需要添加 pnpm-workspace.yaml
- 开发流程：开发者需要安装 pnpm 并使用 pnpm install 替代 npm install