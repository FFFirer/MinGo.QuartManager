## 1. Shared: SwaggerHeaderAttribute

- [x] 1.1 Create `src/MinGo.Qap.Shared/Attributes/SwaggerHeaderAttribute.cs` with Name, Description, Required properties (AllowMultiple = true, targets Method | Class)

## 2. Platform: SwaggerHeaderProcessor

- [x] 2.1 Create `src/MinGo.Qap.Platform/NSwag/SwaggerHeaderProcessor.cs` implementing IOperationProcessor — reads `[SwaggerHeader]` from MethodInfo and ControllerType, deduplicates, adds to operation.Parameters
- [x] 2.2 Update `src/MinGo.Qap.Platform/Program.cs` — replace `AgentTokenHeaderProcessor` registration with `SwaggerHeaderProcessor`
- [x] 2.3 Delete `src/MinGo.Qap.Platform/NSwag/AgentTokenHeaderProcessor.cs`

## 3. Platform: Controller Annotations

- [x] 3.1 Add `[SwaggerHeader("X-Agent-Token", ...)]` to `AgentsController.Register` and `AgentsController.Delete`
- [x] 3.2 Add `[SwaggerHeader("X-Scheduler-Name", ...)]` to `SchedulersController.GetList` and `SchedulersController.GetAgents`
- [x] 3.3 Add `[SwaggerHeader("X-Scheduler-Name", ...)]` to all `JobsController` action methods (GetList, Create, Update, Delete, Trigger, Pause, Resume)
- [x] 3.4 Add `[SwaggerHeader("X-Scheduler-Name", ...)]` to `ManifestController.Post` and `ManifestController.Get`

## 4. Agent: Minimal API Refactor + Annotation

- [x] 4.1 Add `NSwag.AspNetCore` package reference to `src/MinGo.Qap.Agent/MinGo.Qap.Agent.csproj`
- [x] 4.2 Refactor `AgentApiExtensions.cs` lambda handlers to named local functions with `[SwaggerHeader("X-Scheduler-Name", ...)]` for all endpoints
- [x] 4.3 Create `src/MinGo.Qap.Agent/OpenApi/AgentOpenApiExtensions.cs` with `AddMinGoAgentOpenApi()` extension method registering `SwaggerHeaderProcessor`

## 5. Sample: Host Application Update

- [x] 5.1 Update `samples/Sample.Agent/Program.cs` — call `builder.Services.AddMinGoAgentOpenApi()`
