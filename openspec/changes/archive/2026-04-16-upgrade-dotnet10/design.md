## Context

项目当前依赖包版本与 .NET 10 目标框架不匹配：
- Npgsql/EF Core: 8.x (面向 .NET 8)
- Swashbuckle: 6.5.0 (旧版)
- Quartz/YamlDotNet: 旧版本

.NET 10 已于 2025年11月正式发布，相关生态包已全面支持。

## Goals / Non-Goals

**Goals:**
- 所有 NuGet 包升级到与 .NET 10 兼容的最新稳定版本
- 确保升级后项目可正常编译和运行
- 记录Breaking Changes并提供迁移指导

**Non-Goals:**
- 不引入新功能或架构变更
- 不修改数据库Schema（除非是依赖包内置的兼容性修复）
- 不升级 PostgreSQL 服务器版本要求

## Decisions

### 1. 包版本选择策略

**决定**: 使用各包的最新稳定版(GA)

| 包 | 目标版本 | 理由 |
|---|---|---|
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.1 | .NET 10 GA版，2025-11发布 |
| Microsoft.EntityFrameworkCore.Design | 10.0.4 | 配套EF Core版本 |
| Swashbuckle.AspNetCore | 10.1.7 | 最新稳定版，支持OpenAPI 3.1 |
| Quartz | 3.17.1 | 最新稳定版，支持.NET 10 |
| Quartz.Serialization.Json | 3.17.1 | 与Quartz主包同步 |
| YamlDotNet | 16.3.0 | 最新稳定版，支持.NET 10 |
| Microsoft.Extensions.Logging.* | 10.0.5 | 配套.NET 10版本 |

### 2. Npgsql 日期类型映射处理

**决定**: 暂不启用 `LegacyDateAndTimeResolver`，接受新默认值

Npgsql 10.0 将 `date` 映射到 `DateOnly`，`time` 映射到 `TimeOnly`。检查现有实体：
- 如代码中大量使用 `DateTime` 处理日期，评估是否需要 `LegacyDateAndTimeResolverFactory`
- 如主要为数据库存储用途，保持新映射减少不必要转换

### 3. YamlDotNet 类型转换器迁移

**决定**: 检查并更新自定义 `ITypeConverter` 实现

YamlDotNet 16.0 变更：
- `ITypeConverter` 现在接收 `ITypeConverter` 和 `IPropertyDescriptor` 参数
- 需要调用 `BuildTypeConverter()` 获取转换器实例

### 4. Quartz 数据库Schema迁移

**决定**: 评估执行可选的 schema 迁移脚本

Quartz 3.17 新增 `MISFIRE_ORIG_FIRE_TIME` 列：
- 如使用集群模式，强烈建议执行迁移
- 迁移脚本位于 Quartz 包内或 Quartz.NET 仓库

## Risks / Trade-offs

| 风险 | 影响 | 缓解措施 |
|---|---|---|
| Npgsql 日期类型映射变更 | 日期查询结果类型变化 | 代码审查 + 单元测试 |
| YamlDotNet API 变更 | 自定义类型转换器编译失败 | 更新接口实现 |
| EF Core Migration 工具行为变更 | 生成的迁移代码差异 | 检查迁移差异，不自动应用 |
| Swashbuckle OpenAPI 3.1 | 旧版客户端兼容性问题 | 大多数客户端兼容3.1 |

## Migration Plan

### 阶段1: 依赖升级
1. 更新所有 csproj 中的 `PackageReference` 版本号
2. 执行 `dotnet restore` 验证包可还原
3. 执行 `dotnet build` 确保编译通过

### 阶段2: 代码适配
1. 检查 Npgsql 日期类型使用情况
2. 更新 YamlDotNet 类型转换器（如有）
3. 运行测试验证功能正常

### 阶段3: 验证部署
1. Docker 构建测试
2. 本地运行验证
3. 合并后 CI/CD 自动验证

## Open Questions

1. **是否需要启用 `LegacyDateAndTimeResolver`** - 取决于现有代码中日期处理方式
2. **Quartz schema 迁移时机** - 开发环境先执行，生产环境后续迭代
3. **YamlDotNet 16.x 行为变更** - 需要实际测试自定义类型转换器
