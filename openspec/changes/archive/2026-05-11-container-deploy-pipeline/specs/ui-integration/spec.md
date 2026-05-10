## ADDED Requirements

### Requirement: UI 生产构建自动集成到 Platform wwwroot
Platform 的 Docker 构建过程中，SHALL 自动执行 Vite 生产构建，并将输出产物拷贝到 ASP.NET Core 的 `wwwroot/` 目录下，使 Kestrel 直接托管前端 SPA。

#### Scenario: Docker 构建包含 UI 编译步骤
- **WHEN** 执行 `docker build` 构建 Platform 镜像
- **THEN** Node 构建阶段 SHALL 执行 `pnpm install && pnpm build` 生成 `dist/`
- **THEN** .NET 构建阶段 SHALL 将 `dist/` 内容复制到 `wwwroot/` 目录
- **THEN** 最终镜像的 `/app/wwwroot/` SHALL 包含完整的前端静态资源

#### Scenario: UI 构建失败中断 Pipeline
- **WHEN** `pnpm build` 在 Docker 构建阶段失败
- **THEN** 整个 Docker 构建 SHALL 失败退出
- **THEN** 不会生成新的镜像
- **THEN** 已存在的镜像 tag 不受影响

### Requirement: Platform 路由适配 SPA 历史模式
Platform 的 ASP.NET Core 中间件 SHALL 配置 SPA 回退路由，使所有非 API 路径请求返回 `index.html`，确保 React Router 的前端路由正常工作。

#### Scenario: 前端路由刷新正确响应
- **WHEN** 浏览器访问 `/schedulers` 路径
- **THEN** 服务器 SHALL 返回 `wwwroot/index.html`（而非 404）
- **THEN** React SPA SHALL 加载并渲染 SchedulersPage 组件

#### Scenario: API 请求不受 SPA 路由影响
- **WHEN** 浏览器或 HTTP 客户端访问 `/api/agents` 路径
- **THEN** 服务器 SHALL 正常处理 API 请求
- **THEN** 不会触发 SPA 回退路由
