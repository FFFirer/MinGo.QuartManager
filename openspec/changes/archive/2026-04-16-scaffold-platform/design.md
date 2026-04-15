# Design: MinGo.Qap Platform

## System Overview

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         MinGo.Qap Platform                              │
│                      Quartz Admin Platform (QAP)                       │
└─────────────────────────────────────────────────────────────────────────┘

                              HTTP REST
                                  │
              ┌───────────────────┼───────────────────┐
              │                   │                   │
              ▼                   ▼                   ▼
       ┌──────────────┐   ┌──────────────┐   ┌──────────────┐
       │   Agent A    │   │   Agent B    │   │   Agent C    │
       │ Cluster A    │   │ Cluster B    │   │ Cluster C    │
       └──────┬───────┘   └──────┬───────┘   └──────┬───────┘
              │                   │                   │
              │    ┌──────────────┴───────────────────┘
              │    │
              ▼    │
       ┌────────────────────┐
       │  Quartz Scheduler  │
       │  ┌──────────────┐│
       │  │   Jobs       ││
       │  │   Triggers   ││
       │  │   JobStore   ││
       │  └──────────────┘│
       └────────────────────┘
```

## Core Principles

### 1. Cluster-First
所有操作基于当前选中的 Cluster。用户必须先选择 Cluster，再查看和操作其中的资源。

### 2. Quartz-Native
平台只做：
- 配置映射（UI JSON → Quartz Objects）
- API 封装（转发请求）
- 可视化（状态展示）

不做：
- 调度逻辑（完全由 Quartz 决定）
- 业务编排
- 自定义调度算法

### 3. Agent-Stateless
Agent 本身不持久化数据：
- Job 数据 → Quartz JobStore (DB)
- Cluster 元数据 → Platform DB
- 状态 → Agent 内存缓存（可重建）

## Module Design

### MinGo.Qap.Platform

#### Responsibilities
1. Cluster 生命周期管理
2. JobDefinition 备份（可选）
3. 请求转发到 Agent
4. 跨集群聚合查询
5. 心跳状态计算

#### Internal Components
```
Platform
├── API Layer
│   ├── ClustersController
│   ├── JobsController
│   └── ManifestController
├── Services
│   ├── IClusterService
│   ├── IJobService
│   └── IAgentProxyService
├── Data
│   ├── PlatformDbContext
│   └── Entities
│       ├── Cluster
│       └── JobDefinition
└── Heartbeat
    └── HeartbeatHandler
```

#### Key Service Interfaces

```csharp
public interface IClusterService
{
    Task<ClusterDto> CreateAsync(CreateClusterRequest request);
    Task<ClusterDto?> GetAsync(string clusterId);
    Task<IEnumerable<ClusterSummary>> GetAllAsync();
    Task UpdateHeartbeatAsync(string clusterId, HeartbeatDto heartbeat);
    Task DeleteAsync(string clusterId);
}

public interface IJobService
{
    Task<JobDefinitionDto> CreateAsync(string clusterId, CreateJobRequest request);
    Task<JobDefinitionDto?> GetAsync(string clusterId, string jobKey);
    Task<IEnumerable<JobSummaryDto>> GetByClusterAsync(string clusterId);
    Task TriggerAsync(string clusterId, string jobKey);
    Task PauseAsync(string clusterId, string jobKey);
    Task ResumeAsync(string clusterId, string jobKey);
}

public interface IAgentProxyService
{
    Task<T> GetAsync<T>(string clusterId, string path);
    Task<T> PostAsync<T>(string clusterId, string path, object body);
    Task<T> PutAsync<T>(string clusterId, string path, object body);
    Task DeleteAsync(string clusterId, string path);
}
```

### MinGo.Qap.Agent

#### Responsibilities
1. Quartz Scheduler 管理
2. Job 类型注册
3. JobDefinition → Quartz 对象转换
4. HTTP API 接收平台请求
5. 心跳上报

#### Internal Components
```
Agent
├── Configuration
│   ├── AgentConfig
│   └── ConfigLoader
├── Controllers
│   ├── JobsController
│   └── HealthController
├── Services
│   ├── IQuartzService
│   ├── JobRegistry
│   ├── JobConverter
│   └── HeartbeatService
└── Quartz
    └── SchedulerInitializer
```

#### Key Service Interfaces

```csharp
public interface IQuartzService
{
    Task<JobDetailDto> CreateJobAsync(CreateJobRequest request);
    Task UpdateJobAsync(string jobKey, UpdateJobRequest request);
    Task DeleteJobAsync(string jobKey);
    Task TriggerJobAsync(string jobKey);
    Task PauseJobAsync(string jobKey);
    Task ResumeJobAsync(string jobKey);
    Task<JobDetailDto?> GetJobAsync(string jobKey);
    Task<IEnumerable<JobDetailDto>> GetJobsAsync(JobQuery query);
    Task<SchedulerState> GetSchedulerStateAsync();
}

