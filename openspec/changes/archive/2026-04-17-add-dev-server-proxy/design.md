## Context

前端项目 (MinGo.Qap.UI) 使用 Vite 作为开发服务器，后端项目 (MinGo.Qap.Platform) 默认运行在 `http://localhost:5256`。当前前端 API 请求直接使用硬编码的 `http://localhost:5000`，存在以下问题：
- 跨域请求问题
- 开发环境与生产环境 API 地址需要手动切换
- 本地开发时后端端口不匹配

## Goals / Non-Goals

**Goals:**
- 配置 Vite 开发服务器代理，解决开发环境跨域问题
- 前端使用相对路径 `/api` 请求接口，无需硬编码后端地址
- 代理配置仅在开发环境生效，生产环境保持原有行为

**Non-Goals:**
- 不修改后端 CORS 配置
- 不涉及生产环境部署配置
- 不修改现有的 API 调用代码结构

## Decisions

### 1. 使用 Vite 原生代理而非 http-proxy-middleware

**决定**: 在 `vite.config.ts` 中使用 Vite 内置的 `server.proxy` 配置

**理由**:
- Vite 内置代理基于 `http-proxy`，开箱即用，无需额外安装依赖
- 配置简洁，与 Vite 配置风格一致
- 支持热模块替换，配置更改自动生效
- 支持 WebSocket 代理（用于 HMR）

**替代方案**:
- `http-proxy-middleware`: 需要额外安装，配合 Vite 需要额外配置，不推荐

### 2. 代理目标为后端 HTTP 端口

**决定**: 代理到 `http://localhost:5256`

**理由**:
- 开发环境默认使用 HTTP，配置简单
- 后端 launchSettings.json 中 `http` profile 配置为端口 5256
- 开发阶段无需 HTTPS 复杂度

**替代方案**:
- 代理到 HTTPS (`https://localhost:7225`): 需要处理自签名证书问题，增加配置复杂度，开发阶段不必要

### 3. 前端 API 地址改为相对路径

**决定**: 将 `src/api/index.ts` 中的 `baseURL` 改为 `/api`

**理由**:
- 相对路径配合代理配置，天然适配开发/生产环境
- 开发时请求 `/api/*` 被代理到后端
- 生产构建后使用相对路径，适配部署环境

### 4. 环境变量配置后端地址（可选增强）

**决定**: 保留 `VITE_API_URL` 环境变量作为 fallback

**理由**:
- 提供灵活性，允许通过环境变量覆盖默认代理目标
- 不影响现有配置模式
- 便于未来扩展到其他开发场景

## Risks / Trade-offs

| 风险 | 影响 | 缓解措施 |
|------|------|----------|
| 后端端口变更 | 代理目标失效 | 统一使用 launchSettings.json 中的端口，保持文档更新 |
| 生产环境 API 地址不同 | 请求失败 | 构建时通过环境变量配置生产 API 地址 |
| 部分接口不需要代理 | 不必要的代理转发 | 使用 `bypass` 函数排除特定路径 |

## Open Questions

- 后端是否需要配置 CORS？（即使使用代理，生产环境仍需 CORS 配置）
- 是否需要配置 HTTPS 代理用于本地开发调试？
