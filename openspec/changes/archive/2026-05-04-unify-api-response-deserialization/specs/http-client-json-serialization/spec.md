## MODIFIED Requirements

### Requirement: 向后兼容

此变更 SHALL 不改变 DTO 定义。

#### Scenario: DTO 属性映射正确
- **WHEN** Agent 发送以 CamelCase 序列化的 DTO 到 Platform
- **THEN** Platform 端以 CamelCase 反序列化，属性值正确匹配
- **AND** 反之亦然

#### Scenario: ApiResponse 解包后 DTO 属性正确映射（新）
- **WHEN** 接收方使用 `ReadFromApiResponseAsync<T>` 解包 `ApiResponse<T>` 响应
- **THEN** `.Data` 中的 JSON 属性正确映射到类型 `T` 的属性
- **AND** `Success`、`ErrorMessage` 等包装字段不会出现在 `T` 的实例中

## ADDED Requirements

### Requirement: 响应解包使用 ReadFromApiResponseAsync

所有期望 HTTP 响应体是 `ApiResponse<T>` 格式的代码 SHALL 使用 `ReadFromApiResponseAsync<T>` 方法进行解包，而非直接使用 `ReadFromJsonAsync<T>`。

#### Scenario: Direction B 使用 ReadFromApiResponseAsync
- **WHEN** `AgentProxyService.GetAsync<T>`、`PostAsync<T>`、`PutAsync<T>` 解析成功响应
- **THEN** 使用 `ReadFromApiResponseAsync<T>` 解包 `.Data`
- **AND** `HandleResponse<T>` 方法的返回值类型保持 `T?` 不变

#### Scenario: Direction C 使用 ReadFromApiResponseAsync
- **WHEN** Agent 端 `AgentRegistrationService` 或 `HostedAgentService` 读取 Platform 响应
- **THEN** 使用 `ReadFromApiResponseAsync<T>` 而非 `ReadFromJsonAsync<T>`
