# System Architecture

## Purpose

Define the overall architecture design specification for MinGo.QuartzManager project, serving as the foundation for all implementation work and preventing scope creep.

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-04-30 | Agent-Scheduler platform refactor: removed Cluster, added Identity persistence, Scheduler routing, DateTimeOffset migration |
| 1.1.0 | 2026-04-24 | Added Agent as Library project, shared contracts, Quartz.NET wrapper architecture |
| 1.0.0 | 2026-04-24 | Initial architecture specification |

---

## Component Architecture (v2.0.0)

```
┌────────────────────────────────────────────────────────────────────────┐
│                         Solution Structure                              │
├────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌──────────────────┐      ┌───────────────────────────────────────┐   │
│  │ MinGo.Qap.Shared │◄─────│          MinGo.Qap.Platform          │   │
│  │  (Class Library) │      │          (ASP.NET Core Web API)       │   │
│  │                  │      │                                       │   │
│  │ • DTOs           │      │ ┌──────────┐ ┌──────────┐ ┌────────┐ │   │
│  │ • Enums          │      │ │Agents    │ │Schedulers│ │ Jobs   │ │   │
│  │ • Interfaces     │      │ │Controller│ │Controller│ │Cntrlr  │ │   │
│  └────────┬─────────┘      │ └────┬─────┘ └────┬─────┘ └───┬────┘ │   │
│           │                │      │             │           │       │   │
│           │                │ ┌────▼─────────────▼───────────▼────┐  │   │
│           │                │ │        SchedulerRouterService     │  │   │
│           │                │ │    (SchedulerName → Agent路由)    │  │   │
│           │                │ └────────────────┬─────────────────┘  │   │
│           │                │                  │ HTTP Proxy         │   │
│           │                └──────────────────┼────────────────────┘   │
│           │                                   │                        │
│           │                         ┌─────────▼──────────┐            │
│           │                         │  AgentProxyService  │            │
│           │                         │  (X-Scheduler-Name) │            │
│           │                         └─────────┬──────────┘            │
│           │                                   │                        │
│           │              ┌────────────────────┼──────────────────┐    │
│           │              │    MinGo.Qap.Agent │(Class Library)   │    │
│           │              │                    │                  │    │
│           │              │  ┌─────────────────▼──────────────┐   │    │
│           └──────────────┤  │   HostedAgentService          │   │    │
│                          │  │  • Load identity (file)       │   │    │
│                          │  │  • Register (POST /api/agents)│   │    │
│                          │  │  • Save identity              │   │    │
│                          │  │  • Report schedulers           │   │    │
│                          │  │  • Heartbeat loop             │   │    │
│                          │  └────────────────┬──────────────┘   │    │
│                          │                   │                    │    │
│                          │  ┌────────────────▼──────────────┐   │    │
│                          │  │  IAgentSchedulerAccessor     │   │    │
│                          │  │  • AgentSchedulerAccessor    │   │    │
│                          │  │  • DeferredSchedulerAccessor │   │    │
│                          │  └────────────────┬──────────────┘   │    │
│                          │                   │                    │    │
│                          │  ┌────────────────▼──────────────┐   │    │
│                          │  │   Quartz.NET Scheduler(s)    │   │    │
│                          │  │   (One or more IScheduler)   │   │    │
│                          │  └──────────────────────────────┘   │    │
│                          └─────────────────────────────────────┘    │
│                                                                         │
│  ┌───────────────────────┐         ┌─────────────────────────┐       │
│  │   MinGo.Qap.UI        │         │   Consumer App          │       │
│  │   (React + TypeScript)│         │   (Adds Agent package)  │       │
│  │                       │         │                         │       │
│  │ • AgentsPage          │         │ • IScheduler(s) from DI │       │
│  │ • AgentDetailPage     │         │ • Multiple Scheduler    │       │
│  │ • SchedulersPage      │         │   support out-of-box    │       │
│  │ • SchedulerDetailPage │         │                         │       │
│  │ • JobsPage            │         │                         │       │
│  │ • JobDetailPage       │         │                         │       │
│  └───────────────────────┘         └─────────────────────────┘       │
│                                                                         │
└────────────────────────────────────────────────────────────────────────┘
```

### Key Changes in v2.0.0

| Change | Description |
|--------|-------------|
| **Remove Cluster** | Cluster concept removed from Platform, Agent, Shared, UI |
| **Agent Identity** | AgentId persisted locally (agent-identity.json), survives restart |
| **Scheduler Discovery** | `IAgentSchedulerAccessor` discovers all IScheduler instances in host |
| **Scheduler Reporting** | Agent reports Quartz runtime info to Platform on startup |
| **Scheduler Routing** | Job operations routed by schedulerName, not clusterId |
| **DateTimeOffset** | All time fields migrated to DateTimeOffset with UTC enforcement |
| **EF Core UTC** | Global timestamptz convention + Value Converter + save interceptor |

---

## Project Responsibilities (v2.0.0)

