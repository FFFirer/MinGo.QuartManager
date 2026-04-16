# Quartz.NET WebUI 

## 系统要求

- [.NET SDK 10.0 或更高版本](https://dotnet.microsoft.com/download/dotnet/10.0)
- Docker（可选，用于容器化部署）

## 技术栈

- **目标框架**: .NET 10.0
- **主要依赖**:
  - Quartz.NET 3.17.1 - 作业调度
  - Npgsql.EntityFrameworkCore.PostgreSQL 10.0.1 - PostgreSQL 数据持久化
  - ASP.NET Core - Web API 和托管

## 项目结构

```
src/
  MinGo.Qap.Agent/      - 代理服务（net10.0）
  MinGo.Qap.Shared/     - 共享库（net10.0）
  MinGo.Qap.Platform/   - 平台服务（net10.0）
samples/
  Sample.Jobs/          - 示例作业（net10.0）
```

## 快速开始

1. 确保已安装 .NET SDK 10.0
2. 克隆仓库
3. 运行 `dotnet build` 编译所有项目
4. 使用 Docker Compose 启动服务：`docker-compose up`

## 开发说明

项目目标框架为 .NET 10.0，请确保开发环境已安装 .NET SDK 10.0 或更高版本。
