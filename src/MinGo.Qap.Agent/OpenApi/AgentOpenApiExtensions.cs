namespace MinGo.Qap.Agent.OpenApi;

/// <summary>
/// MinGo Agent OpenAPI 配置扩展
/// </summary>
public static class AgentOpenApiExtensions
{
    /// <summary>
    /// 为 MinGo Agent 的 Minimal API 端点添加 OpenAPI 文档支持。
    /// 注册 <see cref="SwaggerHeaderProcessor"/> 使 <c>[SwaggerHeader]</c> 特性
    /// 在 Swagger UI 中可见。
    /// </summary>
    public static IServiceCollection AddMinGoAgentOpenApi(this IServiceCollection services)
    {
        services.AddOpenApiDocument(config =>
        {
            config.OperationProcessors.Add(new SwaggerHeaderProcessor());
        });

        return services;
    }
}
