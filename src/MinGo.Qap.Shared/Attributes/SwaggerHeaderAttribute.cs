namespace MinGo.Qap.Shared.Attributes;

/// <summary>
/// 标记 API 端点需要在 Swagger UI 中展示的 Header 参数。
/// 由 <c>SwaggerHeaderProcessor</c> 读取并添加到 OpenAPI operation parameters。
/// </summary>
/// <remarks>
/// 可应用于 Controller action 方法（MVC）或 Minimal API 处理函数/局部函数。
/// 支持在同一端点上标注多个不同的 Header。
/// </remarks>
/// <param name="name">Header 名称，如 "X-Agent-Token"</param>
/// <param name="description">Header 用途说明（显示在 Swagger UI 中）</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class SwaggerHeaderAttribute(string name, string description) : Attribute
{
    /// <summary>Header 名称</summary>
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

    /// <summary>Header 用途说明</summary>
    public string Description { get; } = description ?? throw new ArgumentNullException(nameof(description));

    /// <summary>是否必填（默认 false，仅作为文档提示，不强制校验）</summary>
    public bool Required { get; init; }
}
