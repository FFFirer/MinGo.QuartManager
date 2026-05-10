## ADDED Requirements

### Requirement: Agent NuGet 包自动打包
Gitea Workflow SHALL 在 Release 触发时自动执行 `dotnet pack` 打包 `MinGo.Qap.Agent` 项目为 NuGet 包。

#### Scenario: Release 触发 NuGet 打包
- **WHEN** Release published 事件触发 Workflow
- **THEN** Workflow SHALL 执行 `dotnet pack src/MinGo.Qap.Agent/MinGo.Qap.Agent.csproj -c Release`
- **THEN** 生成的 `.nupkg` 文件版本号 SHALL 与 Release tag 一致（例如 `v1.2.3` → 1.2.3）

### Requirement: NuGet 包自动推送到 NuGet 服务器
打包后的 NuGet 包 SHALL 自动推送到配置的 NuGet 服务器，服务器地址和 API Key 通过 Gitea secrets 管理。

#### Scenario: NuGet 包推送成功
- **WHEN** `dotnet pack` 成功生成 `.nupkg` 文件
- **THEN** Workflow SHALL 执行 `dotnet nuget push` 推送到 NuGet 服务器
- **THEN** NuGet 服务器 URL SHALL 使用 `${{ vars.NUGET_SERVER_URL }}`
- **THEN** API Key SHALL 使用 `${{ secrets.NUGET_API_KEY }}`

#### Scenario: NuGet 包版本冲突
- **WHEN** 同名版本已存在于 NuGet 服务器
- **THEN** Workflow SHALL 失败退出
- **THEN** 不影响 Docker 镜像的构建和推送
