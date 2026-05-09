using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MinGo.Qap.Agent.Services;
using MinGo.Qap.Shared.Attributes;
using MinGo.Qap.Shared.Models;

namespace MinGo.Qap.Agent;

/// <summary>
/// Agent Minimal API 扩展
/// </summary>
public static class AgentApiExtensions
{
    /// <summary>
    /// 映射 Agent HTTP API 端点（Minimal API 风格）
    /// </summary>
    /// <param name="app">Endpoint 路由构建器</param>
    /// <param name="prefix">API 路由前缀，默认 /api/agent</param>
    public static void MapMinGoAgentApi(
        this IEndpointRouteBuilder app,
        string prefix = "/api/agent")
    {
        // ========== Scheduler 管理 ==========

        // GET /api/agent/schedulers - 获取所有 Scheduler 列表
        app.MapGet($"{prefix}/schedulers", GetSchedulersHandler)
           .WithName("GetSchedulers");

        // GET /api/agent/scheduler - 默认 Scheduler 状态（向后兼容）
        app.MapGet($"{prefix}/scheduler", GetDefaultSchedulerStateHandler)
           .WithName("GetDefaultSchedulerState");

        // ========== Job 管理 ==========

        // GET /api/agent/jobs - 列表
        app.MapGet($"{prefix}/jobs", GetJobsHandler)
           .WithName("GetJobs");

        // GET /api/agent/jobs/{name}[/{group}] - 详情
        app.MapGet($"{prefix}/jobs/{{name}}", GetJobHandler)
           .WithName("GetJob");
        app.MapGet($"{prefix}/jobs/{{name}}/{{group}}", GetJobHandler)
           .WithName("GetJobByGroup");

        // PUT /api/agent/jobs - 创建/替换（幂等）
        app.MapPut($"{prefix}/jobs", CreateJobHandler)
           .WithName("ReplaceJob");

        // PUT /api/agent/jobs/{name}[/{group}] - 更新
        app.MapPut($"{prefix}/jobs/{{name}}", UpdateJobHandler)
           .WithName("UpdateJob");
        app.MapPut($"{prefix}/jobs/{{name}}/{{group}}", UpdateJobHandler)
           .WithName("UpdateJobByGroup");

        // DELETE /api/agent/jobs/{name}[/{group}] - 删除
        app.MapDelete($"{prefix}/jobs/{{name}}", DeleteJobHandler)
           .WithName("DeleteJob");
        app.MapDelete($"{prefix}/jobs/{{name}}/{{group}}", DeleteJobHandler)
           .WithName("DeleteJobByGroup");

        // ========== Job 操作 ==========

        // POST /api/agent/jobs/{name}[/{group}]/trigger
        app.MapPost($"{prefix}/jobs/{{name}}/trigger", TriggerJobHandler)
           .WithName("TriggerJob");
        app.MapPost($"{prefix}/jobs/{{name}}/{{group}}/trigger", TriggerJobHandler)
           .WithName("TriggerJobByGroup");

        // POST /api/agent/jobs/{name}[/{group}]/pause
        app.MapPost($"{prefix}/jobs/{{name}}/pause", PauseJobHandler)
           .WithName("PauseJob");
        app.MapPost($"{prefix}/jobs/{{name}}/{{group}}/pause", PauseJobHandler)
           .WithName("PauseJobByGroup");

        // POST /api/agent/jobs/{name}[/{group}]/resume
        app.MapPost($"{prefix}/jobs/{{name}}/resume", ResumeJobHandler)
           .WithName("ResumeJob");
        app.MapPost($"{prefix}/jobs/{{name}}/{{group}}/resume", ResumeJobHandler)
           .WithName("ResumeJobByGroup");

        // ========== Job Manifest ==========

        // GET /api/agent/manifest
        app.MapGet($"{prefix}/manifest", GetManifestHandler)
           .WithName("GetManifest");
    }

