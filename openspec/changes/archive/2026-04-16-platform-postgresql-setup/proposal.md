## Why

MinGo.QuartzManager Platform 目前使用 PostgreSQL 驱动但缺少 EF Core Migrations，导致数据库结构无法版本化管理，生产环境部署困难。需要建立完整的数据库持久化方案，支持开发环境自动迁移和生产环境手动迁移的最佳实践。

## What Changes

- **添加 EF Core Migrations**：为 PlatformDbContext 创建初始 Migration，包含 Clusters 和 JobDefinitions 表
- **添加 DesignTimeDbContextFactory**：支持 `dotnet ef` CLI 工具在设计和运行时正确读取配置
- **重构配置管理**：使用环境变量和 UserSecrets 读取数据库连接字符串，遵循 ASP.NET Core 标准配置优先级
- **区分迁移策略**：开发环境自动应用迁移（`Migrate()`），生产环境手动应用（`dotnet ef database update`）
- **更新 .csproj**：添加 EF Core CLI 工具引用

## Capabilities

### New Capabilities

- `database-persistence`: Platform 数据库持久化配置和管理
- `ef-core-migrations`: EF Core 迁移创建、应用和管理流程
- `configuration-management`: 环境变量和 UserSecrets 配置管理

### Modified Capabilities

（无 - 这是初始数据库设置）

## Impact

**代码影响范围**：
- `src/MinGo.Qap.Platform/Data/DesignTimeDbContextFactory.cs` - 新增
- `src/MinGo.Qap.Platform/Program.cs` - 修改（环境判断逻辑）
- `src/MinGo.Qap.Platform/appsettings.json` - 保持最小化
- `src/MinGo.Qap.Platform/appsettings.Development.json` - 添加连接字符串
- `src/MinGo.Qap.Platform/MinGo.Qap.Platform.csproj` - 添加工具引用

**构建影响**：
- 新增 `dotnet ef` 工具依赖（开发时）

**运行影响**：
- 开发环境：首次启动自动创建数据库和表
- 生产环境：需要预先手动运行迁移

**Agent 项目**：
- 无影响 - Agent 继续使用原生 Quartz 配置管理自己的数据存储
