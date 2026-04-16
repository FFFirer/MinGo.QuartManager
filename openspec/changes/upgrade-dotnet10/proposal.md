## Why

项目当前目标框架已设置为 net10.0，但依赖包仍停留在 .NET 8 时代。Npgsql/EF Core 为 8.x 版本，与 .NET 10 运行时不完全匹配。为确保稳定性、性能和未来兼容性，需要进行完整的依赖现代化升级。

## What Changes

- **Platform 项目**
  - Npgsql.EntityFrameworkCore.PostgreSQL: 8.0.4 → 10.0.1
  - Microsoft.EntityFrameworkCore.Design: 8.0.4 → 10.0.4
  - Swashbuckle.AspNetCore: 6.5.0 → 10.1.7

- **Agent 项目**
  - Quartz.Serialization.Json: 3.8.1 → 3.17.1
  - YamlDotNet: 15.1.4 → 16.3.0

- **Sample.Jobs 项目**
  - Quartz: 3.9.0 → 3.17.1
  - Microsoft.Extensions.Logging.Abstractions: 8.0.2 → 10.0.5

- **Breaking Changes**
  - **Npgsql 10.0**: `date`/`time` 类型默认映射变更，需检查日期处理代码
  - **Npgsql 10.0**: `cidr` 类型映射到 `IPNetwork`
  - **YamlDotNet 16.0**: `ITypeConverter` 接口签名变化
  - **Quartz 3.17**: 可选数据库 schema 迁移 (新列 `MISFIRE_ORIG_FIRE_TIME`)
  - **Swashbuckle 10.0**: OpenAPI 规范从 3.0 升级到 3.1

## Capabilities

### New Capabilities

- `dotnet-upgrade`: 记录 .NET 升级的技术决策和注意事项

### Modified Capabilities

- `database-persistence`: Npgsql 10.0 的日期类型映射变更可能影响数据库实体定义
- `ef-core-migrations`: EF Core 版本升级后 Migration 工具行为检查

## Impact

- 4 个项目文件的 csproj 包引用版本更新
- 2 个 Dockerfile 基础镜像已为 net10，无需修改
- 数据库迁移脚本需测试兼容性
- YAML 配置文件解析代码需检查类型转换器
- OpenAPI/Swagger 生成的 API 文档格式变化