    private static async Task<IResult> GetSchedulersHandler(
        [FromServices] IQuartzService quartz,
        [FromServices] IAgentSchedulerAccessor accessor,
        CancellationToken ct)
    {
        var schedulerNames = await quartz.GetSchedulerNamesAsync();
        var result = new List<object>();

        foreach (var name in schedulerNames)
        {
            try
            {
                var state = await quartz.GetSchedulerStateAsync(name);
                result.Add(new
                {
                    state.Name,
                    state.InstanceId,
                    state.Status,
                    state.RunningSince,
                    state.IsClustered,
                    state.JobCounts
                });
            }
            catch (Exception ex)
            {
                result.Add(new
                {
                    Name = name,
                    Status = "error",
                    Error = ex.Message
                });
            }
        }

        return Results.Ok(ApiResponse<List<object>>.Ok(result));
    }

    [SwaggerHeader("X-Scheduler-Name", "Scheduler 名称。由 Platform 转发时设置。")]
    private static async Task<IResult> GetDefaultSchedulerStateHandler(
        HttpRequest request,
        [FromServices] IQuartzService quartz,
        [FromServices] IAgentSchedulerAccessor accessor,
        CancellationToken ct)
    {
        var schedulerName = ParseSchedulerName(request, accessor);
        var state = await quartz.GetSchedulerStateAsync(schedulerName);
        return Results.Ok(ApiResponse<SchedulerStateDto>.Ok(state));
    }

    [SwaggerHeader("X-Scheduler-Name", "Scheduler 名称。由 Platform 转发时设置。")]
    private static async Task<IResult> GetJobsHandler(
        HttpRequest request,
        [FromServices] IQuartzService quartz,
        [FromServices] IAgentSchedulerAccessor accessor,
        CancellationToken ct)
    {
        var schedulerName = ParseSchedulerName(request, accessor);
        var query = new JobQuery
        {
            Page = int.TryParse(request.Query["page"], out var p) ? p : 1,
            PageSize = int.TryParse(request.Query["pageSize"], out var ps) ? ps : 20,
            Status = request.Query["status"].FirstOrDefault(),
            Group = request.Query["group"].FirstOrDefault(),
            Keyword = request.Query["keyword"].FirstOrDefault()
        };
        var result = await quartz.GetJobsAsync(schedulerName, query);
        return Results.Ok(ApiResponse<PagedResponse<JobSummaryDto>>.Ok(result));
    }

    [SwaggerHeader("X-Scheduler-Name", "Scheduler 名称。由 Platform 转发时设置。")]
    private static async Task<IResult> GetJobHandler(
        HttpRequest request,
        string name,
        string? group,
        [FromServices] IQuartzService quartz,
        [FromServices] IAgentSchedulerAccessor accessor,
        CancellationToken ct)
    {
        var schedulerName = ParseSchedulerName(request, accessor);
        var jobKey = new JobKeyDto(name, group ?? "DEFAULT");
        var job = await quartz.GetJobAsync(schedulerName, jobKey);
        return job != null
            ? Results.Ok(ApiResponse<JobDetailDto>.Ok(job))
            : Results.NotFound(ApiResponse<JobDetailDto>.Fail("Job not found"));
    }

