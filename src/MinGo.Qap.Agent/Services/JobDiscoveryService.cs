using System.ComponentModel;
using System.Reflection;
using MinGo.Qap.Agent.Configuration;
using MinGo.Qap.Shared.Attributes;
using MinGo.Qap.Shared.Interfaces;
using Quartz;

namespace MinGo.Qap.Agent.Services;

/// <summary>
/// 作业发现服务 - 从程序集中发现 IJob 实现
/// </summary>
public interface IJobDiscoveryService
{
    /// <summary>
    /// 从配置中发现的程序集发现作业
    /// </summary>
    Task<IEnumerable<DiscoveredJobInfo>> DiscoverFromConfigAsync();

    /// <summary>
    /// 从指定程序集发现作业
    /// </summary>
    Task<IEnumerable<DiscoveredJobInfo>> DiscoverFromAssemblyAsync(string assemblyPath);

    /// <summary>
    /// 从程序集名称发现作业
    /// </summary>
    Task<IEnumerable<DiscoveredJobInfo>> DiscoverFromAssemblyNameAsync(string assemblyName);

    /// <summary>
    /// 从当前应用域发现作业
    /// </summary>
    Task<IEnumerable<DiscoveredJobInfo>> DiscoverFromCurrentDomainAsync();
}

/// <summary>
/// Job 发现服务实现
/// </summary>
public class JobDiscoveryService : IJobDiscoveryService
{
    private readonly AgentConfig _config;
    private readonly ILogger<JobDiscoveryService> _logger;

    public JobDiscoveryService(AgentConfig config, ILogger<JobDiscoveryService> logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger;
    }

    public async Task<IEnumerable<DiscoveredJobInfo>> DiscoverFromConfigAsync()
    {
        var discoveredJobs = new List<DiscoveredJobInfo>();

        // 如果配置了程序集路径，从程序集发现
        if (!string.IsNullOrEmpty(_config.Quartz.AssemblyPath))
        {
            var jobs = await DiscoverFromAssemblyAsync(_config.Quartz.AssemblyPath);
            discoveredJobs.AddRange(jobs);
        }

        // 从配置的类型名称发现
        foreach (var jobTypeName in _config.Quartz.JobTypes)
        {
            var jobInfo = CreateJobInfoFromTypeName(jobTypeName);
            if (jobInfo != null)
            {
                discoveredJobs.Add(jobInfo);
            }
        }

        // 如果没有配置任何作业，从当前应用域发现
        if (discoveredJobs.Count == 0)
        {
            _logger.LogWarning("No jobs configured, attempting to discover from current domain");
            var jobs = await DiscoverFromCurrentDomainAsync();
            discoveredJobs.AddRange(jobs);
        }

        _logger.LogInformation("Discovered {Count} jobs", discoveredJobs.Count);
        return discoveredJobs;
    }

    public Task<IEnumerable<DiscoveredJobInfo>> DiscoverFromAssemblyAsync(string assemblyPath)
    {
        return Task.Run(() =>
        {
            var jobs = new List<DiscoveredJobInfo>();

            if (!File.Exists(assemblyPath))
            {
                _logger.LogWarning("Assembly not found: {Path}", assemblyPath);
                return jobs;
            }

            try
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                jobs.AddRange(DiscoverJobsFromAssembly(assembly));
                _logger.LogInformation("Discovered {Count} jobs from assembly: {Path}", jobs.Count, assemblyPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load assembly: {Path}", assemblyPath);
            }

            return (IEnumerable<DiscoveredJobInfo>)jobs;
        });
    }

    public Task<IEnumerable<DiscoveredJobInfo>> DiscoverFromAssemblyNameAsync(string assemblyName)
    {
        return Task.Run(() =>
        {
            var jobs = new List<DiscoveredJobInfo>();

            try
            {
                var assembly = Assembly.Load(assemblyName);
                jobs.AddRange(DiscoverJobsFromAssembly(assembly));
                _logger.LogInformation("Discovered {Count} jobs from assembly: {Name}", jobs.Count, assemblyName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load assembly: {Name}", assemblyName);
            }

            return (IEnumerable<DiscoveredJobInfo>)jobs;
        });
    }

    public Task<IEnumerable<DiscoveredJobInfo>> DiscoverFromCurrentDomainAsync()
    {
        return Task.Run(() =>
        {
            var jobs = new List<DiscoveredJobInfo>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    jobs.AddRange(DiscoverJobsFromAssembly(assembly));
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Failed to inspect assembly: {Name}", assembly.FullName);
                }
            }

            return (IEnumerable<DiscoveredJobInfo>)jobs;
        });
    }

    private IEnumerable<DiscoveredJobInfo> DiscoverJobsFromAssembly(Assembly assembly)
    {
        var jobs = new List<DiscoveredJobInfo>();

        try
        {
            var jobTypes = assembly.GetTypes()
                .Where(t => typeof(IJob).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var jobType in jobTypes)
            {
                var jobInfo = CreateJobInfoFromType(jobType);
                if (jobInfo != null)
                {
                    jobs.Add(jobInfo);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to discover jobs from assembly: {Name}", assembly.FullName);
        }

        return jobs;
    }

    private DiscoveredJobInfo? CreateJobInfoFromType(Type jobType)
    {
        try
        {
            var key = jobType.Name;
            string? description = null;
            string group = "default";

            // 检查 QuartzJobAttribute
            var attr = jobType.GetCustomAttribute<QuartzJobAttribute>();
            if (attr != null)
            {
                group = attr.Group;
                description = attr.Description;
                if (!string.IsNullOrEmpty(attr.Description))
                {
                    key = $"{attr.Group}.{jobType.Name}";
                }
            }

            return new DiscoveredJobInfo(
                JobKey: key,
                JobTypeFullName: jobType.FullName ?? jobType.Name,
                Description: description,
                Parameters: null,
                Schedule: null
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create job info for type: {Type}", jobType.FullName);
            return null;
        }
    }

    private DiscoveredJobInfo? CreateJobInfoFromTypeName(string typeName)
    {
        try
        {
            var type = Type.GetType(typeName);
            if (type == null)
            {
                // 尝试从已加载的程序集中查找
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = assembly.GetType(typeName);
                    if (type != null) break;
                }
            }

            if (type == null)
            {
                _logger.LogWarning("Could not find type: {TypeName}", typeName);
                return null;
            }

            return CreateJobInfoFromType(type);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create job info from type name: {TypeName}", typeName);
            return null;
        }
    }
}
