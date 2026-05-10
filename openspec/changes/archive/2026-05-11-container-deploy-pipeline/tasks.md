## 1. Dockerfile 重构（三阶段构建 + UI 集成）

- [x] 1.1 创建 Dockerfile 第一阶段（Node 构建）：使用 `node:22-alpine` 镜像，安装 pnpm，执行 `pnpm install && pnpm build`，输出 `dist/`
- [x] 1.2 创建 Dockerfile 第二阶段（.NET 构建）：使用 `mcr.microsoft.com/dotnet/sdk:10.0`，从第一阶段复制 `dist/` 到 `wwwroot/`，执行 `dotnet restore && dotnet publish`
- [x] 1.3 创建 Dockerfile 第三阶段（Runtime）：使用 `mcr.microsoft.com/dotnet/aspnet:10.0`，从第二阶段复制 publish 输出，设置 `ENTRYPOINT`
- [ ] 1.4 验证 Dockerfile 构建成功：`docker build -f src/MinGo.Qap.Platform/Dockerfile -t test-platform .`

## 2. Platform 后端配置调整

- [x] 2.1 在 `Program.cs` 中添加 `UseStaticFiles()` 中间件（如果尚不存在），确保 `wwwroot/` 目录被正确托管
- [x] 2.2 在 `Program.cs` 中添加 SPA 回退路由中间件（`UseFallback` 或 `MapFallbackToFile("index.html")`），确保非 API 路径返回前端 SPA
- [x] 2.3 确认 SPA 回退路由仅影响非 `/api` 路径，API 请求不受干扰
- [x] 2.4 确认 `appsettings.json` 中无需额外配置即可支持 wwwroot 托管

## 3. Gitea Workflow 创建

- [x] 3.1 创建 `.gitea/workflows/build.yml`，定义 `on` 触发条件：`release: [published]` + `workflow_dispatch`（带 `environment` 选择参数）
- [x] 3.2 定义全局 `env`：`DOTNET_VERSION: '10.0'`，以及从 Gitea vars 读取的 `REGISTRY`、`IMAGE_NAME`
- [x] 3.3 创建 `build-and-push` job：包含 `actions/checkout@v4`、`docker/setup-buildx-action@v3`、`docker/login-action@v3`、`docker/build-push-action@v5`（使用提取的版本号）
- [x] 3.4 添加版本提取步骤：release 事件从 `GITHUB_REF` 提取 tag 版本，workflow_dispatch 生成 `dev-{sha}-{timestamp}` 格式版本
- [x] 3.5 创建 `deploy` job（`needs: build-and-push`，仅 release 事件执行）：通过 SSH 连接到 Debian 服务器，执行 `docker compose pull && up -d`
- [x] 3.6 创建 `publish-nuget` job（`needs: build-and-push`，仅 release 事件执行）：执行 `dotnet pack src/MinGo.Qap.Agent/` 并 `dotnet nuget push`

## 4. 生产部署配置

- [x] 4.1 创建 `deploy/docker-compose.yml`：定义 `platform` 服务（镜像从 `$REGISTRY/quartmanager` 拉取）和 `nginx` 服务（image: nginx:alpine），共享 `qap-network` bridge 网络
- [x] 4.2 配置 `platform` 服务：暴露内部 80 端口，设置环境变量 `ASPNETCORE_ENVIRONMENT=Production`、`ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`、`ConnectionStrings__PlatformDb` 等，配置 `restart: unless-stopped`
- [x] 4.3 配置 `nginx` 服务：映射宿主 443:443 和 80:80 端口，挂载 `./nginx/nginx.conf` 和 `./nginx/ssl/`，`depends_on: platform`
- [x] 4.4 创建 `deploy/nginx/nginx.conf`：配置 `upstream backend { server platform:80; }`，定义 HTTP→HTTPS 重定向和 HTTPS server block 的 proxy_pass
- [x] 4.5 创建 `deploy/.env.example`：列出 `REGISTRY`、`IMAGE_TAG`、`DB_CONNECTION_STRING` 等环境变量
- [x] 4.6 创建 `deploy/deploy.sh`：SSH 部署脚本模板（`docker compose pull && docker compose up -d --remove-orphans && docker image prune -f`）

## 5. 移除旧配置

- [x] 5.1 删除 `.gitea/workflows/cd.yml`
- [x] 5.2 删除 `.gitea/workflows/docker-build.yml`
- [x] 5.3 删除根目录 `docker-compose.yml`

## 6. 验证

- [x] 6.1 本地验证 Dockerfile 构建成功：`docker build -f src/MinGo.Qap.Platform/Dockerfile -t quartmanager:test . && docker run --rm -p 5000:80 quartmanager:test`，确认 UI 可访问且 API 正常
- [ ] 6.2 验证 Nginx 配置语法：`docker run --rm -v ./deploy/nginx/nginx.conf:/etc/nginx/nginx.conf:ro nginx:alpine nginx -t`（需在部署服务器上执行）
- [ ] 6.3 推送 CI pipeline 变更到 Gitea，验证 workflow 在 `workflow_dispatch` 下执行成功（需推到 Gitea 后验证）
