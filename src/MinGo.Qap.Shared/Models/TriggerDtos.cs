namespace MinGo.Qap.Shared.Models;

/// <summary>
/// Trigger 摘要 DTO — 用于 Job 详情页展示关联的 Trigger 列表
/// </summary>
public class TriggerSummaryDto
{
    /// <summary>Trigger 名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Trigger 组</summary>
    public string Group { get; set; } = string.Empty;

    /// <summary>Trigger 类型: cron, simple, calendar, daily, none</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Trigger 状态: normal, paused, complete, blocked, error, none</summary>
    public string State { get; set; } = string.Empty;

    /// <summary>Cron 表达式（仅 Type=cron）</summary>
    public string? CronExpression { get; set; }

    /// <summary>间隔秒数（仅 Type=simple interval）</summary>
    public int? IntervalSeconds { get; set; }

    /// <summary>重复次数，-1 表示无限重复</summary>
    public int? RepeatCount { get; set; }

    /// <summary>已触发次数</summary>
    public int TimesTriggered { get; set; }

    /// <summary>关联日历名称</summary>
    public string? CalendarName { get; set; }

    /// <summary>描述</summary>
    public string? Description { get; set; }

    /// <summary>优先级</summary>
    public int Priority { get; set; }

    /// <summary>开始时间</summary>
    public DateTimeOffset? StartTime { get; set; }

    /// <summary>结束时间</summary>
    public DateTimeOffset? EndTime { get; set; }

    /// <summary>上次触发时间</summary>
    public DateTimeOffset? PreviousFireTime { get; set; }

    /// <summary>下次触发时间</summary>
    public DateTimeOffset? NextFireTime { get; set; }

    /// <summary>最终触发时间</summary>
    public DateTimeOffset? FinalFireTime { get; set; }
}
