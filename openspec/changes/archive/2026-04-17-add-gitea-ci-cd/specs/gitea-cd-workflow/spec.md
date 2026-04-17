## ADDED Requirements

### Requirement: CD 工作流触发条件
Gitea Actions CD 工作流 SHALL 在 main 分支合并时自动触发，或支持手动触发。

#### Scenario: Main 分支合并触发 CD
- **WHEN** 代码合并到 main 分支
- **THEN** Gitea Actions 自动触发 CD 工作流

#### Scenario: 手动触发 CD
- **WHEN** 用户通过 Gitea UI 手动触发工作流
- **THEN** CD 工作流执行

### Requirement: Docker 镜像构建
CD 工作流 SHALL 构建 Docker 镜像并推送到镜像仓库。

#### Scenario: 镜像构建成功
- **WHEN** Dockerfile 配置正确且构建成功
- **THEN** 镜像被推送到指定的镜像仓库

#### Scenario: 镜像构建失败
- **WHEN** Dockerfile 配置错误或构建失败
- **THEN** CD 工作流失败，显示构建错误详情

### Requirement: 镜像版本标签
CD 工作流 SHALL 为镜像添加版本标签，便于追踪部署版本。

#### Scenario: 使用 Git SHA 作为标签
- **WHEN** 自动触发构建时
- **THEN** 镜像标签使用 Git commit SHA

#### Scenario: 使用 Git Tag 作为标签
- **WHEN** 使用 Git Tag 触发构建时
- **THEN** 镜像标签使用 Tag 版本号