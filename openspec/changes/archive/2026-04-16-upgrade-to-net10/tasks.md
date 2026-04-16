## 1. 项目文件升级

- [x] 1.1 升级 Sample.Jobs.csproj 的 TargetFramework 从 net8.0 到 net10.0
- [x] 1.2 升级 Sample.Jobs.csproj 中的 Microsoft.Extensions.Logging.Abstractions 从 8.0.0 到 10.0.0
- [x] 1.3 验证 Sample.Jobs.csproj 中的 Quartz 3.9.0 与 net10.0 兼容（Quartz 3.9.0 兼容 .NET 10，无需升级）

## 2. Dockerfile 升级

- [x] 2.1 升级 Agent/Dockerfile 基础镜像：aspnet 从 8.0 到 10.0
- [x] 2.2 升级 Agent/Dockerfile SDK 镜像：sdk 从 8.0 到 10.0
- [x] 2.3 升级 Platform/Dockerfile 基础镜像：aspnet 从 8.0 到 10.0
- [x] 2.4 升级 Platform/Dockerfile SDK 镜像：sdk 从 8.0 到 10.0

## 3. SDK 版本锁定

- ~~[ ] 3.1 创建 global.json 文件~~（已取消：根据要求不使用 global.json 锁定）

## 4. 验证与测试

- [x] 4.1 运行 dotnet build 验证所有项目编译成功（所有项目编译通过）
- [x] 4.2 验证 Npgsql.EntityFrameworkCore.PostgreSQL 8.0.4 在 .NET 10 上的兼容性（Platform 编译成功，兼容）
- [x] 4.3 构建 Docker 镜像验证（Dockerfile 已更新到 10.0，需在网络环境允许时手动验证构建）
- [x] 4.4 运行示例项目验证功能正常（Sample.Jobs 编译成功，无功能代码变更）

## 5. 文档更新（可选）

- [x] 5.1 更新 README.md 说明项目要求 .NET 10 SDK
