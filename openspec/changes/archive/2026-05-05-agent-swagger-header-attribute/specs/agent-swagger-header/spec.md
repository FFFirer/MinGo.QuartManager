## ADDED Requirements

### Requirement: Endpoints can declare Swagger Header parameters via attribute

The system SHALL provide a `SwaggerHeaderAttribute` that can be applied to controller action methods and Minimal API handler functions to declare Header parameters for Swagger UI documentation.

- The attribute SHALL be placed in the `MinGo.Qap.Shared` project under `Attributes/` directory
- The attribute SHALL support `AllowMultiple = true` so multiple headers can be declared on a single endpoint
- The attribute SHALL accept a `Name` (string) and `Description` (string) as required constructor parameters
- The attribute SHALL support an optional `Required` boolean property (default: `false`)
- The attribute SHALL have zero external dependencies

#### Scenario: Attribute is applied to a controller action
- **WHEN** a developer adds `[SwaggerHeader("X-Agent-Token", "Token description")]` to a controller action method
- **THEN** the NSwag IOperationProcessor SHALL detect this attribute and add a corresponding Header parameter to the OpenAPI operation

#### Scenario: Multiple headers on a single endpoint
- **WHEN** a developer adds both `[SwaggerHeader("X-Agent-Token", ...)]` and `[SwaggerHeader("X-Scheduler-Name", ...)]` to the same endpoint
- **THEN** both Header parameters SHALL appear in the Swagger UI for that endpoint

### Requirement: IOperationProcessor reads SwaggerHeader attributes and adds OpenAPI parameters

The system SHALL provide a `SwaggerHeaderProcessor : IOperationProcessor` that reads `[SwaggerHeader]` attributes from endpoint methods and adds corresponding Header parameters to the OpenAPI operation definition.

- The processor SHALL check `context.MethodInfo.GetCustomAttributes<SwaggerHeaderAttribute>()` for method-level attributes
- The processor SHALL check `context.ControllerType.GetCustomAttributes<SwaggerHeaderAttribute>()` for class-level attributes (for controllers)
- The processor SHALL deduplicate parameters to avoid adding the same header twice
- The processor SHALL set `IsRequired = false` (header is documented but not enforced by OpenAPI schema)
- The processor SHALL NOT modify the operation if no `[SwaggerHeader]` attributes are found

#### Scenario: Processor adds header for controller action
- **WHEN** `AgentsController.Register` is decorated with `[SwaggerHeader("X-Agent-Token", ...)]`
- **THEN** the Swagger UI for `POST /api/agents` SHALL show an `X-Agent-Token` header parameter

#### Scenario: No attribute, no change
- **WHEN** an endpoint has no `[SwaggerHeader]` attribute
- **THEN** the processor SHALL NOT add any header parameters to that endpoint

### Requirement: Platform Agent controllers declare correct headers

The following controllers SHALL have `[SwaggerHeader]` attributes on their action methods:

| Controller | Action | Header |
|---|---|---|
| `AgentsController` | `Register` | `X-Agent-Token` |
| `AgentsController` | `Delete` | `X-Agent-Token` |
| `SchedulersController` | `GetList` | `X-Scheduler-Name` |
| `SchedulersController` | `GetAgents` | `X-Scheduler-Name` |
| `JobsController` | `GetList` | `X-Scheduler-Name` |
| `JobsController` | `Create` | `X-Scheduler-Name` |
| `JobsController` | `Update` | `X-Scheduler-Name` |
| `JobsController` | `Delete` | `X-Scheduler-Name` |
| `JobsController` | `Trigger` | `X-Scheduler-Name` |
| `JobsController` | `Pause` | `X-Scheduler-Name` |
| `JobsController` | `Resume` | `X-Scheduler-Name` |
| `ManifestController` | `Post` | `X-Scheduler-Name` |
| `ManifestController` | `Get` | `X-Scheduler-Name` |

- The `X-Agent-Token` header description SHALL be "Agent 身份认证 Token。由 Agent 配置中的 platform.apiToken 提供。"
- The `X-Scheduler-Name` header description SHALL be "Scheduler 名称。Platform 转发请求到 Agent 时指定目标 Scheduler。"

#### Scenario: AgentsController has X-Agent-Token
- **WHEN** user views `POST /api/agents` in Swagger UI
- **THEN** the endpoint SHALL display an `X-Agent-Token` Header parameter

#### Scenario: SchedulersController has X-Scheduler-Name
- **WHEN** user views `GET /api/schedulers` in Swagger UI
- **THEN** the endpoint SHALL display an `X-Scheduler-Name` Header parameter

#### Scenario: JobsController has X-Scheduler-Name
- **WHEN** user views `POST /api/schedulers/{name}/jobs` in Swagger UI
- **THEN** the endpoint SHALL display an `X-Scheduler-Name` Header parameter

### Requirement: Agent Minimal API endpoints declare X-Scheduler-Name

The `AgentApiExtensions.MapMinGoAgentApi()` SHALL declare `X-Scheduler-Name` Header parameter on all its endpoints via `[SwaggerHeader]` attribute on local functions.

- All endpoints under `/api/agent/*` SHALL have `[SwaggerHeader("X-Scheduler-Name", "Scheduler 名称。由 Platform 转发时设置。")]`
- Lambda expressions SHALL be refactored to named local functions to allow attribute decoration
- Endpoints SHALL retain `WithName()` for OpenAPI operation naming

#### Scenario: Agent jobs endpoint has header
- **WHEN** user views `GET /api/agent/jobs` in Swagger UI
- **THEN** the endpoint SHALL display an `X-Scheduler-Name` Header parameter

### Requirement: Old AgentTokenHeaderProcessor is removed

The `AgentTokenHeaderProcessor` SHALL be removed from the codebase after the new processor and attributes are in place.

- The file `src/MinGo.Qap.Platform/NSwag/AgentTokenHeaderProcessor.cs` SHALL be deleted
- The registration line `config.OperationProcessors.Add(new AgentTokenHeaderProcessor())` in `Program.cs` SHALL be replaced with the new processor

#### Scenario: Old processor not in use
- **WHEN** the Platform project is built and Swagger UI is opened
- **THEN** there SHALL be no reference to `AgentTokenHeaderProcessor` in the codebase

### Requirement: Agent project provides optional OpenApi extension

The Agent project SHALL provide an extension method `AddMinGoAgentOpenApi()` that registers the `SwaggerHeaderProcessor` for host applications.

- The extension method SHALL be in `MinGo.Qap.Agent.OpenApi` namespace
- The extension method SHALL call `services.AddOpenApiDocument()` with the processor registered
- The `MinGo.Qap.Agent.csproj` SHALL reference `NSwag.AspNetCore` (version aligned with Platform's usage)
- Host applications (e.g., `Sample.Agent`) SHALL call `builder.Services.AddMinGoAgentOpenApi()` to enable OpenAPI documentation for Agent endpoints

#### Scenario: Host app enables Agent OpenAPI
- **WHEN** `Sample.Agent` Program.cs calls `builder.Services.AddMinGoAgentOpenApi()`
- **THEN** the Agent's `/api/agent/*` endpoints SHALL appear in the Swagger UI with `X-Scheduler-Name` header
