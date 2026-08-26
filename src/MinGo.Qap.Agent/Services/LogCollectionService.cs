using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MinGo.Qap.Agent.Configuration;
using MinGo.Qap.Shared;
using MinGo.Qap.Shared.Models;
using System.Net.Http.Json;

namespace MinGo.Qap.Agent.Services;

// Lightweight log collector that buffers logs and flushes periodically (no external deps for now)
public interface ILogCollectionService
{
    void Start();
    Task StopAsync();
    void RecordJobStarted(string? schedulerName, JobKeyDto jobKey);
    void RecordJobCompleted(string? schedulerName, JobKeyDto jobKey, bool success, string? errorMessage = null, string? stackTrace = null, long? durationMs = null);
    Task FlushPendingLogsAsync();
    /// <summary>
    /// 当前缓冲的日志条数（用于 OTel Gauge 指标）
    /// </summary>
    int BufferedCount { get; }
}

public class LogCollectionService : ILogCollectionService
{
    private readonly AgentConfig _config;
    private readonly ILogger<LogCollectionService> _logger;
    private readonly Queue<ExecutionLogDto> _pendingLogs = new();
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAgentRegistrationService _registrationService;
    private readonly object _lock = new();
    private readonly System.Timers.Timer _flushTimer;
    private bool _started;

    public LogCollectionService(IOptions<AgentConfig> options,
        ILogger<LogCollectionService> logger,
        IHttpClientFactory httpClientFactory,
        IAgentRegistrationService registrationService)
    {
        _config = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _registrationService = registrationService;
        _flushTimer = new System.Timers.Timer(TimeSpan.FromSeconds(30).TotalMilliseconds);
        _flushTimer.AutoReset = true;
        _flushTimer.Elapsed += (s, e) => { _ = FlushPendingLogsAsync(); };
    }

    public void Start()
    {
        if (_started) return;
        _started = true;
        _logger.LogInformation("LogCollectionService started");
        _flushTimer.Start();
    }

    public async Task StopAsync()
    {
        if (!_started) return;
        _started = false;
        _flushTimer.Stop();
        await FlushPendingLogsAsync();
        _logger.LogInformation("LogCollectionService stopped");
    }

    public void RecordJobStarted(string? schedulerName, JobKeyDto jobKey)
    {
        if (!_started) return;
        var log = new ExecutionLogDto
        {
            JobKey = jobKey,
            SchedulerName = schedulerName,
            StartTime = DateTimeOffset.UtcNow,
            EndTime = null,
            Success = true
        };
        AddLog(log);
    }

    public void RecordJobCompleted(string? schedulerName, JobKeyDto jobKey, bool success, string? errorMessage = null, string? stackTrace = null, long? durationMs = null)
    {
        if (!_started) return;
        var log = new ExecutionLogDto
        {
            JobKey = jobKey,
            SchedulerName = schedulerName,
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow,
            DurationMs = durationMs,
            Success = success,
            ErrorMessage = errorMessage,
            StackTrace = stackTrace
        };
        AddLog(log);
    }

    public async Task FlushPendingLogsAsync()
    {
        using var activity = QapTelemetry.ActivitySource.StartActivity("qap.logs.flush");

        // 将待上传的日志发送到平台端（若已注册）
        List<ExecutionLogDto> toFlush;
        lock (_lock)
        {
            if (_pendingLogs.Count == 0) return;
            toFlush = new List<ExecutionLogDto>(_pendingLogs);
            _pendingLogs.Clear();
        }

        activity?.SetTag("log.count", toFlush.Count);

        // 尝试上传到平台端
        try
        {
            var reg = _registrationService.GetRegistrationInfo();
            if (reg != null && !string.IsNullOrEmpty(reg.PlatformApiBaseUrl))
            {
                var sw = Stopwatch.StartNew();
                var client = _httpClientFactory.CreateClient("PlatformApi");
                var url = $"{reg.PlatformApiBaseUrl}/api/agents/{reg.AgentId}/logs";
                var resp = await client.PostAsJsonAsync(url, toFlush, MinGoJsonDefaults.Options);
                sw.Stop();

                if (resp.IsSuccessStatusCode)
                {
                    QapTelemetry.LogsFlushed.Add(toFlush.Count,
                        new KeyValuePair<string, object?>("agent.id", reg.AgentId));
                    QapTelemetry.LogsFlushDuration.Record(sw.Elapsed.TotalMilliseconds,
                        new KeyValuePair<string, object?>("agent.id", reg.AgentId));
                    _logger.LogInformation("Uploaded {Count} logs to platform", toFlush.Count);
                }
                else
                {
                    QapTelemetry.LogsFlushFailed.Add(1,
                        new KeyValuePair<string, object?>("agent.id", reg.AgentId));
                    var err = await resp.Content.ReadAsStringAsync();
                    _logger.LogWarning("Platform log upload failed: {Status} - {Error}", resp.StatusCode, err);
                    // 回退日志到缓冲队列，便于下次重试
                    lock (_lock)
                    {
                        foreach (var l in toFlush) _pendingLogs.Enqueue(l);
                    }
                }
            }
            else
            {
                // 未注册，保留日志以便后续上传
                _logger.LogWarning("No registration info for log upload, keeping logs in buffer");
                lock (_lock)
                {
                    foreach (var l in toFlush) _pendingLogs.Enqueue(l);
                }
            }
        }
        catch (Exception ex)
        {
            var reg = _registrationService.GetRegistrationInfo();
            QapTelemetry.LogsFlushFailed.Add(1,
                new KeyValuePair<string, object?>("agent.id", reg?.AgentId ?? "unknown"));
            _logger.LogError(ex, "Error uploading logs to platform, keeping logs in buffer");
            lock (_lock)
            {
                foreach (var l in toFlush) _pendingLogs.Enqueue(l);
            }
        }
    }

    private void AddLog(ExecutionLogDto log)
    {
        lock (_lock)
        {
            _pendingLogs.Enqueue(log);
        }
    }

    /// <inheritdoc />
    public int BufferedCount
    {
        get { lock (_lock) { return _pendingLogs.Count; } }
    }
}
