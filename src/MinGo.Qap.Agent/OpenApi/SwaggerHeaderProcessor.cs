using System.Reflection;
using MinGo.Qap.Shared.Attributes;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace MinGo.Qap.Agent.OpenApi;

/// <summary>
/// 读取 Minimal API 局部函数上的 <see cref="SwaggerHeaderAttribute"/>，
/// 自动将 Header 参数添加到 OpenAPI operation parameters 中。
/// </summary>
public class SwaggerHeaderProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        if (context.MethodInfo is not { } method)
            return true;

        var attributes = method.GetCustomAttributes<SwaggerHeaderAttribute>(inherit: true);
        var operation = context.OperationDescription.Operation;

        foreach (var attr in attributes)
        {
            if (operation.Parameters.Any(p =>
                p.Kind == OpenApiParameterKind.Header &&
                string.Equals(p.Name, attr.Name, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = attr.Name,
                Kind = OpenApiParameterKind.Header,
                IsRequired = attr.Required,
                Description = attr.Description,
                CustomSchema = new NJsonSchema.JsonSchema
                {
                    Type = NJsonSchema.JsonObjectType.String
                }
            });
        }

        return true;
    }
}
