## Context

前端项目已有一个基本的集群列表页面 (ClustersPage)，显示集群卡片但没有创建功能。后端 ClustersController 已实现完整的 CRUD API。

现有组件:
- ClustersPage: 显示集群列表，有 "Add Cluster" 按钮但无点击事件
- Agent 实例列表已有入口 `/clusters/{id}/agents`

技术栈: React + TypeScript + React Router + Axios

## Goals / Non-Goals

**Goals:**
- 实现 CreateClusterModal 模态框组件
- 实现集群创建功能 (POST /api/clusters)
- 实现 Add Cluster 按钮的事件绑定
- 实现 ClusterDetailPage 详情页面

**Non-Goals:**
- 不修改后端 API
- 不添加认证相关功能
- 不实现集群编辑功能（只查看）

## Decisions

1. **使用模态框而非独立页面创建集群**
   - 理由：用户交互更流畅，与现有页面风格一致
   - 替代方案：独立创建页面 → 需更多路由配置

2. **复用现有 useClusters hook**
   - 理由：已有的数据获取逻辑可复用
   - 创建 useCreateCluster hook 处理 POST 请求

3. **沿用现有表单组件风格**
   - 理由：保持 UI 一致性，参考 CreateJobModal 样式

## Risks / Trade-offs

- [风险] API 请求失败处理
  - 解决方案：在 Modal 中显示错误信息，支持重试

- [风险] 环境字段验证
  - 解决方案：下拉选择预定义环境值 (dev, staging, prod)