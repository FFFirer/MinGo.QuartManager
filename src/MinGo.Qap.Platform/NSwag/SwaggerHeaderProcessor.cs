using System.Reflection;
using MinGo.Qap.Shared.Attributes;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace MinGo.Qap.Platform.NSwag;

/// <summary>
/// 读取端点方法/类上的 <see cref="SwaggerHeaderAttribute"/>，
/// 自动将 Header 参数添加到 OpenAPI operation 的 parameters 中。
/// 支持 Controller action 方法和 Minimal API 局部函数。
/// </summary>
public class SwaggerHeaderProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        var attributes = CollectAttributes(context);
        if (attributes.Count == 0)
            return true;

        var operation = context.OperationDescription.Operation;

        foreach (var attr in attributes)
        {
            // 避免重复添加同名 header
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

    private static List<SwaggerHeaderAttribute> CollectAttributes(OperationProcessorContext context)
    {
        var result = new List<SwaggerHeaderAttribute>();

        // 1. 从方法级别读取（Controller action / Minimal API local function）
        if (context.MethodInfo is { } method)
        {
            result.AddRange(method.GetCustomAttributes<SwaggerHeaderAttribute>(inherit: true));
        }

        // 2. 从类级别读取（Controller 类上的全局标注）
        if (context.ControllerType is { } controllerType)
        {
            result.AddRange(controllerType.GetCustomAttributes<SwaggerHeaderAttribute>(inherit: true));
        }

        return result;
    }
}
