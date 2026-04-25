## Why

需要创建一个完整的 Sample 项目，展示如何在 ASP.NET Core 应用中集成 MinGo.Qap.Agent 库和 Quartz.NET（使用 RAMJobStore）。现有的 samples/Sample.Jobs 只是作业类库，不是可运行的应用。新手开发者需要看到一个端到端的使用示例，包括：
- Program.cs 配置
- DI 容器注册
- Quartz 调度器配置
- 作业触发和监控

## What Changes

- 在 samples/ 目录下创建新项目 `Sample.Agent` (ASP.NET Core Web API)
- 引用 MinGo.Qap.Agent 库并配置Agent相关使用
- 配置 Quartz 使用 RAMJobStore（内存存储）
- 提供基本的 REST API 用于查看和触发作业
- 包含 2-3 个示例作业

## Capabilities

### New Capabilities
- `quartz-agent-sample`: 创建完整的示例应用，展示 Agent + Quartz.NET RAMJobStore 集成

## Impact

- 新增 `samples/Sample.Agent/` 项目
- 新增 `samples/Sample.Agent.slnx` 解决方案
- 修改 `samples/Sample.Agent.slnx` 添加新项目引用