## Why

MinGo.QuartManager 目前缺乏自动化的容器化部署流程。现有的 CI 只做 .NET 编译和测试，CD 仅针对单镜像推送到 ghcr.io，没有适配私有 Registry 和 Debian 服务器的部署链路。需要建立一套完整的 CI/CD pipeline：代码提交/发布 → Docker 镜像构建 → 推送到私有 Registry → 自动部署到 Debian 12 服务器，支撑项目的生产级运维。

## What Changes

- **重构 Platform Dockerfile**：改为 3 阶段构建 (Node UI build → .NET publish → runtime)，UI 构建产物自动集成到 Platform 的 wwwroot 目录
- **重写 Gitea Workflow**：合并 CI + CD 为单一 `build.yml`，参考 CertManager 的 release + workflow_dispatch 双触发模式，使用 Gitea vars/secrets 管理 Registry 凭证
- **新增生产部署配置**：创建 `deploy/docker-compose.yml` (Platform + Nginx 反代)、Nginx 配置、部署脚本
- **移除旧配置**：删除旧的 `cd.yml`、`docker-build.yml`、根目录的 `docker-compose.yml`
- **新增 Agent NuGet 发布步骤**：workflow 中增加 `dotnet pack && nuget push` 步骤
- **清理旧 docker-compose 文件**：删除根目录 `docker-compose.yml`，保留 `deploy/` 下的文件作为参考

## Capabilities

### New Capabilities
- `ui-integration`: UI 构建流程集成到 Platform 项目中，Vite build 产物自动拷贝到 `wwwroot/`，Platform 直接托管前端
- `docker-deploy`: 完整的容器化构建和部署 pipeline，支持 release 自动部署和手动触发部署
- `nuget-publish`: Agent NuGet 包的自动打包和发布流程

### Modified Capabilities

<!-- No existing specs are modified since there are no pre-existing openspec specs -->

## Impact

| 影响范围 | 说明 |
|---------|------|
| `src/MinGo.Qap.Platform/Dockerfile` | 重构为 3 阶段构建，新增 Node 阶段 |
| `src/MinGo.Qap.Platform/*.csproj` | 可能需要调整 UI 构建产物的包含逻辑 |
| `.gitea/workflows/` | 删除 `cd.yml`、`docker-build.yml`，新建 `build.yml` |
| `deploy/` | 新建 `docker-compose.yml`、`nginx/nginx.conf`、`deploy.sh`、`.env.example` |
| `docker-compose.yml` (根目录) | 删除，移入 `deploy/` 统一管理 |
