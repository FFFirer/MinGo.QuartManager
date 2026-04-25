using Microsoft.AspNetCore.Mvc;
using Quartz;
using Quartz.Impl.Matchers;

namespace MinGo.Sample.Agent.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IScheduler _scheduler;
    private readonly ILogger<JobsController> _logger;

    public JobsController(IScheduler scheduler, ILogger<JobsController> logger)
    {
        _scheduler = scheduler;
        _logger = logger;
    }

    /// <summary>
    /// Get all registered jobs
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetJobs()
    {
        // Get all job keys using AnyGroup matcher
        var allKeys = await _scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
        var jobs = new List<object>();

        foreach (var key in allKeys)
        {
            var jobDetail = await _scheduler.GetJobDetail(key);
            var triggers = await _scheduler.GetTriggersOfJob(key);
            var nextFireTime = triggers.FirstOrDefault()?.GetNextFireTimeUtc();

            jobs.Add(new
            {
                Key = key.Name,
                Group = key.Group,
                Description = jobDetail?.Description,
                NextFireTime = nextFireTime?.LocalDateTime
            });
        }

        return Ok(jobs);
    }

    /// <summary>
    /// Get job details by key
    /// </summary>
    [HttpGet("{group}/{name}")]
    public async Task<ActionResult<object>> GetJob(string group, string name)
    {
        var key = new JobKey(name, group);
        var jobDetail = await _scheduler.GetJobDetail(key);

        if (jobDetail == null)
            return NotFound($"Job {group}/{name} not found");

        var triggers = await _scheduler.GetTriggersOfJob(key);

        return Ok(new
        {
            Key = key.Name,
            Group = key.Group,
            Description = jobDetail.Description,
            Triggers = triggers.Select(t => new
            {
                Key = t.Key.Name,
                NextFireTime = t.GetNextFireTimeUtc()?.LocalDateTime,
                PreviousFireTime = t.GetPreviousFireTimeUtc()?.LocalDateTime
            })
        });
    }

    /// <summary>
    /// Trigger a job manually
    /// </summary>
    [HttpPost("{group}/{name}/trigger")]
    public async Task<ActionResult> TriggerJob(string group, string name)
    {
        var key = new JobKey(name, group);
        var jobDetail = await _scheduler.GetJobDetail(key);

        if (jobDetail == null)
            return NotFound($"Job {group}/{name} not found");

        await _scheduler.TriggerJob(key);

        _logger.LogInformation("Triggered job {Group}/{Name}", group, name);

        return Ok(new { Message = $"Job {group}/{name} triggered successfully" });
    }
}