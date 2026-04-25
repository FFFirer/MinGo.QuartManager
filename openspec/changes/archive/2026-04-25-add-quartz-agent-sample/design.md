## Context

现有的 `samples/Sample.Jobs` 是一个作业类库，展示了如何定义 IJob 实现，但不包含：
- ASP.NET Core 应用配置
- Quartz 调度器生命周期管理
- REST API 端点
- Agent 库的集成方式

开发者需要看到端到端的使用示例，理解如何在实际应用中使用 MinGo.Qap.Agent。

## Goals / Non-Goals

**Goals:**
- 创建可运行的 ASP.NET Core Web API 项目
- 展示 Quartz.NET RAMJobStore 配置（纯内存，无需数据库）
- 展示 MinGo.Qap.Agent 库的 DI 集成
- 提供 REST API 查询/触发作业
- 包含 2-3 个示例作业作为演示

**Non-Goals:**
- 不使用 PostgreSQL 或其他持久化存储
- 不实现完整的集群功能
- 不需要 Docker 或容器化

## Decisions

### 1. 项目类型: Web API 而非 Worker
**决定**: 使用 ASP.NET Core Web API (空模板) 而非 BackgroundService

**理由**:
- Web API 提供现成的端点用于调试和触发作业
- swagger UI 便于开发者测试
- 与 MinGo Platform 的集成模式一致

**替代考虑**: BackgroundService - 更轻量，但需要额外工作添加健康检查端点

### 2. 存储方式: RAMJobStore (纯内存)

**决定**: 使用 Quartz 内置的 RAMJobStore

**理由**:
- 用户明确要求纯内存存储
- 配置最简单，无需额外 NuGet 包
- 适用于开发/调试场景
- 应用重启后作业丢失（符合 RAMJobStore 预期行为）

**替代考虑**: SQLite - 需要额外配置，重启后能保留，但不满足纯内存需求

### 3. 示例作业设计
**决定**: 实现 3 个作业
- `HelloJob`: 简单 Hello World，每 10 秒执行
- `ScheduledJob`: 定时报告，检查集群健康
- `ManualTriggerJob`: 手动触发，模拟耗时任务

**理由**:
- 覆盖自动触发和手动触发场景
- 简单易懂，便于修改

## Risks / Trade-offs

**风险**: 配置复杂度
- **缓解**: 使用 appsettings.json 而非 config.yaml，保持简单

**风险**: 作业类库引用
- **缓解**: 创建项目后将 Sample.Jobs 作为项目引用