public interface IJobRegistry
{
    void Register(JobManifest manifest);
    JobManifest? Get(string jobType);
    IEnumerable<JobManifest> GetAll();
}

public interface IJobConverter
{
    IJobDetail ConvertToDetail(CreateJobRequest request, JobManifest manifest);
    ITrigger ConvertToTrigger(Schedule schedule);
}
```

## Data Flow

### 1. Create Job

```
User → Platform → Agent → Quartz

1. User POST /api/clusters/{id}/jobs
   {
       "jobKey": "daily-inventory",
       "jobType": "inventory-sync",
       "params": { "warehouseId": "WH001" },
       "schedule": { "type": "cron", "expression": "0 0 2 * * ?" },
       "options": { "disallowConcurrentExecution": true }
   }

2. Platform validates request
3. Platform records JobDefinition (status: pending)
4. Platform forwards to Agent POST /api/jobs
5. Agent validates against JobRegistry
6. Agent converts to Quartz objects
7. Agent calls scheduler.ScheduleJob(job, trigger, replace: true)
8. Agent returns success
9. Platform updates status to synced
```

### 2. Query Jobs

```
User → Platform → Agent (realtime)

1. User GET /api/clusters/{id}/jobs
2. Platform forwards to Agent GET /api/jobs
3. Agent queries Quartz scheduler.GetJobGroupNames()
4. Agent builds response from real-time data
5. Platform returns to user (no caching)
```

### 3. Heartbeat

```
Agent → Platform (every 30s)

1. Agent HeartbeatService runs every 30s
2. Collects: scheduler status, job counts, system metrics
3. POST /api/clusters/{id}/heartbeat
4. Platform updates Cluster.LastHeartbeat
5. Platform calculates status: Online/Warning/Offline
```

## State Machine

### Cluster Status

```
                    ┌─────────┐
            ┌───────│ Pending │───────┐
            │       └────┬────┘       │
     (register)          │            │
            │     (first heartbeat)   │
            │            ▼             │
            │       ┌─────────┐       │
            └──────▶│ Online  │◀──────┘
                    └────┬────┘
                         │ (60s no heartbeat)
                         ▼
                    ┌─────────┐
                    │ Warning │──────┐
                    └────┬────┘      │
                         │ (90s no heartbeat)
                         ▼
                    ┌─────────┐
                    │ Offline │
                    └─────────┘
```

### JobDefinition Status (Platform)

```
┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐
│ Pending │───▶│ Synced  │───▶│ Failed  │◀───│ Timeout │
└─────────┘    └─────────┘    └─────────┘    └─────────┘
      │              │              ▲
      └──────────────┴──────────────┘
              (retry / update)
```

## Data Models

### Platform Entities

```csharp
public class Cluster
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Env { get; set; } = string.Empty;
    public string AgentUrl { get; set; } = string.Empty;
    public ClusterStatus Status { get; set; } = ClusterStatus.Pending;
    public DateTime? LastHeartbeat { get; set; }
    public string? TokenHash { get; set; } // API token hash
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}

