## ADDED Requirements

### Requirement: 用户可以查看集群详情
用户可以通过集群列表进入集群详情页面，查看详细信息。

#### Scenario: 查看集群详情
- **WHEN** 用户在集群卡片上点击集群名称或详情链接
- **THEN** 导航到集群详情页面 /clusters/:clusterId
- **AND** 显示集群的基本信息 (名称、环境、状态、创建时间)

#### Scenario: 查看集群关联的 Agent 实例
- **WHEN** 用户在详情页面点击 "View Agents" 或查看 Agent 标签页
- **THEN** 显示该集群下的所有 Agent 实例列表
- **AND** 每个实例显示 URL、状态、心跳时间

### Requirement: 用户可以删除集群
用户可以从详情页面删除集群。

#### Scenario: 删除集群
- **WHEN** 用户点击 "Delete" 按钮并确认
- **THEN** 发送 DELETE /api/clusters/:clusterId 请求
- **AND** 成功后导航回集群列表
- **AND** 显示成功提示