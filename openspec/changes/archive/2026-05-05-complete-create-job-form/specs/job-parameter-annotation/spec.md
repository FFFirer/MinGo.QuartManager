## ADDED Requirements

### Requirement: EchoJob 标注 message 参数
`EchoJob` SHALL 在 `Message` 属性上标注 `[JobParameter("message")]` 特性，使 Agent 能自动发现此参数元数据。

#### Scenario: Agent 发现 EchoJob 参数
- **WHEN** JobDiscoveryService 扫描 EchoJob 类型
- **THEN** 发现名称为 "message"、类型为 "string"、非必填的参数
- **WHEN** manifest 返回给前端
- **THEN** EchoJob 的 parameters 列表中包含 message 参数定义

### Requirement: DelayJob 标注 delaySeconds 参数
`DelayJob` SHALL 在 `DelaySeconds` 属性上标注 `[JobParameter("delaySeconds")]` 特性，默认值为 5，使 Agent 能自动发现此参数元数据。

#### Scenario: Agent 发现 DelayJob 参数
- **WHEN** JobDiscoveryService 扫描 DelayJob 类型
- **THEN** 发现名称为 "delaySeconds"、类型为 "int"、非必填、默认值为 5 的参数
- **WHEN** manifest 返回给前端
- **THEN** DelayJob 的 parameters 列表中包含 delaySeconds 参数定义

### Requirement: 表单 Step 2 根据参数元数据渲染输入控件
CreateJob 表单 Step 2 SHALL 根据 manifest 返回的 `parameters` 列表，为每个参数渲染对应的输入控件。

#### Scenario: string 类型参数
- **WHEN** 参数 `type` 为 "string"
- **THEN** 渲染文本输入框 (input type="text")
- **THEN** 如果参数有 `default` 值，预填为默认值
- **THEN** 如果 `required` 为 true，标签旁显示红色 *

#### Scenario: int 类型参数
- **WHEN** 参数 `type` 为 "int"
- **THEN** 渲染数字输入框 (input type="number")
- **THEN** 如果参数有 `default` 值，预填为默认值

#### Scenario: bool 类型参数
- **WHEN** 参数 `type` 为 "bool"
- **THEN** 渲染下拉选择框 (True/False)
- **THEN** 如果参数有 `default` 值，选中默认值

### Requirement: CreateJobPanel 宽度响应式
CreateJobPanel SHALL 在宽屏下使用 `max-w-2xl`(672px) 宽度。

#### Scenario: 宽屏显示
- **WHEN** 视口宽度 >= 1024px
- **THEN** SlidePanel 宽度为 `max-w-2xl` (672px)
- **THEN** Step 2 参数配置区域和 Step 3 调度配置区域有足够空间展示

#### Scenario: 小屏显示
- **WHEN** 视口宽度 < 1024px
- **THEN** SlidePanel 宽度为 `w-full` (占满视口)

### Requirement: CreateJobModal 同步宽度
CreateJobModal SHALL 与 CreateJobPanel 保持一致的宽度表现。

#### Scenario: Modal 宽度
- **WHEN** Modal 在宽屏下打开
- **THEN** 宽度为 `max-w-2xl` (672px)
