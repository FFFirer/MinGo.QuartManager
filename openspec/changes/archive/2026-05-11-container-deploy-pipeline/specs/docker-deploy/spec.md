## ADDED Requirements

### Requirement: Gitea Workflow 构建 Docker 镜像并推送到私有 Registry
Gitea Workflow SHALL 在 release published 或 workflow_dispatch 触发时，构建 Platform Docker 镜像并推送到私有 Docker Registry。Registry 地址和凭证通过 Gitea vars/secrets 配置。

#### Scenario: Release 触发完整构建和推送
- **WHEN** 在 Gitea 中创建一个新的 Release
- **THEN** Workflow SHALL 被 `release: [published]` 事件触发
- **THEN** 版本号 SHALL 从 git tag 提取（例如 `v1.2.3`）
- **THEN** Docker 镜像 SHALL 构建并以 `$DOCKER_REGISTRY/1mingo/quartmanager:$VERSION` 和 `$DOCKER_REGISTRY/1mingo/quartmanager:latest` 两个 tag 推送
- **THEN** Workflow SHALL 执行部署步骤

#### Scenario: 手动触发构建和推送但不自动部署
- **WHEN** 手动执行 `workflow_dispatch` 触发 Pipeline
- **THEN** 版本号 SHALL 生成 `dev-{shortSha}-{YYYYMMDDHHMMSS}` 格式
- **THEN** Docker 镜像 SHALL 以 `$DOCKER_REGISTRY/1mingo/quartmanager:$VERSION` 推送
- **THEN** Workflow SHALL 不执行自动部署步骤
- **THEN** Workflow SHALL 输出镜像名称和 tag 供后续手动部署使用

#### Scenario: Registry 凭证通过 Gitea 变量管理
- **WHEN** Workflow 执行 `docker/login-action` 登录 Registry
- **THEN** Registry URL SHALL 使用 `${{ vars.DOCKER_REGISTRY }}`
- **THEN** 用户名 SHALL 使用 `${{ vars.DOCKER_USERNAME }}`
- **THEN** 密码 SHALL 使用 `${{ secrets.DOCKER_PASSWORD }}`

### Requirement: 自动部署到 Debian 12 服务器
Release 触发的 Workflow SHALL 自动通过 SSH 连接到 Debian 12 部署服务器，执行 `docker compose pull && docker compose up -d`。

#### Scenario: Release 触发自动部署
- **WHEN** Release 触发的 Pipeline 完成镜像推送
- **THEN** Workflow SHALL 通过 SSH 连接到 `${{ secrets.DEPLOY_HOST }}`
- **THEN** 在服务器上 SHALL 执行 `cd /opt/quartmanager && docker compose pull && docker compose up -d`
- **THEN** 旧镜像 SHALL 被清理（`docker image prune -f`）

#### Scenario: SSH 连接失败
- **WHEN** SSH 连接到部署服务器失败（网络问题或密钥无效）
- **THEN** 部署 job SHALL 失败并输出明确的错误信息
- **THEN** 已推送的镜像 SHALL 不受影响，可手动部署

### Requirement: 生产 docker-compose 配置
`deploy/docker-compose.yml` SHALL 定义 Platform 服务和 Nginx 反向代理服务，通过 Docker bridge 网络通信。

#### Scenario: 生产 compose 启动完整服务栈
- **WHEN** 执行 `docker compose up -d` (在 deploy/ 目录下)
- **THEN** Platform 容器 SHALL 启动并监听内部 80 端口
- **THEN** Nginx 容器 SHALL 启动并映射宿主 443 和 80 端口
- **THEN** Platform 和 Nginx SHALL 连接到 `qap-network` bridge 网络
- **THEN** Platform 容器 SHALL 配置 `restart: unless-stopped`

### Requirement: Nginx 反向代理配置
Nginx SHALL 作为反向代理承载外部 HTTPS 流量，转发到 Platform 容器内部 HTTP 端口。

#### Scenario: Nginx 代理 API 请求
- **WHEN** 客户端请求 `https://host/api/agents`
- **THEN** Nginx SHALL 将请求转发到 `http://platform:80/api/agents`
- **THEN** 请求头 SHALL 包含正确的 `X-Forwarded-For`, `X-Forwarded-Proto`, `Host` 头

#### Scenario: Nginx 代理 SPA 路由
- **WHEN** 客户端请求 `https://host/schedulers`
- **THEN** Nginx SHALL 将请求转发到 `http://platform:80/schedulers`
- **THEN** Platform SHALL 返回 `wwwroot/index.html`（SPA 回退路由处理）
