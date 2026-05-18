## Context

当前 `.gitea/workflows/build.yml` 流水线在 release 或手动触发时执行 Docker 镜像构建并推送。构建错误有两个根本原因：

1. **路径错误**: build.yml 中 `file: src/MinGo.Qap.Platform/Dockerfile` 引用了一个不存在的路径。Docker 实际位于项目根目录 `./Dockerfile`。这是由于此前项目结构调整（Dockerfile 从 Platform 项目移到了根目录）但 workflow 未同步更新所致。

2. **网络不可达**: Dockerfile 中 `FROM node:22-alpine` 指向 Docker Hub。国内 Gitea Actions CI 运行环境访问 Docker Hub 不稳定，导致 `pnpm install` 阶段前的镜像拉取就已失败。同一组织下的 MinGo.ApplicationPortal 仓库已使用华为云 SWR 镜像 `swr.cn-north-4.myhuaweicloud.com/ddn-k8s/docker.io/library/node:20-alpine` 解决此问题。

## Goals / Non-Goals

**Goals:**
- 修复 build.yml 的 Dockerfile 路径，使构建流水线能找到正确的 Dockerfile
- 将 Node.js 基础镜像替换为国内可达的华为云 SWR 镜像
- 与 MinGo.ApplicationPortal 仓库的镜像策略保持一致

**Non-Goals:**
- 不修改 .NET SDK 和 ASP.NET Runtime 镜像（`mcr.microsoft.com` 在当前环境可用）
- 不修改 Dockerfile 的三阶段构建结构
- 不改动 pnpm 版本或构建逻辑

## Decisions

| 决策 | 选项 | 选择 | 理由 |
|---|---|---|---|
| Dockerfile 路径 | `./Dockerfile` vs 复制回 `src/MinGo.Qap.Platform/` | `./Dockerfile` | 保持文件在根目录的约定，仅修复引用路径。单行变更，风险最低 |
| Node.js 镜像 | `node:22-alpine` vs 华为云 SWR 镜像 | 华为云 `swr.cn-north-4.myhuaweicloud.com/ddn-k8s/docker.io/library/node:20-alpine` | 与 ApplicationPortal 仓库保持一致的镜像源策略，该镜像已验证在国内环境可用。Node 20 为当前 LTS，稳定可靠 |
| Node 版本 | 22 vs 20 | 20 (SWR 镜像) | SWR 镜像目前提供 node:20-alpine。前端构建 (Tailwind + Vite) 对 Node 版本不敏感，20 LTS 完全满足需求 |

## Risks / Trade-offs

| 风险 | 缓解措施 |
|---|---|
| SWR 镜像版本更新滞后，可能缺少最新的 Node 22 | 前端构建仅需 Node 运行环境，20 LTS 已足够。未来 Node 20 EOL (2026-10) 前评估升级 |
| `mcr.microsoft.com` 未来也可能不可达 | 如果出现此问题，可参考同样方式替换为 SWR 或阿里云镜像。当前环境可用，暂不处理 |
