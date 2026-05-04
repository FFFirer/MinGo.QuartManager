## Context

系统中有两套 API 需要为 Swagger UI 添加 Header 参数文档：

1. **Platform 端**（NSwag + Controllers）：`AgentsController`、`SchedulersController`、`JobsController`、`ManifestController`
2. **Agent 端**（Minimal API）：`AgentApiExtensions.MapMinGoAgentApi()` 的所有端点

当前仅 `AgentsController` 通过硬编码路径匹配的 `AgentTokenHeaderProcessor` 添加了 `X-Agent-Token`，但该方式不可扩展、不可维护。

约束条件：
- `MinGo.Qap.Shared` 是无 ASP.NET 依赖的纯合约库
- `MinGo.Qap.Agent` 是类库，宿主应用控制 Web 服务器和 Swagger 配置
- Platform 和 Sample.Agent 都已引用 `NSwag.AspNetCore 14.7.1`
- .NET 10.0 目标框架

## Goals / Non-Goals

**Goals:**
- 提供统一的 `[SwaggerHeader]` 特性标注方式，替代硬编码路径匹配
- 为 Platform 所有 Agent 相关 Controller 端点添加对应的 Header 文档
- 为 Agent Minimal API 端点添加 OpenAPI 文档（含 Header 参数）
- 移除旧的 `AgentTokenHeaderProcessor`

**Non-Goals:**
- 不修改现有 API 的行为逻辑或参数校验
- 不引入新的认证/授权机制
- 不修改 Swagger UI 主题或布局
- 不改动前端 UI 代码

## Decisions

### Decision 1: 使用自定义 Attribute + NSwag IOperationProcessor

**方案**: 创建 `SwaggerHeaderAttribute` 标注在 action/local function 上，`SwaggerHeaderProcessor : IOperationProcessor` 读取后动态生成 OpenAPI 参数。

**理由**:
- 声明式标注，一目了然，比硬编码路径匹配更可维护
- Attribute 可复用，Controller 和 Minimal API 使用同一套标注
- NSwag `IOperationProcessor` 能读取 `MethodInfo` 上的属性，对 Controller actions 和 Minimal API 局部函数均有效
- 与项目已用的 NSwag 技术栈一致

**替代方案考虑**:
- `WithOpenApi()` (B1): 仅适用于 Minimal API，无法统一 Controller 端
- 硬编码路径匹配: 现有做法，不可维护，容易遗漏新端点
- Middleware 方案: 运行时拦截，不在 OpenAPI 文档层面，不适合 Swagger UI

### Decision 2: Attribute 放在 Shared 项目中

**理由**:
- Shared 项目已有 `Attributes/` 目录（`JobParameterAttribute` 等），风格一致
- Attribute 是纯元数据，零外部依赖
- Platform 和 Agent 都能引用 Shared，不需要到处重复定义

### Decision 3: Processor 放在 Platform 项目中

**理由**:
- Platform 是主要使用方，已有 `NSwag/` 目录
- Processor 依赖 `NSwag.Generation.Processors` 命名空间，Platform 已引用 NSwag
- Agent 作为类库不应强制依赖 NSwag，通过可选扩展方法提供支持

### Decision 4: Agent Minimal API lambda → 局部函数 + Attribute

**理由**:
- 局部函数可以标注 `[SwaggerHeader]`，NSwag processor 通过 `MethodInfo` 读取
- 不改变功能逻辑，只改变组织方式
- 与 Controller 端使用完全相同的机制

### Decision 5: Agent 提供可选的 OpenApi 扩展

**理由**:
- Agent 类库不应强制宿主绑定特定 OpenAPI 实现
- `AddMinGoAgentOpenApi()` 作为可选扩展方法，宿主按需调用
- 扩展方法内部注册 `SwaggerHeaderProcessor`，处理 Agent 端点的 Header 标注

### Decision 6: Header 按端点实际用途分配

| Header | AgentsController | SchedulersController | JobsController | ManifestController | Agent Minimal API |
|--------|:---:|:---:|:---:|:---:|:---:|
| `X-Agent-Token` | ✅ | ❌ | ❌ | ❌ | ❌ |
| `X-Scheduler-Name` | ❌ | ✅ | ✅ | ✅ | ✅ |

**理由**:
- `X-Agent-Token`：Agent 调用 Platform 时提供身份认证，只用于 AgentsController 的 Register/Delete
- `X-Scheduler-Name`：Platform 转发时设置通知 Agent 使用哪个 Scheduler，用于所有代理到 Agent 的端点和 Agent 自身端点

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| NSwag 在 Minimal API 局部函数上无法读取 `MethodInfo` | 已验证 ASP.NET Core 的 Endpoint Data Source 会捕获局部函数的 MethodInfo，通过 delegate.Method 可访问。NSwag 的 `OperationProcessorContext.MethodInfo` 能正确获取 |
| Agent 端 `NSwag.AspNetCore` 引用增加类库大小 | NSwag 是开发时依赖，发布时会被修剪；且宿主通常已经引用 NSwag |
| 旧 Processor 删除后影响正在运行的 Swagger UI | 新旧 Processor 都注册在 AddOpenApiDocument 的配置阶段，不影响运行时请求处理。只需在下次启动时生效 |
| Minimal API 重构 lambda → 局部函数导致闭包变量捕获变化 | 当前 lambda 都是独立 handler，没有捕获外部变量，重构安全 |
