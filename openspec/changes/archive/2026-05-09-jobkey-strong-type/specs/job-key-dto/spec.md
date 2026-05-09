## ADDED Requirements

### Requirement: JobKeyDto 值类型定义
系统 SHALL 定义 `JobKeyDto` 为 `readonly record struct`，包含以下字段：
- `Name: string` — 必填，Job 名称
- `Group: string` — 可选，默认值为 `"DEFAULT"`

`JobKeyDto` SHALL 实现以下行为：
- `ToString()` 返回 `"Group.Name"` 格式字符串（仅用于显示/日志）
- 值相等性：两个 `JobKeyDto` 当且仅当 Name 和 Group 都相等时相等
- JSON 序列化为 `{"name":"...", "group":"..."}` 结构化对象

#### Scenario: 创建带自定义 Group 的 JobKeyDto
- **WHEN** 代码 `new JobKeyDto("myJob", "MyGroup")`
- **THEN** `Name == "myJob"` 且 `Group == "MyGroup"` 且 `ToString() == "MyGroup.myJob"`

#### Scenario: 创建使用默认 Group 的 JobKeyDto
- **WHEN** 代码 `new JobKeyDto("myJob")`
- **THEN** `Group == "DEFAULT"` 且 `ToString() == "DEFAULT.myJob"`

#### Scenario: JobKeyDto JSON 序列化
- **WHEN** `JsonSerializer.Serialize(new JobKeyDto("myJob", "MyGroup"))`
- **THEN** 结果字符串为 `{"name":"myJob","group":"MyGroup"}`

### Requirement: Shared DTO 使用 JobKeyDto
所有相关 DTO SHALL 将 `string JobKey` 或 `string JobKey` + `string Group` 替换为 `JobKeyDto JobKey`。

涉及的 DTO：
- `CreateJobRequest.JobKey: string` → `JobKeyDto`
- `JobDefinitionDto.JobKey: string` → `JobKeyDto`（移除独立的 `Group` 字段）
- `JobSummaryDto.JobKey: string` → `JobKeyDto`（移除独立的 `Group` 字段）
- `JobDetailDto.JobKey: string` → `JobKeyDto`（移除独立的 `Group` 字段）
- `ExecutionLogDto.JobKey: string` → `JobKeyDto`

#### Scenario: CreateJobRequest 使用 JobKeyDto 创建
- **WHEN** 创建 `CreateJobRequest` 时指定 `JobKey = new JobKeyDto("myJob", "MyGroup")`
- **THEN** JSON 序列化后请求体包含 `"jobKey":{"name":"myJob","group":"MyGroup"}`

#### Scenario: JobSummaryDto 携带 JobKeyDto 信息
- **WHEN** 前端收到 `JobSummaryDto`
- **THEN** `jobKey.name` 和 `jobKey.group` 可分别用于展示

### Requirement: 不保留 string 类型 JobKey
系统 SHALL 在所有 API 请求/响应中移除 `string JobKey` 字段，仅使用 `JobKeyDto`。

API 边界 SHALL 不再出现 `"Group.Name"` 格式的字符串作为标识符传输。

DB 实体 `JobDefinition` SHALL 添加 `Name` 列来存储 Job 名称，原有 `JobKey` 列保留为过渡期兼容（代码不再写入新数据）。

#### Scenario: API 创建 Job 请求体不含 string JobKey
- **WHEN** 前端发送 POST `/api/schedulers/sch1/jobs` 请求
- **THEN** 请求体包含 `"jobKey":{"name":"myJob","group":"DEFAULT"}` 而非 `"jobKey":"DEFAULT.myJob"`

#### Scenario: 现有 DB 数据迁移
- **WHEN** 执行 DB 迁移
- **THEN** `JobDefinition.Name` 列从 `JobKey` 列解析回填：`SPLIT_PART(JobKey, '.', 2)`
- **AND** 新增唯一索引 `(SchedulerName, Group, Name)`

### Requirement: 前端 TypeScript JobKeyDto 接口
前端 SHALL 定义对应的 TypeScript 接口：

```typescript
interface JobKeyDto {
  name: string;
  group: string;
}
```

前端 SHALL 删除 `parseJobKey()` 函数，不再需要从字符串解析 group/name。

前端 SHALL 使用 `JobKeyDto` 对象传递数据，不在 API 边界拼接字符串。

#### Scenario: TypeScript 接口可用
- **WHEN** 从 API 接收到 `JobKeyDto`
- **THEN** `jobKey.name` 和 `jobKey.group` 可直接访问

#### Scenario: CreateJobPage 提交 JobKeyDto
- **WHEN** 用户填写 Group="MyGroup" 和 Name="myJob"
- **THEN** 提交的 `CreateJobRequest.jobKey` 为 `{name: "myJob", group: "MyGroup"}` 而非 `"MyGroup.myJob"`
