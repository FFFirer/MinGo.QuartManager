## Purpose

定义 Docker 容器镜像构建规范，确保基础镜像使用国内可访问的镜像源，CI 构建流水线能够成功执行。

## Requirements

### Requirement: Base image MUST use domestic-accessible registry

所有 Docker 基础镜像 MUST 优先使用国内可访问的镜像源（如华为云 SWR、阿里云 ACR），避免因 Docker Hub 网络不可达导致构建失败。

#### Scenario: Build succeeds with domestic mirror
- **WHEN** CI 执行 Docker 构建
- **THEN** Node.js 阶段使用华为云 SWR 镜像 `swr.cn-north-4.myhuaweicloud.com/ddn-k8s/docker.io/library/node:20-alpine` 而非 Docker Hub 上的 `node:22-alpine`
- **THEN** 构建过程不会因镜像拉取超时而失败

### Requirement: build.yml Dockerfile path MUST match actual location

`.gitea/workflows/build.yml` 中的 `file` 参数 MUST 指向实际存在的 Dockerfile 路径。

#### Scenario: Build finds Dockerfile
- **WHEN** CI 触发 build-and-push 作业
- **THEN** `docker/build-push-action` 的 `file` 参数指向 `./Dockerfile`
- **THEN** Docker 构建上下文能够正常读取该文件