public class JobDefinition
{
    public string Id { get; set; } = string.Empty;
    public string ClusterId { get; set; } = string.Empty;
    public string JobKey { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public string Params { get; set; } = "{}"; // JSON
    public string Schedule { get; set; } = "{}"; // JSON
    public string Options { get; set; } = "{}"; // JSON
    public SyncStatus Status { get; set; } = SyncStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
```

### Shared DTOs

```csharp
// Heartbeat
public class HeartbeatDto
{
    public DateTime Timestamp { get; set; }
    public string AgentVersion { get; set; } = string.Empty;
    public long UptimeSeconds { get; set; }
    public string SchedulerStatus { get; set; } = string.Empty;
    public JobCountsDto Jobs { get; set; } = new();
    public SystemMetricsDto System { get; set; } = new();
}

public class JobCountsDto
{
    public int Total { get; set; }
    public int Normal { get; set; }
    public int Paused { get; set; }
    public int Blocked { get; set; }
    public int Executing { get; set; }
}

// Job Manifest
public class JobManifestDto
{
    public string ClusterId { get; set; } = string.Empty;
    public List<JobTypeInfoDto> Jobs { get; set; } = new();
}

public class JobTypeInfoDto
{
    public string Key { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ParameterInfoDto> Parameters { get; set; } = new();
}

public class ParameterInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // string, int, bool, datetime
    public bool Required { get; set; }
    public object? Default { get; set; }
    public string? Label { get; set; }
}

// Job Operations
public class CreateJobRequest
{
    public string JobKey { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public Dictionary<string, object> Params { get; set; } = new();
    public ScheduleDto Schedule { get; set; } = new();
    public QuartzOptionsDto Options { get; set; } = new();
}

public class ScheduleDto
{
    public string Type { get; set; } = string.Empty; // once, cron, interval
    public string? CronExpression { get; set; }
    public int? IntervalSeconds { get; set; }
    public DateTime? RunAt { get; set; }
}

public class QuartzOptionsDto
{
    public bool DisallowConcurrentExecution { get; set; } = false;
    public string MisfirePolicy { get; set; } = "FireAndProceed";
}
```

## Error Handling

### Idempotent Operations

```
JobKey is unique within cluster.

CreateJob with replace=true:
- If Job not exists → Create
- If Job exists → Update (replace)

Result: Idempotent. Retrying same request is safe.
```

### Failure Scenarios

```
1. Platform receives request → validates → records backup → forwards
2. If Agent timeout/unreachable → Platform marks status Timeout
3. If Agent returns error → Platform records error, marks Failed
4. User can retry: same JobKey triggers replace
```

## Security

### V1 Scope
- Platform: Basic Auth or none (rely on network isolation)
- Platform → Agent: Network isolation (internal network only)
- No Token/API Key between Platform and Agent

### Future
- Add API Key for Agent authentication
- Add mTLS for stronger security
- Add RBAC for user permissions

## Configuration

### Platform

```json
{
  "ConnectionStrings": {
    "PlatformDb": "Server=..."
  },
  "Authentication": {
    "Type": "Basic",
    "Username": "${ADMIN_USER}",
    "Password": "${ADMIN_PASS}"
  },
  "Heartbeat": {
    "TimeoutWarningSeconds": 60,
    "TimeoutOfflineSeconds": 90
  }
}
```

### Agent

```yaml
agent:
  id: "agent-a"
  clusterId: "cls-001"
  port: 8080

platform:
  url: "http://platform.internal:5000"
  
quartz:
  assemblyPath: "/app/jobs/"
  jobTypes:
    - "Sample.Jobs.InventorySyncJob"
    - "Sample.Jobs.OrderSyncJob"
  properties:
    quartz.scheduler.instanceName: "ClusterA"
    quartz.jobStore.type: "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz"
    quartz.jobStore.driverDelegateType: "Quartz.Impl.AdoJobStore.SqlServerDelegate, Quartz"
    quartz.jobStore.dataSource: "quartzDs"
    quartz.dataSource.quartzDs.connectionString: "${QUARTZ_CONNECTION_STRING}"
    quartz.dataSource.quartzDs.provider: "SqlServer"

logging:
  level: "Information"
```

## UI Design Principles

### Layout
- Compact information density
- Card-based layout with tight spacing
- Dark theme default (operators-friendly)

### Components
- **Cluster Cards**: Small cards showing cluster name, status dot, job count
- **Job Table**: Dense table with minimal padding, status badges
- **Job Detail**: Split view - left info/actions, right parameters

### Color Scheme
```
Status:
  Normal: #22c55e (green)
  Paused: #f59e0b (amber)
  Offline: #6b7280 (gray)
  Error: #ef4444 (red)

Background:
  Primary: #0f172a (slate-900)
  Card: #1e293b (slate-800)
  
Text:
  Primary: #f8fafc (slate-50)
  Secondary: #94a3b8 (slate-400)
```

## Testing Strategy

### Unit Tests
- JobConverter: Test conversion logic
- JobRegistry: Test registration and lookup
- HeartbeatHandler: Test status calculation

### Integration Tests
- Platform ↔ Agent communication
- Agent ↔ Quartz operations
- End-to-end job lifecycle

### Sample Jobs
```csharp
[QuartzJob(
    Key = "test-echo",
    Description = "Echo job for testing"
)]
public class EchoJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var message = context.MergedJobDataMap["message"]?.ToString() ?? "Hello";
        Console.WriteLine($"[Echo] {message} at {DateTime.Now}");
        await Task.CompletedTask;
    }
}
```

## Deployment

### Docker Compose

```yaml
# deploy/platform/docker-compose.yml
version: '3.8'
services:
  platform:
    image: mango/qap-platform:latest
    ports:
      - "5000:80"
    environment:
      - ConnectionStrings__PlatformDb=${PLATFORM_DB}
      - Authentication__Password=${ADMIN_PASS}

# deploy/agent/docker-compose.yml
version: '3.8'
services:
  agent:
    image: mango/qap-agent:latest
    ports:
      - "8080:80"
    volumes:
      - ./config.yaml:/app/config.yaml:ro
      - ./jobs:/app/jobs:ro
```
