## Why

当前项目缺少自动化 CI/CD 流程，每次代码提交都需要手动构建和部署。通过集成 Gitea Actions，可以实现代码提交后的自动构建、测试和部署，提高开发效率和代码质量。

## What Changes

- 添加 `.gitea/workflows/ci.yml` - 持续集成工作流
- 添加 `.gitea/workflows/cd.yml` - 持续部署工作流
- 添加 `.gitea/workflows/docker-build.yml` - Docker 镜像构建工作流

## Capabilities

### New Capabilities
- `gitea-ci-workflow`: Gitea Actions CI 流程，包含代码检查、构建、单元测试
- `gitea-cd-workflow`: Gitea Actions CD 流程，包含镜像构建和部署

### Modified Capabilities
- (无)

## Impact

- 新增文件: `.gitea/workflows/*.yml`
- 无需额外的外部依赖
- 影响所有开发人员