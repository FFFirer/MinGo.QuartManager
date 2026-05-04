using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace MinGo.Qap.Platform.NSwag;

/// <summary>
/// 为 Agent 管理相关 API 端点添加 X-Agent-Token Header 参数，
/// 使其在 Swagger UI 中可见并可传入。
/// </summary>
public class AgentTokenHeaderProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        var path = context.OperationDescription.Path;

        // 只处理 /api/agents 路径下的端点
        if (!path.StartsWith("/api/agents", StringComparison.OrdinalIgnoreCase))
            return true;

        var operation = context.OperationDescription.Operation;

        // 避免重复添加
        if (operation.Parameters.Any(p =>
            p.Kind == OpenApiParameterKind.Header &&
            string.Equals(p.Name, "X-Agent-Token", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Agent-Token",
            Kind = OpenApiParameterKind.Header,
            IsRequired = false,
            Description = "Agent 身份认证 Token。由 Agent 配置中的 platform.apiToken 提供，用于注册、注销及上报 Scheduler 信息。",
            CustomSchema = new NJsonSchema.JsonSchema
            {
                Type = NJsonSchema.JsonObjectType.String
            }
        });

        return true;
    }
}