### MinGo.Qap.Shared
| Responsibility |
|---------------|
| Data transfer objects (DTOs) shared between Platform and Agent |
| Interface definitions (IAgentSchedulerAccessor, IAgentIdentityStore) |
| Enumerations (SyncStatus, ScheduleType) |
| REST API contract models |
| Time field conventions (DateTimeOffset, ISO 8601) |

### MinGo.Qap.Platform
| Responsibility |
|---------------|
| Agent CRUD API (register, heartbeat, list, delete) |
| Scheduler info management (report, query) |
| Scheduler → Agent routing (SchedulerRouterService) |
| Job operation forwarding via AgentProxyService |
| UTC audit interceptor for all DateTimeOffset fields |
| 301 redirect from old Cluster endpoints |
| Reference Shared library |

### MinGo.Qap.Agent (Library)
| Responsibility |
|---------------|
| Scheduler discovery via IAgentSchedulerAccessor |
| Agent identity persistence (IAgentIdentityStore → file) |
| Scheduler runtime info reporting (SchedulerReporterService) |
| Registration: POST /api/agents (supports reconnect) |
| Heartbeat with scheduler status summary |
| Job creation/triggering via QuartzService (schedulerName-aware) |
| API endpoints: schedulerName routing via X-Scheduler-Name header |
| Reference Shared library |
| Wraps Quartz.NET Scheduler(s) |

### Consumer Application
| Responsibility |
|---------------|
| Add MinGo.Qap.Agent NuGet package |
| Register IScheduler(s) in DI (single or multiple) |
| Agent auto-discovers all IScheduler instances |
| Agent handles registration, reporting, heartbeat automatically |

---

## Technology Stack (v2.0.0)

| Component | Technology | Version |
|-----------|-----------|---------|
| Scheduling framework | Quartz.NET | 3.17.1 |
| Web framework | ASP.NET Core | 10.0 |
| Database | PostgreSQL | 15+ |
| ORM | Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.1 |
| Target framework | .NET | 10.0 |
| Frontend | React + TypeScript + Vite | Latest |
| UI Libraries | @tanstack/react-query, lucide-react | Latest |

---

## Design Principles (v2.0.0)

| # | Principle |
|---|-----------|
| 1 | **Agent is a Library, not a Web App** - No Kestrel, no Swagger UI |
| 2 | **Agent wraps Quartz.NET** - Thin wrapper adding metadata |
| 3 | **Multi-Scheduler support** - Agent discovers all IScheduler instances |
| 4 | **No Platform dependency** - Agent is completely standalone |
| 5 | **Shared contracts via Shared project** - No duplicated DTOs |
| 6 | **Agent identity persists** - Survives restarts via local file |
| 7 | **Scheduler-centric routing** - Operations routed by schedulerName |
| 8 | **All time fields UTC** - DateTimeOffset + timestamptz + save interceptor |

---

## REST API Contracts (v2.0.0)

### Agent APIs

| Method | Path | Purpose |
|--------|------|---------|
| POST | /api/agents | Register/reconnect Agent |
| GET | /api/agents | List all agents |
| GET | /api/agents/{agentId} | Agent detail + schedulers |
| DELETE | /api/agents/{agentId} | Soft delete agent |
| POST | /api/agents/{agentId}/heartbeat | Agent heartbeat + scheduler status |
| POST | /api/agents/{agentId}/schedulers | Report scheduler info |
| GET | /api/agents/{agentId}/schedulers | Query agent's schedulers |

### Scheduler APIs

| Method | Path | Purpose |
|--------|------|---------|
| GET | /api/schedulers | List all schedulers |
| GET | /api/schedulers/{name} | Scheduler detail + agents |
| GET | /api/schedulers/{name}/agents | List agents for scheduler |

### Job APIs

| Method | Path | Purpose |
|--------|------|---------|
| GET | /api/schedulers/{name}/jobs | List jobs |
| POST | /api/schedulers/{name}/jobs | Create job |
| GET | /api/schedulers/{name}/jobs/{key} | Get job detail |
| PUT | /api/schedulers/{name}/jobs/{key} | Update job |
| DELETE | /api/schedulers/{name}/jobs/{key} | Delete job |
| POST | /api/schedulers/{name}/jobs/{key}/trigger | Trigger job |
| POST | /api/schedulers/{name}/jobs/{key}/pause | Pause job |
| POST | /api/schedulers/{name}/jobs/{key}/resume | Resume job |

### Manifest APIs

| Method | Path | Purpose |
|--------|------|---------|
| GET | /api/schedulers/{name}/manifest | Get job manifest |
| POST | /api/schedulers/{name}/manifest | Report job manifest |

---

## Agent NuGet Package Structure (v2.0.0)

