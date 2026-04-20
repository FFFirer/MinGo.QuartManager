## Why

当前前端已有 Cluster 列表页面，但缺少集群的创建表单模态框和查看/编辑详情功能。用户无法通过 UI 创建新集群，只能通过 API 操作。同时需要完善 Agent 实例的管理界面。

## What Changes

- 添加 Create Cluster 模态框组件（名称、环境、描述表单）
- 完善 Add Cluster 按钮的点击事件处理
- 添加集群详情页面（查看和编辑集群基本信息）
- Agent 实例列表页面已存在入口，需验证功能完整性

## Capabilities

### New Capabilities

- **cluster-create**: 前端创建集群表单模态框
- **cluster-details**: 集群详情查看和编辑页面

### Modified Capabilities

- 无（现有功能无需修改规格）

## Impact

- 影响的代码：`src/MinGo.Qap.UI/src/pages/`, `src/components/`
- 新增组件：CreateClusterModal, ClusterDetailPage
- 依赖后端 API：POST /api/clusters, GET /api/clusters/{clusterId}