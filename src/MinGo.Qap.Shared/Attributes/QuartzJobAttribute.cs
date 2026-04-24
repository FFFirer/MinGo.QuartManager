namespace MinGo.Qap.Shared.Attributes;

/// <summary>
/// Quartz Job 标记属性
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class QuartzJobAttribute : Attribute
{
    /// <summary>
    /// 作业组
    /// </summary>
    public string Group { get; }

    /// <summary>
    /// 作业描述
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// 是否持久化作业（即使没有触发器）
    /// </summary>
    public bool Durable { get; init; }

    /// <summary>
    /// 是否请求恢复
    /// </summary>
    public bool RequestRecovery { get; init; }

    /// <summary>
    /// 作业并发执行策略
    /// </summary>
    public JobConcurrencyPolicy ConcurrencyPolicy { get; init; } = JobConcurrencyPolicy.Allow;

    public QuartzJobAttribute(string group, string? description = null)
    {
        Group = group ?? throw new ArgumentNullException(nameof(group));
        Description = description;
    }
}

/// <summary>
/// 作业并发策略
/// </summary>
public enum JobConcurrencyPolicy
{
    /// <summary>
    /// 允许并发执行
    /// </summary>
    Allow = 1,

    /// <summary>
    /// 禁止并发执行
    /// </summary>
    Forbid = 2,

    /// <summary>
    /// 替换当前执行
    /// </summary>
    Replace = 3
}