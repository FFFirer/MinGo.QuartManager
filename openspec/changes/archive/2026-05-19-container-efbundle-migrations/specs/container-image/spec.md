## ADDED Requirements

### Requirement: Docker image build MUST produce efbundle

容器镜像构建过程中 MUST 生成 EF Core 迁移 bundle（efbundle），用于生产环境的数据库迁移。

#### Scenario: 构建同时产生应用发布输出和 efbundle
- **WHEN** CI 执行 Docker 构建
- **AND** dotnet-build 阶段完成
- **THEN** 构建产物中同时包含 `MinGo.Qap.Platform.dll`（应用）和 `efbundle`（迁移工具）
- **AND** runtime 镜像中这两者均可用
