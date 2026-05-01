using System.Net.Http.Json;
using System.Text.Json;
using MinGo.Qap.Shared.Models;
using Quartz;
using Quartz.Impl;

namespace MinGo.Qap.Agent.Services;

/// <summary>
/// Scheduler 信息上报服务
/// </summary>
public class SchedulerReporterService
{
    private readonly IAgentSchedulerAccessor _schedulerAccessor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SchedulerReporterService> _logger;

    // 重试配置
    private const int MaxRetries = 3;
    private static readonly TimeSpan[] RetryDelays = {
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(4)
    };

    public SchedulerReporterService(
        IAgentSchedulerAccessor schedulerAccessor,
        IHttpClientFactory httpClientFactory,
        ILogger<SchedulerReporterService> logger)
    {
        _schedulerAccessor = schedulerAccessor ?? throw new ArgumentNullException(nameof(schedulerAccessor));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 采集并上报所有 Scheduler 信息
    /// </summary>
    /// <param name="platformUrl">Platform 地址</param>
    /// <param name="agentId">Agent ID</param>
    /// <param name="token">认证 Token</param>
    /// <returns>是否上报成功</returns>
    public async Task<bool> ReportAsync(string platformUrl, string agentId, string token)
    {
        try
        {
            // 采集 Scheduler 信息
            var schedulers = CollectSchedulerInfo();
            if (schedulers.Count == 0)
            {
                _logger.LogWarning("No schedulers found to report");
                return false;
            }

            // 构建上报请求
            var request = new SchedulerReportRequest
            {
                Schedulers = schedulers
            };

            // 发送上报请求（带重试）
            return await SendReportWithRetryAsync(platformUrl, agentId, token, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to report scheduler information");
            return false;
        }
    }

    /// <summary>
    /// 采集所有 Scheduler 的运行时信息
    /// </summary>
    private List<SchedulerInfoDto> CollectSchedulerInfo()
    {
        var schedulers = _schedulerAccessor.GetAll();
        var result = new List<SchedulerInfoDto>();

        foreach (var kvp in schedulers)
        {
            try
            {
                var info = ExtractSchedulerInfo(kvp.Value);
                result.Add(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract scheduler info for {SchedulerName}", kvp.Key);
            }
        }

        return result;
    }

    /// <summary>
    /// 从 IScheduler 提取信息
    /// </summary>
    private SchedulerInfoDto ExtractSchedulerInfo(IScheduler scheduler)
    {
        var metaData = scheduler.GetMetaData().Result;
        var jobCounts = scheduler.GetJobGroupNames().Result.Count;
        var triggerCounts = scheduler.GetTriggerGroupNames().Result.Count;

        // 获取当前执行的 Job 数量
        var currentlyExecutingJobs = scheduler.GetCurrentlyExecutingJobs().Result.Count;

        // 获取 Job 存储信息
        var jobStoreType = metaData.JobStoreType?.Name;
        var threadPoolType = metaData.ThreadPoolType?.Name;

        // RunningSince 转为 UTC
        var runningSince = metaData.RunningSince.HasValue
            ? new DateTimeOffset(metaData.RunningSince.Value, TimeSpan.Zero)
            : (DateTimeOffset?)null;

        return new SchedulerInfoDto
        {
            SchedulerName = scheduler.SchedulerName,
            SchedulerInstanceId = scheduler.SchedulerInstanceId,
            Status = scheduler.IsStarted && !scheduler.InStandbyMode ? "running" : "standby",
            IsClustered = metaData.JobStoreClustered,
            JobStoreType = jobStoreType,
            ThreadPoolType = threadPoolType,
            ThreadPoolSize = metaData.ThreadPoolSize,
            RunningSince = runningSince,
            Version = metaData.Version,
            NumberOfJobsExecuted = metaData.NumberOfJobsExecuted,
            JobCounts = new JobCountsDto
            {
                TotalJobs = jobCounts,
                RunningJobs = currentlyExecutingJobs
            },
            Properties = new Dictionary<string, string>
            {
                ["SchedulerInstanceId"] = scheduler.SchedulerInstanceId,
                ["SupportsPersistence"] = metaData.SupportsPersistence.ToString(),
                ["TriggerCount"] = triggerCounts.ToString()
            }
        };
    }

    /// <summary>
    /// 带重试机制的发送上报
    /// </summary>
    private async Task<bool> SendReportWithRetryAsync(
        string platformUrl,
        string agentId,
        string token,
        SchedulerReportRequest request)
    {
        var client = _httpClientFactory.CreateClient();
        var url = $"{platformUrl.TrimEnd('/')}/api/agents/{agentId}/schedulers";

        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
                httpRequest.Headers.Add("X-Agent-Token", token);
                httpRequest.Content = JsonContent.Create(request);

                var response = await client.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "Successfully reported {Count} schedulers to platform",
                        request.Schedulers.Count);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Failed to report schedulers (attempt {Attempt}/{Max}): {StatusCode} - {Error}",
                    attempt + 1,
                    MaxRetries,
                    response.StatusCode,
                    errorContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error reporting schedulers (attempt {Attempt}/{Max})",
                    attempt + 1,
                    MaxRetries);
            }

            // 等待后重试
            if (attempt < MaxRetries - 1)
            {
                await Task.Delay(RetryDelays[attempt]);
            }
        }

        _logger.LogError("Failed to report schedulers after {Max} attempts", MaxRetries);
        return false;
    }
}
