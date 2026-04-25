using System.Threading.Tasks;
using Quartz;

namespace MinGo.QuartzManager.Agent.Tests;

/// <summary>
/// Test job for integration tests.
/// </summary>
public class HelloJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        return Task.CompletedTask;
    }
}