    [SwaggerHeader("X-Scheduler-Name", "Scheduler 名称。由 Platform 转发时设置。")]
    private static async Task<IResult> CreateJobHandler(
        HttpRequest request,
        CreateJobRequest requestBody,
        [FromServices] IQuartzService quartz,
        [FromServices] IAgentSchedulerAccessor accessor,
        CancellationToken ct)
    {
        try
        {
            var schedulerName = ParseSchedulerName(request, accessor);
            var job = await quartz.CreateJobAsync(schedulerName, requestBody);
            return Results.Ok(ApiResponse<JobDetailDto>.Ok(job));
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ApiResponse<JobDetailDto>.Fail(ex.Message));
        }
    }

    [SwaggerHeader("X-Scheduler-Name", "Scheduler 名称。由 Platform 转发时设置。")]
    private static async Task<IResult> UpdateJobHandler(
        HttpRequest request,
        string name,
        string? group,
        UpdateJobRequest requestBody,
        [FromServices] IQuartzService quartz,
        [FromServices] IAgentSchedulerAccessor accessor,
        CancellationToken ct)
    {
        try
        {
            var schedulerName = ParseSchedulerName(request, accessor);
            var jobKey = new JobKeyDto(name, group ?? "DEFAULT");
            await quartz.UpdateJobAsync(schedulerName, jobKey, requestBody);
            return Results.Ok(ApiResponse<object>.Ok(new { }));
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [SwaggerHeader("X-Scheduler-Name", "Scheduler 名称。由 Platform 转发时设置。")]
    private static async Task<IResult> DeleteJobHandler(
        HttpRequest request,
        string name,
        string? group,
        [FromServices] IQuartzService quartz,
        [FromServices] IAgentSchedulerAccessor accessor,
        CancellationToken ct)
    {
        try
        {
            var schedulerName = ParseSchedulerName(request, accessor);
            var jobKey = new JobKeyDto(name, group ?? "DEFAULT");
            await quartz.DeleteJobAsync(schedulerName, jobKey);
            return Results.Ok(ApiResponse<object>.Ok(new { }));
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [SwaggerHeader("X-Scheduler-Name", "Scheduler 名称。由 Platform 转发时设置。")]
    private static async Task<IResult> TriggerJobHandler(
        HttpRequest request,
        string name,
        string? group,
        [FromServices] IQuartzService quartz,
        [FromServices] IAgentSchedulerAccessor accessor,
        CancellationToken ct)
    {
        try
        {
            var schedulerName = ParseSchedulerName(request, accessor);
            var jobKey = new JobKeyDto(name, group ?? "DEFAULT");
            await quartz.TriggerJobAsync(schedulerName, jobKey);
            return Results.Ok(ApiResponse<object>.Ok(new { }));
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [SwaggerHeader("X-Scheduler-Name", "Scheduler 名称。由 Platform 转发时设置。")]
    private static async Task<IResult> PauseJobHandler(
        HttpRequest request,
        string name,
        string? group,
        [FromServices] IQuartzService quartz,
        [FromServices] IAgentSchedulerAccessor accessor,
        CancellationToken ct)
    {
        try
        {
            var schedulerName = ParseSchedulerName(request, accessor);
            var jobKey = new JobKeyDto(name, group ?? "DEFAULT");
            await quartz.PauseJobAsync(schedulerName, jobKey);
            return Results.Ok(ApiResponse<object>.Ok(new { }));
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [SwaggerHeader("X-Scheduler-Name", "Scheduler 名称。由 Platform 转发时设置。")]
    private static async Task<IResult> ResumeJobHandler(
        HttpRequest request,
        string name,
        string? group,
        [FromServices] IQuartzService quartz,
        [FromServices] IAgentSchedulerAccessor accessor,
        CancellationToken ct)
    {
        try
        {
            var schedulerName = ParseSchedulerName(request, accessor);
            var jobKey = new JobKeyDto(name, group ?? "DEFAULT");
            await quartz.ResumeJobAsync(schedulerName, jobKey);
            return Results.Ok(ApiResponse<object>.Ok(new { }));
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [SwaggerHeader("X-Scheduler-Name", "Scheduler 名称。由 Platform 转发时设置。")]
    private static async Task<IResult> GetManifestHandler(
        [FromServices] IJobRegistry registry)
    {
        var manifest = await Task.FromResult(registry.GetManifest());
        return Results.Ok(ApiResponse<JobManifestDto>.Ok(manifest));
    }

    /// <summary>
    /// 解析 Scheduler 名称
    /// 优先级：X-Scheduler-Name Header > ?schedulerName= Query > 默认第一个
    /// </summary>
    private static string ParseSchedulerName(HttpRequest request, IAgentSchedulerAccessor accessor)
    {
        // 1. 尝试从 Header 获取
        var headerValue = request.Headers["X-Scheduler-Name"].FirstOrDefault();
        if (!string.IsNullOrEmpty(headerValue))
        {
            return headerValue;
        }

        // 2. 尝试从 Query 获取
        var queryValue = request.Query["schedulerName"].FirstOrDefault();
        if (!string.IsNullOrEmpty(queryValue))
        {
            return queryValue;
        }

        // 3. 使用默认第一个 Scheduler
        var firstScheduler = accessor.GetAll().Keys.FirstOrDefault();
        if (!string.IsNullOrEmpty(firstScheduler))
        {
            return firstScheduler;
        }

        throw new InvalidOperationException("No scheduler available");
    }
}
