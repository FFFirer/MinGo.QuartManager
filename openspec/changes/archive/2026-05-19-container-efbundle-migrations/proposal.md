## Why

当前容器镜像构建中不包含 EF Core 迁移工具。生产环境（Docker 部署）下无法方便地执行数据库迁移——现有 spec 禁止生产环境自动迁移，但 `dotnet ef database update` 需要 SDK/Tools/源码，在容器环境中不切实际。通过 `dotnet ef migrations bundle` 生成单文件迁移可执行程序嵌入镜像，使生产环境的数据库迁移变得可靠、可重复、无需额外工具链。

## What Changes

- **Dockerfile 改造**: 在 dotnet-build 阶段增加 `dotnet ef migrations bundle` 步骤，生成自包含的迁移可执行文件 `efbundle`
- **Runtime 镜像**: 将生成的 `efbundle` 纳入 runtime 镜像（`/app/efbundle`）
- **efbundle 使用方式**: 提供通过 `docker run --rm` 或部署编排执行迁移的能力
- **Spec 更新**: `container-image` spec 增加 efbundle 构建要求；`ef-core-migrations` spec 增加生产环境 bundle 部署要求

## Capabilities

### New Capabilities
- `container-efbundle`: 定义 efbundle 如何在 Docker 多阶段构建中生成、输出到 runtime 镜像，以及如何在容器化部署中使用

### Modified Capabilities
- `container-image`: 增加 requirement——容器镜像必须在构建阶段生成 efbundle 并包含在 runtime 镜像中
- `ef-core-migrations`: 增加 requirement——生产环境必须支持通过 efbundle 执行数据库迁移（替代目前的仅警告策略）

## Impact

- `Dockerfile`: 在 dotnet-build 阶段增加 dotnet-ef tool 安装 + migrations bundle 生成命令
- `openspec/specs/container-image/spec.md`: 增加 efbundle 构建 requirement
- `openspec/specs/ef-core-migrations/spec.md`: 增加生产环境 efbundle 使用 requirement
- `README.md`: 可选——补充 efbundle 用途说明
