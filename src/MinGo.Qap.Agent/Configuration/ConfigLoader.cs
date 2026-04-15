using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MinGo.Qap.Agent.Configuration;

/// <summary>
/// 配置加载器
/// </summary>
public class ConfigLoader
{
    private readonly IConfiguration _environmentConfig;
    
    public ConfigLoader(IConfiguration environmentConfig)
    {
        _environmentConfig = environmentConfig;
    }
    
    /// <summary>
    /// 从文件加载配置
    /// </summary>
    public AgentConfig Load(string configPath = "config.yaml")
    {
        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException(
                $"Configuration file not found: {configPath}. " +
                "Please create a config.yaml file or specify a different path.");
        }
        
        var yamlContent = File.ReadAllText(configPath);
        
        // 替换环境变量占位符 ${VAR_NAME}
        yamlContent = ReplaceEnvironmentVariables(yamlContent);
        
        // 解析 YAML
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        
        var config = deserializer.Deserialize<AgentConfig>(yamlContent);
        
        // 合并环境变量覆盖
        MergeEnvironmentOverrides(config);
        
        // 验证配置
        Validate(config);
        
        // 设置默认值
        ApplyDefaults(config);
        
        return config;
    }
    
    /// <summary>
    /// 替换 YAML 中的环境变量占位符
    /// </summary>
    private string ReplaceEnvironmentVariables(string content)
    {
        // 匹配 ${VAR_NAME} 或 ${VAR_NAME:default}
        var pattern = @"\$\{(\w+)(?::([^}]*))?\}";
        
        return Regex.Replace(content, pattern, match =>
        {
            var varName = match.Groups[1].Value;
            var defaultValue = match.Groups[2].Success ? match.Groups[2].Value : null;
            
            var envValue = Environment.GetEnvironmentVariable(varName);
            
            if (!string.IsNullOrEmpty(envValue))
            {
                return envValue;
            }
            
            if (defaultValue != null)
            {
                return defaultValue;
            }
            
            // 保留原样，让后续验证处理
            return match.Value;
        });
    }
    
    /// <summary>
    /// 合并环境变量覆盖
    /// </summary>
    private void MergeEnvironmentOverrides(AgentConfig config)
    {
        // QAP_AGENT_ID
        var agentId = _environmentConfig["QAP_AGENT_ID"];
        if (!string.IsNullOrEmpty(agentId))
        {
            config.Agent.Id = agentId;
        }
        
        // QAP_CLUSTER_ID
        var clusterId = _environmentConfig["QAP_CLUSTER_ID"];
        if (!string.IsNullOrEmpty(clusterId))
        {
            config.Agent.ClusterId = clusterId;
        }
        
        // QAP_PORT
        var port = _environmentConfig["QAP_PORT"];
        if (!string.IsNullOrEmpty(port) && int.TryParse(port, out var portValue))
        {
            config.Agent.Port = portValue;
        }
        
        // QAP_PLATFORM_URL
        var platformUrl = _environmentConfig["QAP_PLATFORM_URL"];
        if (!string.IsNullOrEmpty(platformUrl))
        {
            config.Platform.Url = platformUrl;
        }
        
        // QAP_LOG_LEVEL
        var logLevel = _environmentConfig["QAP_LOG_LEVEL"];
        if (!string.IsNullOrEmpty(logLevel))
        {
            config.Logging ??= new LoggingSettings();
            config.Logging.Level = logLevel;
        }
    }
    
    /// <summary>
    /// 验证配置
    /// </summary>
    private void Validate(AgentConfig config)
    {
        var errors = new List<string>();
        
        // 验证 ClusterId
        if (string.IsNullOrWhiteSpace(config.Agent.ClusterId))
        {
            errors.Add("Agent.ClusterId is required. Set it in config.yaml or via QAP_CLUSTER_ID environment variable.");
        }
        
        // 验证 Platform URL
        if (string.IsNullOrWhiteSpace(config.Platform.Url))
        {
            errors.Add("Platform.Url is required. Set it in config.yaml or via QAP_PLATFORM_URL environment variable.");
        }
        else if (!Uri.TryCreate(config.Platform.Url, UriKind.Absolute, out _))
        {
            errors.Add($"Platform.Url is not a valid URL: {config.Platform.Url}");
        }
        
        // 验证 Port
        if (config.Agent.Port < 1 || config.Agent.Port > 65535)
        {
            errors.Add($"Agent.Port must be between 1 and 65535. Current value: {config.Agent.Port}");
        }
        
        // 验证 AssemblyPath
        if (!string.IsNullOrWhiteSpace(config.Quartz.AssemblyPath))
        {
            // 如果是文件，检查是否存在
            if (File.Exists(config.Quartz.AssemblyPath))
            {
                // OK，是文件
            }
            // 如果是目录，检查是否存在
            else if (Directory.Exists(config.Quartz.AssemblyPath))
            {
                // OK，是目录
            }
            else
            {
                // 警告但不报错（可能是在容器内，路径挂载后才会存在）
                Console.WriteLine($"Warning: Quartz.AssemblyPath does not exist: {config.Quartz.AssemblyPath}");
            }
        }
        
        if (errors.Any())
        {
            throw new InvalidOperationException(
                "Configuration validation failed:\n" + 
                string.Join("\n", errors.Select(e => $"  - {e}")));
        }
    }
    
    /// <summary>
    /// 应用默认值
    /// </summary>
    private void ApplyDefaults(AgentConfig config)
    {
        // 生成默认 Agent ID
        if (string.IsNullOrWhiteSpace(config.Agent.Id))
        {
            config.Agent.Id = $"agent-{Guid.NewGuid().ToString()[..8]}";
        }
        
        // 确保 JobTypes 不为 null
        config.Quartz.JobTypes ??= new List<string>();
        
        // 确保 Properties 不为 null
        config.Quartz.Properties ??= new Dictionary<string, string>();
        
        // 确保 Logging 不为 null
        config.Logging ??= new LoggingSettings();
    }
}