```
MinGo.Qap.Agent/
├── MinGo.Qap.Agent.csproj
├── AgentApiExtensions.cs           # Minimal API endpoints + schedulerName parsing
├── AgentExtensions.cs              # DI registration + SchedulerAccessor setup
├── Services/
│   ├── IAgentSchedulerAccessor.cs      # Scheduler discovery interface
│   ├── AgentSchedulerAccessor.cs       # Default implementation
│   ├── DeferredSchedulerAccessor.cs    # Lazy discovery
│   ├── IAgentIdentityStore.cs          # Identity persistence interface
│   ├── AgentIdentityFileStore.cs       # File-based identity store
│   ├── SchedulerReporterService.cs     # Scheduler info reporter
│   ├── HostedAgentService.cs           # Lifecycle management
│   ├── AgentRegistrationService.cs     # Registration service
│   ├── QuartzService.cs                # Quartz operations (multi-scheduler)
│   ├── JobDiscoveryService.cs          # Assembly scanning
│   ├── JobRegistry.cs                  # Job type registry
│   ├── JobConverter.cs                 # Job/trigger conversion
│   └── ... (other services)
├── Configuration/
│   ├── AgentConfig.cs                  # Configuration options
│   ├── ConfigureAgentConfigOptions.cs  # Options setup
│   └── ... (other config)
└── README.md
```

---

## Integration Flow (v2.0.0)

### Agent Startup Sequence

```
Application Start
       │
       ▼
IAgentSchedulerAccessor Initializes
       │
       ▼
HostedAgentService Starts
       │
       ├── Phase 1: Load identity from agent-identity.json
       │   ├── Found AgentId → reconnect
       │   └── No AgentId → first registration
       │
       ▼
Phase 2: POST /api/agents
  { agentId?, name, url, agentVersion, startedAt }
       │
       ▼
← { agentId, token, heartbeatIntervalSeconds, ... }
       │
       ▼
Phase 3: Save agent-identity.json (persist AgentId)
       │
       ▼
Phase 4: IAgentSchedulerAccessor.GetAll() → all IScheduler instances
       │   → Extract metadata from each scheduler
       ▼
POST /api/agents/{agentId}/schedulers (report all schedulers)
       │
       ▼
Phase 5: Heartbeat loop
  POST /api/agents/{agentId}/heartbeat
  { schedulerSummaries: [...] }
```

### Scheduler Name Resolution (Agent API)

Requests from Platform to Agent resolve the target scheduler via:
1. `X-Scheduler-Name` HTTP header
2. `?schedulerName=` query parameter
3. Default first scheduler

---

## Database Schema (v2.0.0)

### New Tables (Replacing Cluster/AgentInstance)

**Agents**
| Column | Type | Notes |
|--------|------|-------|
| Id | varchar(64) | PK, agt-xxx format |
| Name | varchar(256) | Display name |
| Url | varchar(512) | Agent HTTP endpoint |
| Status | varchar(32) | Pending/Online/Warning/Offline |
| AgentVersion | varchar(64) | Optional |
| TokenHash | varchar(256) | API token hash |
| LastHeartbeat | timestamptz | UTC |
| LastReportedAt | timestamptz | UTC |
| StartedAt | timestamptz | UTC |
| CreatedAt | timestamptz | UTC |
| UpdatedAt | timestamptz | UTC |
| DeletedAt | timestamptz? | Soft delete |

**SchedulerInfos**
| Column | Type | Notes |
|--------|------|-------|
| Id | varchar(64) | PK, sch-xxx format |
| SchedulerName | varchar(256) | Quartz scheduler name |
| SchedulerInstanceId | varchar(256) | Quartz instance ID |
| Status | varchar(32) | running/standby |
| IsClustered | boolean | |
| RunningSince | timestamptz? | UTC |
| FirstReportedAt | timestamptz | UTC |
| LastReportedAt | timestamptz | UTC |
| UNIQUE(SchedulerName, SchedulerInstanceId) | | |

**AgentSchedulers** (many-to-many)
| Column | Type | Notes |
|--------|------|-------|
| AgentId | varchar(64) | FK → Agents |
| SchedulerInfoId | varchar(64) | FK → SchedulerInfos |
| ReportedAt | timestamptz | UTC |
| PK(AgentId, SchedulerInfoId) | | |

### Time Field Convention

All time fields follow strict UTC convention:
- **Code type**: `DateTimeOffset`
- **Write**: `DateTimeOffset.UtcNow`
- **DB column**: `timestamptz`
- **Enforcement**: EF Core Value Converter + `UtcAuditInterceptor`

---

## Boundary Constraints (v2.0.0)

### Agent without Web Interface
- Agent provides Minimal API via extension method `MapMinGoAgentApi()`
- Consumer app calls `app.MapMinGoAgentApi()` to enable endpoints
- No embedded web server (Kestrel); consumer controls hosting

### Agent without Platform Dependency
- No reference to MinGo.Qap.Platform
- Uses only Shared library for contracts
- Communicates via HTTP REST (no shared DB)

### Multi-Scheduler Support
- Agent discovers ALL IScheduler instances via DI
- Each operation targets a specific scheduler by name
- DeferredSchedulerAccessor handles late scheduler initialization

### UTC Time Convention
- All new time fields: `DateTimeOffset` + `DateTimeOffset.UtcNow`
- Database: `timestamptz`
- API: ISO 8601 UTC string format
- Migration: old `DateTime` fields to be migrated incrementally
