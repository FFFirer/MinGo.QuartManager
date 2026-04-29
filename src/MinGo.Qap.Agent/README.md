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

### Option A: Using config.yaml (default)

Create `config.yaml` in your project root:

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

### Option B: Using appsettings.json

You can also configure via ASP.NET Core `appsettings.json`:

```json
{
  "agent": {
    "clusterId": "my-cluster",
    "heartbeatIntervalSeconds": 30,
    "externalUrl": "",
    "networkInterface": ""
  },
  "platform": {
    "url": "http://localhost:5000",
    "apiToken": "your-api-token"
  },
  "quartz": {
    "jobTypes": [
      "MyApp.Jobs.DataSyncJob",
      "MyApp.Jobs.ReportJob"
    ],
    "properties": {
      "quartz.scheduler.instanceId": "AUTO",
      "quartz.threadPool.threadCount": "10"
    }
  }
}
```

> **Note**: `builder.AddMinGoAgent()` reads from the ASP.NET Core configuration pipeline, so you can use `appsettings.json`, environment variables, or user secrets interchangeably. Configuration priority (highest to lowest): **Environment Variables > User Secrets > appsettings.Development.json > appsettings.json > config.yaml**

### Configure in Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add MinGo Agent services (does NOT initialize Quartz)
builder.AddMinGoAgent();

// Host application initializes Quartz Scheduler itself
builder.Services.AddSingleton<IScheduler>(sp =>
{
    var factory = new StdSchedulerFactory();
    var scheduler = factory.GetScheduler().GetAwaiter().GetResult();
    scheduler.Start();
    return scheduler;
});

var app = builder.Build();

// Map MinGo Agent HTTP API
app.MapMinGoAgentApi();

app.Run();
```

## Configuration

### Agent Settings

| Property | Type | Default | Description |
|---------|------|---------|-------------|
| `clusterId` | string | required | Cluster identifier |
| `id` | string | auto-generated | Agent instance ID |
| `port` | int | 8080 | HTTP listening port |
| `heartbeatIntervalSeconds` | int | 30 | Heartbeat interval |
| `registrationMaxAttempts` | int | 5 | Max registration retry attempts |
| `registrationRetryDelaySeconds` | int | 5 | Delay between registration retries |
| `clusterMode` | bool | false | Enable Quartz clustering |
| `externalUrl` | string | `null` | Explicit external URL (highest priority) |
| `networkInterface` | string | `null` | Network interface name for IP binding (e.g. `eth0`) |

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

- **Library, not application**: Agent is a class library; host application controls the web server and Quartz Scheduler lifecycle
- **Minimal API provided**: Agent exposes HTTP endpoints via `MapMinGoAgentApi()` for Platform integration
- **No Platform dependency**: Uses only Shared contracts
- **Host-managed Quartz**: Agent does not initialize Quartz Scheduler; host application is responsible for scheduler creation

## License

MIT