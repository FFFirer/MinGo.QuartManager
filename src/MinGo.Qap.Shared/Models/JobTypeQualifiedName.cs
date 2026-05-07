namespace MinGo.Qap.Shared.Models;

/// <summary>
/// CLR 类型限定名的结构化表示。
/// 从 Type.AssemblyQualifiedName 解析而来，各部件独立存储。
/// </summary>
public class JobTypeQualifiedName
{
    /// <summary>
    /// 类型完全限定名（Type.FullName），如 "Sample.Jobs.EchoJob"
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// 程序集简单名（Assembly.GetName().Name），如 "Sample.Jobs"
    /// </summary>
    public string Assembly { get; set; } = string.Empty;

    /// <summary>
    /// 程序集版本，如 "1.0.0.0"
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// 区域信息，如 "neutral"
    /// </summary>
    public string? Culture { get; set; }

    /// <summary>
    /// 公钥令牌，如 "null" 或实际十六进制值
    /// </summary>
    public string? PublicKeyToken { get; set; }

    /// <summary>
    /// 从 CLR Type 创建。
    /// </summary>
    public static JobTypeQualifiedName ParseFrom(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return ParseFrom(type.AssemblyQualifiedName ?? type.FullName ?? type.Name);
    }

    /// <summary>
    /// 从 Type.AssemblyQualifiedName 字符串解析。
    /// 格式：&lt;FullName&gt;, &lt;Assembly&gt;, Version=&lt;v&gt;, Culture=&lt;c&gt;, PublicKeyToken=&lt;t&gt;
    /// 支持泛型类型（含嵌套方括号）。
    /// </summary>
    public static JobTypeQualifiedName ParseFrom(string assemblyQualifiedName)
    {
        if (string.IsNullOrEmpty(assemblyQualifiedName))
            return new JobTypeQualifiedName { FullName = "unknown" };

        // 找到第一个不在方括号内的逗号，分割 FullName 与后续部分
        var fullName = assemblyQualifiedName;
        var remaining = string.Empty;

        var bracketDepth = 0;
        for (var i = 0; i < assemblyQualifiedName.Length; i++)
        {
            switch (assemblyQualifiedName[i])
            {
                case '[':
                    bracketDepth++;
                    break;
                case ']':
                    bracketDepth--;
                    break;
                case ',' when bracketDepth == 0:
                    fullName = assemblyQualifiedName[..i].Trim();
                    remaining = assemblyQualifiedName[(i + 1)..].Trim();
                    i = assemblyQualifiedName.Length; // break loop
                    break;
            }
        }

        var result = new JobTypeQualifiedName
        {
            FullName = fullName,
            Assembly = string.Empty
        };

        if (string.IsNullOrEmpty(remaining))
            return result;

        // 解析剩余部分：Assembly, Version=..., Culture=..., PublicKeyToken=...
        var parts = remaining.Split(',');
        for (var i = 0; i < parts.Length; i++)
        {
            var part = parts[i].Trim();
            if (i == 0)
            {
                result.Assembly = part;
            }
            else if (part.StartsWith("Version=", StringComparison.OrdinalIgnoreCase))
            {
                result.Version = part["Version=".Length..];
            }
            else if (part.StartsWith("Culture=", StringComparison.OrdinalIgnoreCase))
            {
                result.Culture = part["Culture=".Length..];
            }
            else if (part.StartsWith("PublicKeyToken=", StringComparison.OrdinalIgnoreCase))
            {
                result.PublicKeyToken = part["PublicKeyToken=".Length..];
            }
        }

        return result;
    }

    /// <summary>
    /// 拼接为可用于 Type.GetType() 的限定名字符串。
    /// 仅包含 FullName 和 Assembly，不含 version/culture，避免版本脆弱性。
    /// </summary>
    public string ToAssemblyQualifiedName()
    {
        return string.IsNullOrEmpty(Assembly)
            ? FullName
            : $"{FullName}, {Assembly}";
    }

    public override string ToString()
    {
        return ToAssemblyQualifiedName();
    }

    public override bool Equals(object? obj)
    {
        return obj is JobTypeQualifiedName other &&
               FullName == other.FullName;
    }

    public override int GetHashCode()
    {
        return FullName.GetHashCode(StringComparison.Ordinal);
    }
}
