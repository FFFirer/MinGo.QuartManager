# Design: Agent 使用场景文档

## Context

### 当前状态

**已有文档**：
- `docs/产品及架构方案.md` - 架构设计
- `docs/quartz-cluster-setup.md` - Quartz 集群配置
- `docs/cluster-management.md` - 集群基础管理
- `docs/api-reference.md` - API 参考

**已有代码**：
- Platform API: `JobsController`, `ClustersController`, `AgentInstancesController`
- Agent: `AgentRegistrationService`, `HeartbeatService`, `QuartzService`
- UI: React + TypeScript + Tailwind (`src/MinGo.Qap.UI/`)

**缺口**：
- 没有完整的 UI 交互场景说明
- 没有端到端操作手册
- 没有环境特定的最佳实践

### 约束

- 文档基于现有实现，不改变代码
- 存放在 `openspec/specs/agent-usage-scenarios/` 进行版本化管理
- 可导出为用户手册

## Goals / Non-Goals

**Goals:**
1. 完整的 Agent 使用场景规范文档
2. UI 交互操作说明（基于现有 React 组件）
3. 端到端操作手册（部署 → 配置 → 运维）
4. 故障处理指南

**Non-Goals:**
- 不实现新功能
- 不修改现有代码
- 不创建独立用户手册网站（只有 Markdown 文档）

## Decisions

### 1. 文档结构

采用 **按场景组织** 而非按功能组织：

```
openspec/specs/agent-usage-scenarios/
├── 01-dev-scenarios.spec.md      # 开发环境
├── 02-prod-single.spec.md     # 生产单实例
├── 03-prod-cluster.spec.md    # 生产集群
├── 04-job-management.spec.md # Job 管理
├── 05-health-monitoring.spec.md # 健康监控
└── 06-operations.spec.md     # 运维操作
```

**Rationale**: 用户更关心"我该怎么做"而非"系统如何工作"。

### 2. UI 交互说明方式

每个场景包含：
- **操作步骤**: 1. 2. 3. ...
- **预期结果**: UI 反馈 + API 调用
- **截图位置**: 可选占位符 `[截图]`

**Rationale**: 现有 UI 是 React 单页应用，操作是直观的。

### 3. 内容深度

每个场景文档 1500-3000 字，覆盖：
- 前置条件
- 配置步骤
- UI 操作流程
- 验证方法
- 常见问题

**Rationale**: 
- 太简略 → 用户不知道怎么操作
- 太详细 → 变成技术手册

## Risks / Trade-offs

### Risk 1: UI 组件可能变化
- **Mitigation**: 文档基于 API 层，不依赖特定 UI 组件
- **Alternative**: 如果 UI 变化大，可以降低 UI 细节

### Risk 2: 某些操作未实现
- **Mitigation**: 区分"已实现"和"计划中"内容
- **Trade-off**: 部分高级场景可能只是设计稿

### Risk 3: 文档维护
- **Mitigation**: 版本化管理在 openspec/
- **Trade-off**: 需要同步更新现有文档

## Migration Plan

1. **Phase 1**: 创建 spec 文件（design → specs）
2. **Phase 2**: 实现各场景文档内容
3. **Phase 3**: 导出到 `docs/` 可选
4. **Phase 4**: 链接到主 README

## Open Questions

1. **Q**: 是否需要包含"快速开始" отдель文档？
   - **A**: 可以作为一个独立的 spec 或主索引

2. **Q**: 如何处理"未实现的 UI"场景？
   - **A**: 标记为 `[计划中]`，不混入已实现内容

3. **Q**: 是否需要 Docker/Docker Compose 使用说明？
   - - **A**: 已在 `docs/quartz-cluster-setup.md` 有部分内容，可引用