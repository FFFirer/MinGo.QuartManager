namespace MinGo.Qap.Shared.Models;

/// <summary>
/// Job 类型信息
/// </summary>
public class JobTypeInfoDto
{
    /// <summary>
    /// Job 类型 Key（唯一标识）
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Job 类型完整名称（用于反射创建实例）
    /// </summary>
    public string JobTypeFullName { get; set; } = string.Empty;

    /// <summary>
    /// Job 描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 参数定义列表
    /// </summary>
    public List<ParameterInfoDto> Parameters { get; set; } = new();
}

/// <summary>
/// 参数信息
/// </summary>
public class ParameterInfoDto
{
    /// <summary>
    /// 参数名
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 参数类型: string, int, bool, datetime
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否必填
    /// </summary>
    public bool Required { get; set; }
    
    /// <summary>
    /// 默认值
    /// </summary>
    public object? Default { get; set; }
    
    /// <summary>
    /// 显示标签
    /// </summary>
    public string? Label { get; set; }
}

/// <summary>
/// Job Manifest（Agent 上报的可用 Job 列表）
/// </summary>
public class JobManifestDto
{
    public List<JobTypeInfoDto> Jobs { get; set; } = new();
}
