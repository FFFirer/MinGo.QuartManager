## Context

当前代码库存在 .NET 版本碎片化问题：

```
当前状态:
├── Agent (net10.0) ✓
├── Shared (net10.0) ✓
├── Platform (net10.0) ✓
└── Sample.Jobs (net8.0) ⚠️ 待升级

Docker 镜像: aspnet:8.0, sdk:8.0 (需要更新)
依赖包: Microsoft.Extensions.* 8.0.0 (需要更新)
```

核心项目已在 .NET 10 上验证运行，示例项目是独立的演示项目，没有下游依赖，升级风险较低。

## Goals / Non-Goals

**Goals:**
- 统一所有项目到 .NET 10 目标框架
- 更新所有 Dockerfile 基础镜像到 .NET 10
- 升级 Microsoft.Extensions.* 包到 10.x 版本
- 验证所有项目编译和运行正常

**Non-Goals:**
- 修改任何功能代码或业务逻辑
- 修改 API 契约或接口
- 添加新功能
- 升级 Quartz（保持 3.9.0，验证兼容性即可）
- 强制要求 Npgsql.EntityFrameworkCore.PostgreSQL 升级到 10.x（评估后决定）

## Decisions

### 1. 依赖包版本策略

**决策**: Microsoft.Extensions.Logging.Abstractions 升级到 10.x，Npgsql.EntityFrameworkCore.PostgreSQL 保持 8.0.4（除非验证发现兼容性问题）。

**理由**:
- EF Core 8.x 与 .NET 10 运行时应该兼容
- EF Core 9.x/10.x 可能有破坏性变更，需要额外测试
- 保持变更范围最小，如果 8.0.4 能正常工作则无需升级

**替代方案考虑**:
- 升级到 Npgsql.EntityFrameworkCore.PostgreSQL 10.x: 更彻底的统一，但引入额外风险
- 结论: 先尝试保持 8.0.4，如有问题再升级

### 2. global.json 添加策略

**决策**: 添加 global.json 锁定 SDK 版本为 10.0.202

**理由**:
- 确保团队成员使用一致的 SDK 版本
- 避免不同开发者使用不同 SDK 版本导致的构建差异
- 明确项目要求的最低 SDK 版本

**替代方案**:
- 不添加 global.json: 依赖开发者自觉，可能有版本差异
- 结论: 添加 global.json 更好

### 3. Dockerfile 镜像选择

**决策**: 使用 `mcr.microsoft.com/dotnet/aspnet:10.0` 和 `sdk:10.0`

**理由**:
- 使用带 tag 的版本而非 `latest`，确保可重复构建
- 10.0 标签会指向最新的 10.x 补丁版本，自动获得安全更新

## Risks / Trade-offs

| 风险 | 可能性 | 影响 | 缓解措施 |
|------|--------|------|----------|
| EF Core 8.x 与 .NET 10 运行时不兼容 | 低 | 中 | 本地编译验证，发现问题再升级 EF Core |
| Docker 镜像 10.0 标签不可用 | 极低 | 高 | 检查微软容器仓库，使用完整版本号如 10.0.2 |
| 团队成员未安装 .NET 10 SDK | 中 | 低 | global.json 会提示安装，文档中说明要求 |
| Quartz 3.9.0 在 .NET 10 上有问题 | 低 | 中 | Quartz 3.x 支持 .NET 8+，应该兼容 10 |

## Migration Plan

**部署步骤**:
1. 在本地完成所有代码变更
2. 运行 `dotnet build` 验证所有项目编译成功
3. 运行单元测试（如有）
4. 构建 Docker 镜像验证
5. 提交代码变更
6. 团队成员拉取代码后，如未安装 .NET 10 SDK，global.json 会自动提示

**回滚策略**:
- 如有问题，可通过 git revert 回滚
- 核心项目原本就是 net10.0，回滚只影响 Sample.Jobs 和 Dockerfile

## Open Questions

- [ ] Npgsql.EntityFrameworkCore.PostgreSQL 8.0.4 是否需要升级到 10.x？（待验证）
- [ ] 是否需要更新 README 文档说明 .NET 10 要求？
