## Purpose

定义 Agent 项目与 Platform 项目之间 HTTP 通信的命名 HttpClient 使用规范以及 JSON 序列化/反序列化的命名策略，确保双方通信的一致性和可维护性。

## Requirements

### Requirement: Agent 项目使用命名 HttpClient 与 Platform 通信

Agent 项目 SHALL 注册名为 "PlatformApi" 的命名 HttpClient，覆盖所有向 Platform 发起的 HTTP 请求。

#### Scenario: 注册命名 HttpClient "PlatformApi"
- **WHEN** Agent 服务启动（`AddMinGoAgent` 调用）
- **THEN** 注册名为 "PlatformApi" 的命名 HttpClient
- **AND** 配置 `BaseAddress` 为 `AgentConfig.Platform.Url`
- **AND** 配置默认请求头 `X-Agent-Token`
- **AND** 配置超时为 30 秒

#### Scenario: 所有 Agent 服务使用命名 HttpClient
- **WHEN** `AgentRegistrationService`、`HostedAgentService`、`SchedulerReporterService`、`LogCollectionService` 发起 HTTP 请求
- **THEN** 使用 `IHttpClientFactory.CreateClient("PlatformApi")`，而非未命名方式

### Requirement: Platform 项目使用命名 HttpClient 与 Agent 通信

Platform 项目 SHALL 注册名为 "AgentApi" 的命名 HttpClient，覆盖所有向 Agent 转发的 HTTP 请求。

#### Scenario: 注册命名 HttpClient "AgentApi"
- **WHEN** Platform 服务启动（Program.cs）
- **THEN** 注册名为 "AgentApi" 的命名 HttpClient
- **AND** 配置超时为 30 秒
- **AND** 不配置固定 BaseAddress（运行时从路由服务动态获取）

#### Scenario: AgentProxyService 使用命名 HttpClient
- **WHEN** `AgentProxyService` 发起对 Agent 的 HTTP 请求
- **THEN** 使用 `IHttpClientFactory.CreateClient("AgentApi")`
- **AND** 在每个请求前通过 `PickAgentForSchedulerAsync` 动态设置请求目标 URL

### Requirement: HTTP JSON 序列化/反序列化使用 CamelCase

Agent 和 Platform 之间的所有 HTTP 通信 SHALL 使用 CamelCase 命名策略进行 JSON 序列化和反序列化。

#### Scenario: PostAsJsonAsync 使用 CamelCase
- **WHEN** Agent 或 Platform 调用 `PostAsJsonAsync` 发送请求体
- **THEN** JSON 属性名使用 CamelCase 格式

#### Scenario: ReadFromJsonAsync 使用 CamelCase
- **WHEN** Agent 或 Platform 调用 `ReadFromJsonAsync<T>` 读取响应
- **THEN** JSON 属性名以 CamelCase 格式解析

#### Scenario: JsonContent.Create 使用 CamelCase
- **WHEN** 使用 `JsonContent.Create(body)` 创建请求内容
- **THEN** JSON 属性名使用 CamelCase 格式

### Requirement: 向后兼容

此变更 SHALL 不改变 DTO 定义，仅改变序列化配置。

#### Scenario: DTO 属性映射正确
- **WHEN** Agent 发送以 CamelCase 序列化的 DTO 到 Platform
- **THEN** Platform 端以 CamelCase 反序列化，属性值正确匹配
- **AND** 反之亦然

#### Scenario: ApiResponse 解包后 DTO 属性正确映射
- **WHEN** 接收方使用 `ReadFromApiResponseAsync<T>` 解包 `ApiResponse<T>` 响应
- **THEN** `.Data` 中的 JSON 属性正确映射到类型 `T` 的属性
- **AND** `Success`、`ErrorMessage` 等包装字段不会出现在 `T` 的实例中

### Requirement: 响应解包使用 ReadFromApiResponseAsync

所有期望 HTTP 响应体是 `ApiResponse<T>` 格式的代码 SHALL 使用 `ReadFromApiResponseAsync<T>` 方法进行解包，而非直接使用 `ReadFromJsonAsync<T>`。

#### Scenario: Direction B 使用 ReadFromApiResponseAsync
- **WHEN** `AgentProxyService.GetAsync<T>`、`PostAsync<T>`、`PutAsync<T>` 解析成功响应
- **THEN** 使用 `ReadFromApiResponseAsync<T>` 解包 `.Data`
- **AND** `HandleResponse<T>` 方法的返回值类型保持 `T?` 不变

#### Scenario: Direction C 使用 ReadFromApiResponseAsync
- **WHEN** Agent 端 `AgentRegistrationService` 或 `HostedAgentService` 读取 Platform 响应
- **THEN** 使用 `ReadFromApiResponseAsync<T>` 而非 `ReadFromJsonAsync<T>`
