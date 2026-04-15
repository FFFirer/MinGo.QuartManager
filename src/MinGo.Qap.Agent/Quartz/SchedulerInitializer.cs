using Quartz;
using Quartz.Impl;
using System.Collections.Specialized;

namespace MinGo.Qap.Agent.Quartz;

/// <summary>
/// Quartz Scheduler 初始化器
/// </summary>
public class SchedulerInitializer
{
    private readonly IConfiguration _quartzConfig;
    private readonly ILogger<SchedulerInitializer> _logger;

    public SchedulerInitializer(IConfiguration quartzConfig, ILogger<SchedulerInitializer> logger)
    {
        _quartzConfig = quartzConfig;
        _logger = logger;
    }

    /// <summary>
    /// 初始化并启动 Scheduler
    /// </summary>
    public async Task<IScheduler> InitializeAsync()
    {
        _logger.LogInformation("Initializing Quartz Scheduler...");

        try
        {
            // 从配置构建 Quartz 属性
            var properties = BuildQuartzProperties();

            // 创建 Scheduler 工厂
            var schedulerFactory = new StdSchedulerFactory(properties);

            // 获取 Scheduler
            var scheduler = await schedulerFactory.GetScheduler();

            // 添加全局 Job 监听器（用于执行日志，V2 实现）
            // scheduler.ListenerManager.AddJobListener(...);

            // 启动 Scheduler
            await scheduler.Start();

            _logger.LogInformation(
                "Quartz Scheduler started successfully. InstanceId: {InstanceId}, Name: {Name}",
                scheduler.SchedulerInstanceId,
                scheduler.SchedulerName);

            return scheduler;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Quartz Scheduler");
            throw;
        }
    }

    /// <summary>
    /// 从配置构建 Quartz 属性
    /// </summary>
    private NameValueCollection BuildQuartzProperties()
    {
        var properties = new NameValueCollection();

        // 基础属性
        var instanceName = _quartzConfig["quartz.scheduler.instanceName"] ?? "QapAgentScheduler";
        properties["quartz.scheduler.instanceName"] = instanceName;

        // JobStore 类型
        var jobStoreType = _quartzConfig["quartz.jobStore.type"] ?? "Quartz.Simpl.RAMJobStore, Quartz";
        properties["quartz.jobStore.type"] = jobStoreType;

        // 如果是 ADO.NET JobStore，配置相关属性
        if (jobStoreType.Contains("AdoJobStore"))
        {
            var driverDelegate = _quartzConfig["quartz.jobStore.driverDelegateType"];
            if (!string.IsNullOrEmpty(driverDelegate))
            {
                properties["quartz.jobStore.driverDelegateType"] = driverDelegate;
            }

            var dataSource = _quartzConfig["quartz.jobStore.dataSource"];
            if (!string.IsNullOrEmpty(dataSource))
            {
                properties["quartz.jobStore.dataSource"] = dataSource;

                // 数据源连接字符串
                var connectionString = _quartzConfig[$"quartz.dataSource.{dataSource}.connectionString"];
                if (!string.IsNullOrEmpty(connectionString))
                {
                    properties[$"quartz.dataSource.{dataSource}.connectionString"] = connectionString;
                }

                // 数据源提供程序
                var provider = _quartzConfig[$"quartz.dataSource.{dataSource}.provider"];
                if (!string.IsNullOrEmpty(provider))
                {
                    properties[$"quartz.dataSource.{dataSource}.provider"] = provider;
                }
            }

            // 表前缀（可选）
            var tablePrefix = _quartzConfig["quartz.jobStore.tablePrefix"];
            if (!string.IsNullOrEmpty(tablePrefix))
            {
                properties["quartz.jobStore.tablePrefix"] = tablePrefix;
            }
        }

        // 线程池配置
        var maxThreads = _quartzConfig["quartz.threadPool.maxThreads"] ?? "10";
        properties["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz";
        properties["quartz.threadPool.threadCount"] = maxThreads;

        // 其他自定义属性
        foreach (var configItem in _quartzConfig.GetChildren())
        {
            var key = configItem.Key;
            var value = configItem.Value;

            if (!string.IsNullOrEmpty(value) && !properties.AllKeys.Contains(key))
            {
                properties[key] = value;
            }
        }

        return properties;
    }
}
