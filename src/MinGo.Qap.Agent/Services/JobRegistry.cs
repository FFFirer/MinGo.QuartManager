using MinGo.Qap.Shared.Models;

namespace MinGo.Qap.Agent.Services;

/// <summary>
/// Job 类型注册表
/// </summary>
public interface IJobRegistry
{
    /// <summary>
    /// 注册 Job Manifest
    /// </summary>
    void Register(JobManifestDto manifest);
    
    /// <summary>
    /// 根据 Key 获取 Job 类型信息
    /// </summary>
    JobTypeInfoDto? Get(string jobTypeKey);
    
    /// <summary>
    /// 获取所有注册的 Job 类型
    /// </summary>
    IEnumerable<JobTypeInfoDto> GetAll();
    
    /// <summary>
    /// 获取完整的 Manifest
    /// </summary>
    JobManifestDto GetManifest();
    
    /// <summary>
    /// 检查 Job 类型是否已注册
    /// </summary>
    bool Contains(string jobTypeKey);
}

/// <summary>
/// Job Registry 实现
/// </summary>
public class JobRegistry : IJobRegistry
{
    private readonly List<JobTypeInfoDto> _jobs = new();
    private readonly ILogger<JobRegistry> _logger;

    public JobRegistry(ILogger<JobRegistry> logger, JobManifestDto? manifest = null)
    {
        _logger = logger;
        if (manifest?.Jobs != null)
        {
            Register(manifest);
        }
    }

    public void Register(JobManifestDto manifest)
    {
        if (manifest?.Jobs == null) return;

        foreach (var job in manifest.Jobs)
        {
            RegisterSingle(job);
        }
    }

    private void RegisterSingle(JobTypeInfoDto jobInfo)
    {
        if (string.IsNullOrWhiteSpace(jobInfo.Key))
        {
            _logger.LogWarning("Skipping job registration: Key is empty");
            return;
        }

        // 检查是否已存在
        var existing = _jobs.FirstOrDefault(j => j.Key == jobInfo.Key);
        if (existing != null)
        {
            _logger.LogInformation("Updating registered job type: {Key}", jobInfo.Key);
            _jobs.Remove(existing);
        }
        else
        {
            _logger.LogInformation("Registering job type: {Key}", jobInfo.Key);
        }

        _jobs.Add(jobInfo);
    }

    public JobTypeInfoDto? Get(string jobTypeKey)
    {
        return _jobs.FirstOrDefault(j => j.Key == jobTypeKey);
    }

    public IEnumerable<JobTypeInfoDto> GetAll()
    {
        return _jobs.AsReadOnly();
    }

    public JobManifestDto GetManifest()
    {
        return new JobManifestDto
        {
            Jobs = _jobs.ToList()
        };
    }

    public bool Contains(string jobTypeKey)
    {
        return _jobs.Any(j => j.Key == jobTypeKey);
    }
}
