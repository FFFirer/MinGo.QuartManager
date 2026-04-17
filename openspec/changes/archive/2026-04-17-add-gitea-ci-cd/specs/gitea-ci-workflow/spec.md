## ADDED Requirements

### Requirement: CI 工作流自动触发
Gitea Actions CI 工作流 SHALL 在代码推送到任意分支时自动触发执行。

#### Scenario: Push 触发 CI
- **WHEN** 开发者推送代码到任意 Git 分支
- **THEN** Gitea Actions 自动触发 CI 工作流

#### Scenario: PR 创建触发 CI
- **WHEN** 创建或更新 Pull Request
- **THEN** Gitea Actions 自动触发 CI 工作流

### Requirement: 代码格式检查
CI 工作流 SHALL 执行代码格式检查，确保代码符合项目规范。

#### Scenario: 格式检查通过
- **WHEN** 代码格式符合规范
- **THEN** 格式检查步骤通过

#### Scenario: 格式检查失败
- **WHEN** 代码格式不符合规范
- **THEN** CI 工作流失败，报告格式问题

### Requirement: 项目构建
CI 工作流 SHALL 执行 dotnet build，确保代码能够成功编译。

#### Scenario: 构建成功
- **WHEN** 代码编译成功无错误
- **THEN** 构建步骤通过

#### Scenario: 构建失败
- **WHEN** 代码存在编译错误
- **THEN** CI 工作流失败，显示编译错误详情

### Requirement: 单元测试执行
CI 工作流 SHALL 执行 dotnet test，运行所有单元测试。

#### Scenario: 所有测试通过
- **WHEN** 所有单元测试执行成功
- **THEN** 测试步骤通过

#### Scenario: 测试失败
- **WHEN** 任意单元测试失败
- **THEN** CI 工作流失败，显示失败测试详情