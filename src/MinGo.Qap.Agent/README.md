# MinGo.Qap.Agent

Quartz.NET Agent library for MinGo Platform — provides job discovery, scheduling, and execution logging capabilities.

## Features

- **Job Listener**: Captures Quartz.NET job execution events (start, complete, error) via `QapJobListener`
- **Log Collection**: Buffers execution logs and periodically reports to Platform via `LogCollectionService`
- **Scheduler Integration**: Auto-registers with Platform and receives job scheduling commands
- **YAML Configuration**: Define jobs and schedules via `config.yaml`
- **NSwag Client**: Auto-generated API client for Platform communication

## Quick Start

```csharp
builder.Services.AddQapAgent(builder.Configuration);

// Or configure manually:
builder.Services.AddQapAgent(options =>
{
    options.PlatformUrl = "http://localhost:5256";
    options.AgentName = "my-agent";
});
```

## Configuration

```yaml
# config.yaml
agent:
  name: my-agent
  platformUrl: http://platform:5256
  
jobs:
  - type: MyNamespace.MyJob, MyAssembly
    name: my-job
    group: DEFAULT
    schedule:
      type: cron
      cronExpression: "0 */5 * * * ?"
```

## NuGet

```bash
dotnet add package MinGo.Qap.Agent
```

## Dependencies

- `MinGo.Qap.Shared` — Common DTOs and data contracts
- `Quartz` 3.18+ — Job scheduling engine
- `NSwag.AspNetCore` — API client generation
- `YamlDotNet` — YAML configuration parsing
