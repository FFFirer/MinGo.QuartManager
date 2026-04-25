namespace MinGo.Qap.Shared.Attributes;

/// <summary>
/// 标记复杂对象参数，用于 JSON Schema 生成和 UI 动态表单
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
public sealed class JobPayloadAttribute : Attribute
{
    /// <summary>
    /// 参数描述（用于 UI 展示）
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// 是否必填（默认 true）
    /// </summary>
    public bool Required { get; init; } = true;

    /// <summary>
    /// 显示标签（可选）
    /// </summary>
    public string? Label { get; init; }
}
