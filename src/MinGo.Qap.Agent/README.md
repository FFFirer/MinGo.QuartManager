# MinGo.Qap.Agent

Quartz.NET Agent library for MinGo Platform - provides job discovery, scheduling, execution, and logging capabilities.

## Overview

**MinGo.Qap.Agent** is a .NET library that wraps Quartz.NET to provide automated job management with MinGo Platform integration. It is designed to be added to any .NET application that uses Quartz.NET.

## Features

- **Job Discovery**: Automatically discover and register IJob implementations from assemblies
- **Job Registration**: Register jobs to Quartz.NET scheduler
- **Platform Integration**: Register/deregister with MinGo Platform via REST API
- **Heartbeat**: Periodic health reporting to Platform
- **Log Collection**: Collect and report execution logs to Platform

## Requirements

- .NET 10.0
- Quartz.NET 3.17.1+
- An application using ASP.NET Core (for DI integration)

## Installation

```bash
dotnet add package MinGo.Qap.Agent
```

## Quick Start

### 1. Create config.yaml

```yaml
agent:
  clusterId: "my-cluster"
  heartbeatIntervalSeconds: 30
  
platform:
  url: "http://localhost:5000"
  apiToken: "your-api-token"

quartz:
  jobTypes:
    - "MyApp.Jobs.DataSyncJob"
    - "MyApp.Jobs.ReportJob"
  properties:
    quartz.scheduler.instanceId: "AUTO"
    quartz.threadPool.threadCount: 10
```

### 2. Configure in Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add MinGo Agent services
builder.Services.AddMinGoAgent(builder.Configuration);

// Add Quartz hosted service
builder.Services.AddQuartzHostedService();

var app = builder.Build();

// Register with Platform on startup
app.UseMinGoAgent();

app.Run();
```

## Configuration

### Agent Settings

| Property | Type | Default | Description |
|---------|------|---------|-------------|
| `clusterId` | string | required | Cluster identifier |
| `id` | string | auto-generated | Agent instance ID |
| `heartbeatIntervalSeconds` | int | 30 | Heartbeat interval |
| `clusterMode` | bool | false | Enable Quartz clustering |

### Platform Settings

| Property | Type | Default | Description |
|---------|------|---------|-------------|
| `url` | string | required | Platform API URL |
| `apiToken` | string | required | API authentication token |

### Quartz Settings

| Property | Type | Default | Description |
|---------|------|---------|-------------|
| `jobTypes` | list | [] | Job type full names |
| `assemblyPath` | string | "" | Path to scan for jobs |
| `properties` | dict | {} | Quartz properties |

## Usage

### Adding Jobs

```csharp
// Jobs are automatically registered from config.yaml
// Or via assembly scanning
```

### Triggering Jobs Manually

Inject `IQuartzService` and use:

```csharp
public class MyController : ControllerBase
{
    private readonly IQuartzService _quartzService;
    
    public MyController(IQuartzService quartzService)
    {
        _quartzService = quartzService;
    }
    
    [HttpPost("/jobs/{jobKey}/trigger")]
    public async Task Trigger(string jobKey)
    {
        await _quartzService.TriggerJobAsync(jobKey);
    }
}
```

## Architecture

```
┌─────────────────────┐
│   MinGo.Qap.Agent  │
│   (Class Library)   │
├─────────────────────┤
│ Job Discovery      │
│ Job Registration  │
│ Log Collection   │
│ Heartbeat       │
│ Platform Client │
└─────────────────────┘
         │
         │ wraps
         ▼
┌─────────────────────┐
│    Quartz.NET     │
│   Scheduler      │
└─────────────────────┘
```

## Constraints

- **No HTTP endpoints**: Agent is a library, not a web application
- **No Platform dependency**: Uses only Shared contracts
- **Quartz.NET DI**: Integrates via Quartz.Extensions.DependencyInjection

## License

MIT