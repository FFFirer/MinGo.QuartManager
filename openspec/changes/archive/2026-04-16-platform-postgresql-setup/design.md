## Context

**当前状态**：
- Platform 项目已引用 `Npgsql.EntityFrameworkCore.PostgreSQL` (8.0.4)
- `PlatformDbContext` 已配置使用 PostgreSQL 但缺少 Migrations
- 连接字符串硬编码在 `Program.cs` 中
- 使用 `db.Database.Migrate()` 自动迁移但缺少 Migration 文件

**约束条件**：
- 遵循 ASP.NET Core 标准配置模式
- Agent 项目保持独立，使用原生 Quartz 配置
- 开发环境友好（自动迁移），生产环境安全（手动迁移）

## Goals / Non-Goals

**Goals:**
1. 建立完整的数据库持久化方案（Clusters + JobDefinitions 表）
2. 实现 EF Core Migrations 版本化管理
3. 支持环境变量和 UserSecrets 读取连接字符串
4. 区分开发和生产的迁移策略
5. 提供 DesignTimeFactory 支持 CLI 工具

**Non-Goals:**
- 修改 Agent 项目的存储配置
- 添加额外的数据库表（如执行日志）
- 支持除 PostgreSQL 外的其他数据库
- 实现复杂的多租户数据隔离

## Decisions

### 1. 配置读取优先级
**决策**：使用 ASP.NET Core 标准配置链
```
环境变量 (QAP_DB_CONNECTION) > UserSecrets > appsettings.Development.json > appsettings.json
```
**理由**：
- 符合 ASP.NET Core 生态标准
- 支持容器部署（环境变量）
- 保护敏感信息（UserSecrets 开发时）
- 团队协作友好（appsettings.Development.json 提交到仓库）

### 2. DesignTimeDbContextFactory 实现
**决策**：创建 `DesignTimeDbContextFactory` 实现 `IDesignTimeDbContextFactory`
**理由**：
- 支持 `dotnet ef migrations add` 无需启动应用
- 读取配置逻辑与运行时保持一致
- 处理相对路径和项目结构的复杂性

### 3. 迁移策略分离
**决策**：
- 开发环境：`Migrate()` 自动应用最新迁移
- 生产环境：需手动运行 `dotnet ef database update`
**理由**：
- 开发体验：快速迭代，无需手动步骤
- 生产安全：避免意外 Schema 变更，支持蓝绿部署

### 4. 表命名和结构
**决策**：保持现有 `PlatformDbContext.OnModelCreating` 配置不变
**理由**：
- 业务逻辑已稳定定义
- Migration 应反映当前状态，不引入破坏性变更

## Risks / Trade-offs

| 风险 | 影响 | 缓解措施 |
|------|------|----------|
| 生产环境忘记运行迁移 | 高 | 文档明确标注，CI/CD 流程集成检查 |
| 开发/生产数据库 Schema 不一致 | 中 | 所有环境使用同一 Migration 文件 |
| 连接字符串泄露到日志 | 低 | 确保 `UserSecrets` 和 `.gitignore` 正确配置 |
| Migration 文件冲突 | 低 | 团队约定：单人负责 Migration 创建 |

**Trade-offs:**
- 选择分离数据库（Platform DB vs Quartz DB）：增加运维复杂度，但实现关注点分离
- 选择 PostgreSQL 专用：无法轻松切换到其他数据库，但获得最佳性能和功能支持

## Migration Plan

**开发环境部署**：
1. 克隆代码
2. 配置连接字符串（UserSecrets 或 appsettings.Development.json）
3. 运行 `dotnet run` → 自动创建数据库和表

**生产环境部署**：
1. 配置环境变量 `QAP_DB_CONNECTION`
2. 运行 `dotnet ef database update`
3. 验证数据库 Schema
4. 启动应用

**回滚策略**：
- 使用 `dotnet ef database update <migration-name>` 回退到特定版本
- 建议：生产环境变更前备份数据库

## Open Questions

（无 - 方案已明确）
