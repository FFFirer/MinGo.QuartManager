## Context

本项目是一个基于 .NET 10 的 Quartz 定时任务管理平台，使用 PostgreSQL 数据库。目前没有自动化 CI/CD 流程，每次代码变更都需要手动执行构建和部署操作。

## Goals / Non-Goals

**Goals:**
- 实现代码提交后自动触发 CI 流程（代码检查、构建、测试）
- 实现 CI 通过后自动触发 CD 流程（镜像构建、部署）
- 支持多环境部署（dev, staging, production）

**Non-Goals:**
- 不包含性能测试和负载测试
- 不包含手动部署回滚操作（通过版本控制实现自动回滚）
- 不涉及复杂的灰度发布策略

## Decisions

1. **使用 Gitea Actions 而非 GitHub Actions**
   - 项目托管在 Gitea 上，原生支持 Actions
   - 无需额外的外部服务配置

2. **CI 流程包含步骤**
   - 代码格式检查 (dotnet format)
   - 静态代码分析 (dotnet build / nowarn)
   - 单元测试 (dotnet test)
   - 构建产物发布

3. **CD 流程触发策略**
   - 仅在 `main` 分支合并时触发
   - 支持手动触发部署

4. **Docker 镜像构建**
   - 使用多阶段构建优化镜像大小
   - 镜像标签使用 commit SHA 或 Git tag

## Risks / Trade-offs

- [风险] Gitea Actions runner 需要自行搭建 →  Mitigation: 使用 Docker Runner 或 Actions Runner Controller
- [风险] 部署目标不确定 →  Mitigation: CD 工作流支持可配置的部署目标，默认跳过实际部署仅构建镜像