## ADDED Requirements

### Requirement: 用户可以打开集群创建模态框
当用户点击 "Add Cluster" 按钮时，系统 MUST 显示创建集群的模态框。

#### Scenario: 点击添加按钮
- **WHEN** 用户点击 ClustersPage 上的 "Add Cluster" 按钮
- **THEN** 显示 CreateClusterModal 模态框，包含名称和环境输入字段

### Requirement: 用户可以创建新集群
用户填写表单后提交，系统调用后端 API 创建集群。

#### Scenario: 成功创建集群
- **WHEN** 用户填写名称 (必填) 和环境 (必填)，点击 "Create" 按钮
- **THEN** 发送 POST /api/clusters 请求
- **AND** 成功后关闭模态框并刷新集群列表
- **AND** 显示成功提示

#### Scenario: 名称为空
- **WHEN** 用户不填写名称直接点击 "Create"
- **THEN** 显示验证错误 "Name is required"
- **AND** 不提交表单

#### Scenario: 环境为空
- **WHEN** 用户不选择环境直接点击 "Create"
- **THEN** 显示验证错误 "Environment is required"
- **AND** 不提交表单

#### Scenario: API 请求失败
- **WHEN** 后端返回错误
- **THEN** 在模态框中显示错误信息
- **AND** 用户可以修改后重试