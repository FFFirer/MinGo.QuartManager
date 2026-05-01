# MinGo.Qap - Quartz.NET Agent & Platform

Quartz.NET 分布式 Agent 管理与调度平台。

## 系统要求

- [.NET SDK 10.0 或更高版本](https://dotnet.microsoft.com/download/dotnet/10.0)
- PostgreSQL 15+
- Docker（可选，用于容器化部署）

## 技术栈

- **目标框架**: .NET 10.0
- **主要依赖**:
  - Quartz.NET 3.17.1 - 作业调度
  - Npgsql.EntityFrameworkCore.PostgreSQL 10.0.1 - 数据持久化
  - ASP.NET Core - Web API 和托管
  - React 19 + TypeScript + Vite - 前端 UI

## 架构

```
┌─────────────────────────────────────────────────────┐
│                    Platform (Web API)               │
│  ┌──────────┐  ┌──────────┐  ┌──────────────────┐  │
│  │ Agent    │  │Scheduler │  │  Job             │  │
│  │ Service  │  │Service   │  │  Service         │  │
│  └────┬─────┘  └────┬─────┘  └───────┬──────────┘  │
│       │              │                │              │
│  ┌────▼──────────────▼────────────────▼──────────┐  │
│  │          SchedulerRouterService               │  │
│  │     (SchedulerName → Agent 路由)              │  │
│  └───────────────────┬──────────────────────────┘  │
│                      │ HTTP                         │
├──────────────────────┼──────────────────────────────┤
│              ┌───────▼────────┐                     │
│              │   Agent HTTP   │                     │
│              │   API Proxy   │                     │
│              └───────┬────────┘                     │
└──────────────────────┼──────────────────────────────┘
                       │ HTTP
┌──────────────────────┼──────────────────────────────┐
│              ┌───────▼────────┐                     │
│              │    Agent       │                     │
│  ┌───────────┴──────────────┐ │                     │
│  │ HostedAgentService       │ │                     │
│  │  (注册/心跳/Scheduler上报)│ │                     │
│  └───────────┬──────────────┘ │                     │
│  ┌───────────▼──────────────┐ │                     │
│  │ IAgentSchedulerAccessor  │ │                     │
│  │ → AgentSchedulerAccessor │ │                     │
│  │ → DeferredSchedulerAcc.  │ │                     │
│  └───────────┬──────────────┘ │                     │
│  ┌───────────▼──────────────┐ │                     │
│  │   Quartz.NET Scheduler   │ │                     │
│  │   (IScheduler 实例)      │ │                     │
│  └──────────────────────────┘ │                     │
└──────────────────────────────┘──────────────────────┘
```

### 核心概念

- **Agent**: 内嵌于宿主程序的 Quartz 代理，负责注册、心跳、Scheduler 上报
- **Scheduler**: Quartz.NET 调度器实例，Agent 可持有多个
- **Platform**: 中心化管理平台，提供 Agent/Scheduler/Job 管理 API
- **Agent 身份持久化**: 首次注册由 Platform 分配 AgentId，本地 `agent-identity.json` 持久化

## 项目结构

```
src/
  MinGo.Qap.Agent/      - Agent 代理类库（嵌入宿主程序）
    Services/
      IAgentSchedulerAccessor.cs   # Scheduler 访问接口
      AgentSchedulerAccessor.cs    # 默认实现
      DeferredSchedulerAccessor.cs # 延迟发现
      IAgentIdentityStore.cs       # 身份持久化接口
      AgentIdentityFileStore.cs    # 文件实现
      SchedulerReporterService.cs  # Scheduler 上报服务
      HostedAgentService.cs        # 生命周期管理
      AgentRegistrationService.cs  # 注册服务
      QuartzService.cs             # Quartz 封装
  MinGo.Qap.Shared/     - 共享 DTO（net10.0）
  MinGo.Qap.Platform/   - 平台 Web API（net10.0）
    Controllers/
      AgentsController.cs
      SchedulersController.cs
      JobsController.cs
    Services/
      AgentService.cs
      SchedulerService.cs
      SchedulerRouterService.cs
      AgentProxyService.cs
      JobService.cs
    Data/
      Entities/Agent.cs, SchedulerInfo.cs, AgentScheduler.cs
      PlatformDbContext.cs
      UtcAuditInterceptor.cs
  MinGo.Qap.UI/         - React 前端
    pages/
      AgentsPage.tsx, AgentDetailPage.tsx
      SchedulersPage.tsx, SchedulerDetailPage.tsx
      JobsPage.tsx, JobDetailPage.tsx
      PlatformDashboardPage.tsx
samples/
  Sample.Jobs/          - 示例作业（net10.0）
```

## 快速开始

1. 确保已安装 .NET SDK 10.0
2. 克隆仓库
3. 运行 `dotnet build` 编译所有项目
4. 配置 PostgreSQL 连接字符串（`appsettings.json` 或环境变量）
5. 运行 `dotnet ef database update` 应用数据库迁移
6. 启动 Platform：`dotnet run --project src/MinGo.Qap.Platform`
7. 启动 UI：`cd src/MinGo.Qap.UI && pnpm install && pnpm dev`

## API 路由

| 方法 | 路径 | 说明 |
|------|------|------|
| POST | `/api/agents` | 注册 Agent |
| GET | `/api/agents` | Agent 列表 |
| GET | `/api/agents/{id}` | Agent 详情 |
| DELETE | `/api/agents/{id}` | 删除 Agent |
| POST | `/api/agents/{id}/heartbeat` | 心跳 |
| POST | `/api/agents/{id}/schedulers` | 上报 Scheduler |
| GET | `/api/agents/{id}/schedulers` | 查询 Agent Scheduler |
| GET | `/api/schedulers` | Scheduler 列表 |
| GET | `/api/schedulers/{name}` | Scheduler 详情 |
| GET | `/api/schedulers/{name}/agents` | Scheduler Agents |
| GET | `/api/schedulers/{name}/jobs` | Job 列表 |
| POST | `/api/schedulers/{name}/jobs` | 创建 Job |
| PUT | `/api/schedulers/{name}/jobs/{key}` | 更新 Job |
| DELETE | `/api/schedulers/{name}/jobs/{key}` | 删除 Job |

详细 API 文档见 [docs/api-reference.md](docs/api-reference.md)。

## 数据库迁移

```bash
# 生成新的迁移
dotnet ef migrations add <MigrationName> --project src/MinGo.Qap.Platform

# 应用到数据库
dotnet ef database update --project src/MinGo.Qap.Platform

# 数据迁移脚本
# 参见 docs/migrations/v2_migrate_agent_instance_to_agent.sql
# 参见 docs/migrations/v2_migrate_cluster_deprecate.sql
```

## 开发说明

项目目标框架为 .NET 10.0，请确保开发环境已安装 .NET SDK 10.0 或更高版本。
前端使用 pnpm 作为包管理器。
