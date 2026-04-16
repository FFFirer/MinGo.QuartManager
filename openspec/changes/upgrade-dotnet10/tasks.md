## 1. 依赖版本更新

- [x] 1.1 更新 MinGo.Qap.Platform.csproj 包版本
  - Npgsql.EntityFrameworkCore.PostgreSQL: 8.0.4 → 10.0.1
  - Microsoft.EntityFrameworkCore.Design: 8.0.4 → 10.0.4
  - Swashbuckle.AspNetCore: 6.5.0 → 10.1.7

- [x] 1.2 更新 MinGo.Qap.Agent.csproj 包版本
  - Quartz.Serialization.Json: 3.8.1 → 3.17.1
  - YamlDotNet: 15.1.4 → 16.3.0

- [x] 1.3 更新 Sample.Jobs.csproj 包版本
  - Quartz: 3.9.0 → 3.17.1
  - Microsoft.Extensions.Logging.Abstractions: 8.0.2 → 10.0.5

## 2. 构建验证

- [x] 2.1 执行 `dotnet restore` 验证包还原
- [x] 2.2 执行 `dotnet build` 确保编译通过
- [x] 2.3 检查并修复任何编译警告

## 3. 代码适配

- [x] 3.1 检查日期/时间类型使用情况
  - 审查实体中的 DateTime 类型字段
  - 评估是否需要启用 LegacyDateAndTimeResolver

- [x] 3.2 检查 YamlDotNet 类型转换器
  - 搜索项目中的 ITypeConverter 实现
  - 如有自定义转换器，更新接口实现

- [x] 3.3 检查 EF Core 使用情况
  - 验证 DbContext 配置正常
  - 检查 Migration 文件兼容性

## 4. 测试验证

- [x] 4.1 运行项目验证基本功能
  - Platform 服务启动验证
  - Agent 服务启动验证

- [ ] 4.2 Docker 构建测试 (环境限制：Docker 不可用)
  - `docker build` Platform Dockerfile
  - `docker build` Agent Dockerfile

## 5. 收尾工作

- [ ] 5.1 检查是否有 Quartz schema 迁移需求
- [ ] 5.2 更新 README.md 中的版本信息（如需要）
- [ ] 5.3 提交代码变更
