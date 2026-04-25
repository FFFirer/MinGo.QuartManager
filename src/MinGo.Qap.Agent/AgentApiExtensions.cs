using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MinGo.Qap.Agent.Services;
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
        // ========== Job 管理 ==========

        // GET /api/agent/jobs - 列表
        app.MapGet($"{prefix}/jobs", async (
            HttpRequest request,
            [FromServices] IQuartzService quartz,
            CancellationToken ct) =>
        {
            var query = new JobQuery
            {
                Page = int.TryParse(request.Query["page"], out var p) ? p : 1,
                PageSize = int.TryParse(request.Query["pageSize"], out var ps) ? ps : 20,
                Status = request.Query["status"].FirstOrDefault(),
                Group = request.Query["group"].FirstOrDefault(),
                Keyword = request.Query["keyword"].FirstOrDefault()
            };
            var jobs = await quartz.GetJobsAsync(query);
            return Results.Ok(ApiResponse<List<JobSummaryDto>>.Ok(jobs));
        })
        .WithName("GetJobs");

        // GET /api/agent/jobs/{jobKey} - 详情
        app.MapGet($"{prefix}/jobs/{{jobKey}}", async (
            string jobKey,
            [FromServices] IQuartzService quartz,
            CancellationToken ct) =>
        {
            var job = await quartz.GetJobAsync(jobKey);
            return job != null
                ? Results.Ok(ApiResponse<JobDetailDto>.Ok(job))
                : Results.NotFound(ApiResponse<JobDetailDto>.Fail("Job not found"));
        })
        .WithName("GetJob");

        // POST /api/agent/jobs - 创建
        app.MapPost($"{prefix}/jobs", async (
            CreateJobRequest request,
            [FromServices] IQuartzService quartz,
            CancellationToken ct) =>
        {
            try
            {
                var job = await quartz.CreateJobAsync(request);
                return Results.Ok(ApiResponse<JobDetailDto>.Ok(job));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ApiResponse<JobDetailDto>.Fail(ex.Message));
            }
        })
        .WithName("CreateJob");

        // PUT /api/agent/jobs/{jobKey} - 更新
        app.MapPut($"{prefix}/jobs/{{jobKey}}", async (
            string jobKey,
            UpdateJobRequest request,
            [FromServices] IQuartzService quartz,
            CancellationToken ct) =>
        {
            try
            {
                await quartz.UpdateJobAsync(jobKey, request);
                return Results.Ok(ApiResponse<object>.Ok(new { }));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        })
        .WithName("UpdateJob");

        // DELETE /api/agent/jobs/{jobKey} - 删除
        app.MapDelete($"{prefix}/jobs/{{jobKey}}", async (
            string jobKey,
            [FromServices] IQuartzService quartz,
            CancellationToken ct) =>
        {
            try
            {
                await quartz.DeleteJobAsync(jobKey);
                return Results.Ok(ApiResponse<object>.Ok(new { }));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        })
        .WithName("DeleteJob");

        // ========== Job 操作 ==========

        // POST /api/agent/jobs/{jobKey}/trigger
        app.MapPost($"{prefix}/jobs/{{jobKey}}/trigger", async (
            string jobKey,
            [FromServices] IQuartzService quartz,
            CancellationToken ct) =>
        {
            try
            {
                await quartz.TriggerJobAsync(jobKey);
                return Results.Ok(ApiResponse<object>.Ok(new { }));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        })
        .WithName("TriggerJob");

        // POST /api/agent/jobs/{jobKey}/pause
        app.MapPost($"{prefix}/jobs/{{jobKey}}/pause", async (
            string jobKey,
            [FromServices] IQuartzService quartz,
            CancellationToken ct) =>
        {
            try
            {
                await quartz.PauseJobAsync(jobKey);
                return Results.Ok(ApiResponse<object>.Ok(new { }));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        })
        .WithName("PauseJob");

        // POST /api/agent/jobs/{jobKey}/resume
        app.MapPost($"{prefix}/jobs/{{jobKey}}/resume", async (
            string jobKey,
            [FromServices] IQuartzService quartz,
            CancellationToken ct) =>
        {
            try
            {
                await quartz.ResumeJobAsync(jobKey);
                return Results.Ok(ApiResponse<object>.Ok(new { }));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        })
        .WithName("ResumeJob");

        // ========== Scheduler 状态 ==========

        // GET /api/agent/scheduler
        app.MapGet($"{prefix}/scheduler", async (
            [FromServices] IQuartzService quartz,
            CancellationToken ct) =>
        {
            var state = await quartz.GetSchedulerStateAsync();
            return Results.Ok(ApiResponse<SchedulerStateDto>.Ok(state));
        })
        .WithName("GetSchedulerState");

        // ========== Job Manifest ==========

        // GET /api/agent/manifest
        app.MapGet($"{prefix}/manifest", async (
            [FromServices] IJobRegistry registry) =>
        {
            var manifest = await Task.FromResult(registry.GetManifest());
            return Results.Ok(ApiResponse<JobManifestDto>.Ok(manifest));
        })
        .WithName("GetManifest");
    }
}
