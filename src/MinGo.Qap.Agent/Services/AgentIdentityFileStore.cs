using System.Text.Json;
using MinGo.Qap.Shared.Models;

namespace MinGo.Qap.Agent.Services;

/// <summary>
/// 基于文件的 Agent 身份存储实现
/// </summary>
public class AgentIdentityFileStore : IAgentIdentityStore
{
    private readonly string _filePath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// 使用默认路径创建（AppContext.BaseDirectory/agent-identity.json）
    /// </summary>
    public AgentIdentityFileStore()
        : this(Path.Combine(AppContext.BaseDirectory, "agent-identity.json"))
    {
    }

    /// <summary>
    /// 使用指定路径创建
    /// </summary>
    public AgentIdentityFileStore(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }

    /// <inheritdoc />
    public AgentIdentity? Load()
    {
        if (!File.Exists(_filePath))
            return null;

        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<AgentIdentity>(json, JsonOptions);
        }
        catch (Exception)
        {
            // 读取失败，返回 null（视为首次注册）
            return null;
        }
    }

    /// <inheritdoc />
    public void Save(AgentIdentity identity)
    {
        if (identity == null)
            throw new ArgumentNullException(nameof(identity));

        // 原子写入：写入临时文件 → 重命名
        var tempFile = _filePath + ".tmp";
        var json = JsonSerializer.Serialize(identity, JsonOptions);

        // 确保目录存在
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(tempFile, json);
        File.Move(tempFile, _filePath, overwrite: true);
    }

    /// <inheritdoc />
    public void Clear()
    {
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }
    }
}
