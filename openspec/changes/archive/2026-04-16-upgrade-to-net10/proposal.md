## Why

当前项目存在技术碎片化问题：核心项目（Agent、Shared、Platform）已升级到 .NET 10，但示例项目（Sample.Jobs）仍停留在 .NET 8.0，Dockerfile 基础镜像也保持 8.0，NuGet 包版本与目标框架不一致。这种碎片化增加了维护复杂性，可能导致运行时兼容性问题，并使团队成员难以确定应该使用哪个 SDK 版本。统一所有组件到 .NET 10 是一次性技术债务清理，确保代码库一致性和长期可维护性。

## What Changes

- **项目文件**: Sample.Jobs.csproj 的 TargetFramework 从 net8.0 升级到 net10.0
- **Dockerfile**: Agent 和 Platform 的 Dockerfile 基础镜像从 `dotnet/aspnet:8.0` 和 `dotnet/sdk:8.0` 升级到 10.0
- **依赖包版本**: 
  - Microsoft.Extensions.Logging.Abstractions 从 8.0.0 升级到 10.x
  - 评估 Npgsql.EntityFrameworkCore.PostgreSQL 8.0.4 的兼容性，必要时升级到 10.x
- **SDK 锁定**: 可选添加 global.json 锁定 SDK 版本为 10.0.202
- **验证**: 确保所有项目在 .NET 10 上编译和运行正常

## Capabilities

### New Capabilities

<!-- 此变更仅为技术升级，不引入新功能，无需新增 capabilities -->

### Modified Capabilities

<!-- 此变更不修改现有功能的规格要求，仅升级实现技术栈 -->

## Impact

- **受影响的项目**: Sample.Jobs (主要变更), MinGo.Qap.Agent, MinGo.Qap.Shared, MinGo.Qap.Platform (Dockerfile 更新)
- **构建系统**: Docker 镜像需要重新构建
- **CI/CD**: 需要验证构建环境支持 .NET 10
- **开发者体验**: 团队成员需要确保本地安装了 .NET SDK 10.0.202
- **风险**: 低 - 核心项目已验证在 .NET 10 上运行，示例项目无下游依赖
