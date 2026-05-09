## TriggerSummaryDto

```csharp
public class TriggerSummaryDto
{
    public string Name { get; set; }
    public string Group { get; set; }
    public string Type { get; set; }       // cron / simple / calendar / daily / none
    public string State { get; set; }      // normal / paused / complete / blocked / error / none
    public string? CronExpression { get; set; }
    public int? IntervalSeconds { get; set; }
    public int? RepeatCount { get; set; }
    public int TimesTriggered { get; set; }
    public string? CalendarName { get; set; }
    public string? Description { get; set; }
    public int Priority { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public DateTimeOffset? PreviousFireTime { get; set; }
    public DateTimeOffset? NextFireTime { get; set; }
    public DateTimeOffset? FinalFireTime { get; set; }
}
```

## 数据流

```
Quartz scheduler.GetTriggersOfJob()
  → QuartzSerivce.GetJobAsync()
    → 每个 trigger: GetTriggerState() + MapToTriggerDto()
      → JobDetailDto.Triggers
        → Platform JobService.GetAsync() 映射
          → JobDefinitionDto.Triggers
            → Frontend 解析并展示
```

## 前端展示

在 Job 信息网格下方新增 "Triggers" 区域，复用 DataTable 组件，列：
- Name / Group / Type / State / Cron / Next Fire / Previous Fire / Priority

点击 Trigger 行暂不导航（Trigger 详情页为未来能力）。

## 兼容性

- `JobDetailDto.Schedule` 仍保留（等于第一个 trigger 的 schedule），向后兼容
- `JobDefinitionDto.Triggers` 在 Agent 不可用回退时返回 null
- 不修改 DB schema 或 API 路由
