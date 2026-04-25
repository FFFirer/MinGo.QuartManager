namespace MinGo.Qap.Shared.Attributes;

/// <summary>
/// 标记 Job 参数，用于参数元数据自动发现和 UI 动态生成
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
public sealed class JobParameterAttribute : Attribute
{
    /// <summary>
    /// 参数名称（唯一标识）
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 参数描述（用于 UI 展示）
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// 是否必填（默认 true）
    /// </summary>
    public bool Required { get; init; } = true;

    /// <summary>
    /// 默认值（可选）
    /// </summary>
    public object? DefaultValue { get; init; }

    /// <summary>
    /// 验证正则表达式（可选）
    /// </summary>
    public string? ValidationRegex { get; init; }

    /// <summary>
    /// 显示标签（可选，默认使用 Name）
    /// </summary>
    public string? Label { get; init; }

    public JobParameterAttribute(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}
