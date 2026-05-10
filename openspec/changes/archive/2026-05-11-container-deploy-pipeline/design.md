## Context

MinGo.QuartManager 平台当前已在生产环境运行。代码托管在私有 Gitea 实例，部署目标为 Debian 12 服务器。

**当前状态：**
- Platform (.NET 10 Web API) 有单体 Dockerfile，但未包含前端 UI
- UI (React 19 + Vite) 仅用于本地开发，无容器化配置
- Agent 作为类库发布，无独立部署需求
- CI 使用 Gitea Actions 兼容的 GitHub Actions 语法 (`ci.yml`)
- CD 推送到 ghcr.io（需切换为私有 Registry）
- 数据库生产环境已独立部署，无需在容器中管理

**约束条件：**
- Gitea Actions 运行环境为 `ubuntu-latest`
- 私有 Docker Registry 信息通过 Gitea vars/secrets 注入
- 部署服务器 SSH 凭证通过 secrets 管理
- NuGet 服务器地址和 API Key 通过 secrets 注入

## Goals / Non-Goals

**Goals:**
- 建立一套完整的 CI/CD pipeline：代码提交 → 镜像构建 → 推送到私有 Registry → 自动部署
- 重构 Platform Dockerfile，实现 UI 构建产物自动集成到 wwwroot
- Agent NuGet 包随 pipeline 自动发布
- Nginx 反向代理承载 SSL 和路由
- 生产 docker-compose 统一管理部署服务

**Non-Goals:**
- 不管理数据库部署（由外部提供 PostgreSQL 实例）
- 不管理 Agent 宿主程序的容器化（Agent 是 NuGet 包）
- 不处理 UI 开发服务器（仅生产构建）
- 不涉及 K8s 或容器编排平台（单机 Docker Compose 已足够）

## Decisions

### D1: Dockerfile 三阶段构建策略

**选择**：Node 构建 → .NET SDK 构建 → ASP.NET Runtime

```
Stage 1 (node:22-alpine)
  └─ pnpm install && pnpm build → dist/

Stage 2 (mcr.microsoft.com/dotnet/sdk:10.0)
  ├─ COPY --from=stage1 dist/ → wwwroot/
  ├─ dotnet restore
  ├─ dotnet publish
  └─ output → /app/publish/

Stage 3 (mcr.microsoft.com/dotnet/aspnet:10.0)
  └─ COPY --from=stage2 /app/publish → /app/
  └─ ENTRYPOINT ["dotnet", "MinGo.Qap.Platform.dll"]
```

**理由**：多阶段构建确保最终镜像只包含 runtime 和编译产物，不留构建工具链。UI 在 Node 阶段编译为静态文件后由 .NET 阶段收入 wwwroot，Kestrel 直接托管。

### D2: Nginx 作为反向代理而非直接暴露 Kestrel

**选择**：Nginx 容器在前端终结 SSL，代理请求到 Platform 容器

```
Client → Nginx (:443/SSL) → Platform (:80/HTTP)
```

**理由**：Nginx 处理 SSL 证书、静态资源缓存、请求头清洗比 Kestrel 更成熟。如需水平扩展，Nginx 也可做负载均衡。

### D3: 单一 Workflow 合并 CI + CD

**选择**：参考 CertManager 的 `build.yml` 模式，创建单一 workflow 文件，通过触发方式区分执行阶段

**理由**：
- 简化管理：一个文件定义完整 pipeline
- release 触发时执行完整流程（build → push → deploy）
- workflow_dispatch 触发仅执行到 push，手动接管部署（可指定 staging 或 production）

### D4: Gitea vars/secrets 管理凭证

**选择**：所有敏感信息和环境特定配置通过 Gitea 的 Actions secrets 和 variables 管理

| 配置项 | Gitea 配置方式 |
|--------|---------------|
| Registry 地址 | `${{ vars.DOCKER_REGISTRY }}` |
| Registry 用户名 | `${{ vars.DOCKER_USERNAME }}` |
| Registry 密码 | `${{ secrets.DOCKER_PASSWORD }}` |
| 部署服务器地址 | `${{ secrets.DEPLOY_HOST }}` |
| SSH 私钥 | `${{ secrets.DEPLOY_KEY }}` |
| NuGet API Key | `${{ secrets.NUGET_API_KEY }}` |

**理由**：与 CertManager 一致的模式，无硬编码，环境间切换只需修改变量值。

### D5: 版本标签策略

**选择**：参考 CertManager，release 使用 git tag 版本，手动触发使用 dev-{sha} 格式

```
Release:   $VERSION (e.g. v1.2.3) + latest
Manual:    dev-{shortSha}-{YYYYMMDDHHMMSS}
```

### D6: UI 产物集成方案

**选择**：修改 Platform 的 `.csproj`，在 `BeforeBuild` 或 `Target` 中调用 UI 构建脚本

**方案对比**：

| 方案 | 优点 | 缺点 |
|------|------|------|
| csproj 中调用 pnpm build | CI 无需额外步骤，Dockerfile 统一管理 | 需要 Node 在 SDK 镜像中 |
| Dockerfile 独立 Stage 构建 UI | 职责清晰，构建环境隔离 | 增加 Dockerfile 复杂度 |
| Pre-build 脚本 | 灵活 | 额外维护一个脚本 |

**选择**：Dockerfile 独立 Stage（方案 2），因为构建环境隔离是最佳实践，且多阶段构建是 Docker 推荐模式。

## Risks / Trade-offs

| 风险 | 影响 | 缓解措施 |
|------|------|---------|
| Gitea Actions runner 无法访问私有 Registry | Pipeline 中断 | 在 Gitea 层面配置 HTTP 代理或 registry mirror |
| UI 构建失败导致整个 Pipeline 失败 | 无法部署 | workflow 中可考虑将 UI 构建作为独立 job，允许跳过 |
| SSH 部署密钥泄露 | 服务器被入侵 | 使用受限的 deploy key，仅允许特定目录操作和 docker compose 命令 |
| Nginx 配置与 Platform 路由冲突 | API 404 | 确保 nginx location 规则与 Platform 路由匹配，部署前验证 |
| Docker image 体积过大 | 部署传输慢 | 多阶段构建确保最终镜像仅 200-300MB，利用 registry 缓存 layer |
| 回滚复杂性 | 线上故障恢复慢 | 保留历史镜像 tag，部署脚本支持 `docker compose up -d` 指定版本回滚 |
