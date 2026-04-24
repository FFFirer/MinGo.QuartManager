using MinGo.Qap.Shared.Models;

namespace MinGo.Qap.Shared.Interfaces;

/// <summary>
/// 日志上报接口
/// </summary>
public interface ILogReporter
{
    /// <summary>
    /// 上报作业执行日志
    /// </summary>
    Task<bool> ReportExecutionLogAsync(ExecutionLogDto log, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量上报作业执行日志
    /// </summary>
    Task<bool> ReportExecutionLogsAsync(IEnumerable<ExecutionLogDto> logs, CancellationToken cancellationToken = default);
}