## ADDED Requirements

### Requirement: efbundle MUST be built during Docker image build

Docker 镜像构建的 dotnet-build 阶段 MUST 执行 `dotnet ef migrations bundle`，生成框架依赖的迁移可执行文件。

#### Scenario: efbundle 在 dotnet-build 阶段成功生成
- **WHEN** Docker 构建执行 dotnet-build 阶段
- **AND** `dotnet ef migrations bundle` 命令完成
- **THEN** 在 `/app/publish/efbundle` 路径下生成 efbundle 二进制文件（framework-dependent）
- **AND** 构建日志记录 efbundle 生成成功

### Requirement: efbundle MUST be included in runtime image

构建生成的 efbundle MUST 通过多阶段构建的 COPY 指令被包含在 runtime 镜像中，路径为 `/app/efbundle`。

#### Scenario: runtime 镜像包含 efbundle
- **WHEN** runtime 阶段从 dotnet-build 阶段复制 `/app/publish` 目录
- **THEN** runtime 镜像的 `/app/efbundle` 路径下存在 efbundle 二进制文件
- **AND** 该文件在容器中可通过 `dotnet /app/efbundle.dll` 执行

### Requirement: efbundle 必须可执行

efbundle 二进制文件 MUST 在镜像中具有可执行权限。

#### Scenario: efbundle 权限正确
- **WHEN** 查看 runtime 镜像中 `/app/efbundle` 的文件权限
- **THEN** 文件具有可执行权限（`chmod +x`）
