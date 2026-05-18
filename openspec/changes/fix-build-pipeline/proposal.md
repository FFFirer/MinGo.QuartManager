## Why

当前 build.yml 构建流水线存在两个导致构建失败的问题：
1. **Dockerfile 路径错误**：build.yml 中指定 `file: src/MinGo.Qap.Platform/Dockerfile`，但实际 Dockerfile 位于项目根目录 `./Dockerfile`，导致 Docker 构建提示文件不存在。
2. **Node.js 基础镜像不可达**：Dockerfile 使用 `node:22-alpine` 从 Docker Hub 拉取，国内 CI 环境网络受限，拉取超时/失败。参考 MinGo.ApplicationPortal 仓库的修复方式，改为华为云 SWR 镜像。

## What Changes

- 修改 `.gitea/workflows/build.yml` 第 66 行 Dockerfile 路径为 `./Dockerfile`
- 修改 `Dockerfile` 第 4 行 Node.js 基础镜像为华为云国内镜像 `swr.cn-north-4.myhuaweicloud.com/ddn-k8s/docker.io/library/node:20-alpine`

## Capabilities

### New Capabilities
- `container-image`: 规范 Docker 容器镜像的基础镜像引用策略，要求所有基础镜像优先使用国内可访问的镜像源

### Modified Capabilities
无（这是基础设施修复，不涉及功能规格变更）

## Impact

- `.gitea/workflows/build.yml` — 1 行修改
- `Dockerfile` — 1 行修改
- 无 API、依赖、或功能变更
