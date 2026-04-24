# System Architecture

## Purpose

Define the overall architecture design specification for MinGo.QuartzManager project, serving as the foundation for all implementation work and preventing scope creep.

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.1.0 | 2026-04-24 | Added Agent as Library project, shared contracts, Quartz.NET wrapper architecture |
| 1.0.0 | 2026-04-24 | Initial architecture specification |

---

## ADDED Requirements (v1.1.0)

### Requirement: Agent is a NuGet Library Package

The Agent component SHALL be distributed as a NuGet package that can be added to any .NET application using Quartz.NET.

#### Scenario: Agent integration via NuGet
- **GIVEN** a .NET application with Quartz.NET
- **WHEN** the developer adds the `MinGo.Agent` NuGet package
- **THEN** the application gains Agent capabilities without inheriting Platform dependencies
- **AND** configuration is done through Quartz.NET's standard configuration mechanism

#### Scenario: Agent as pure library
- **GIVEN** Agent package is referenced
- **WHEN** the host application starts
- **THEN** Agent provides Job discovery, execution, and logging capabilities
- **AND** Agent does NOT expose any HTTP endpoints or user-facing interfaces
- **AND** Agent does NOT require its own WebHost/Kestrel

### Requirement: Agent Wraps Quartz.NET Instance

The Agent component SHALL be a thin wrapper around a Quartz.NET Scheduler instance.

#### Scenario: Quartz.NET as foundation
- **GIVEN** a running Quartz.NET Scheduler instance
- **WHEN** Agent is configured
- **THEN** Agent wraps the existing Scheduler without replacing it
- **AND** Agent provides additional metadata, logging, and Platform integration capabilities
- **AND** Agent respects all Quartz.NET configurations (clustering, job stores, thread pools)

#### Scenario: Agent lifecycle tied to Scheduler
- **GIVEN** Agent is added to an application
- **WHEN** the Quartz.NET Scheduler starts/shuts down
- **THEN** Agent lifecycle is managed alongside the Scheduler
- **AND** Agent heartbeat stops when Scheduler is shut down
 
### Requirement: Agent does not depend on specific JobStore backend

The Agent component SHALL be agnostic to the Quartz.NET JobStore backend used by the host application.

#### Scenario: Host configures arbitrary JobStore
- **GIVEN** the host configures Quartz to use a specific JobStore (e.g., PostgreSQL, SQL Server, SQLite)
- **WHEN** the Agent library is integrated as a host library
- **THEN** the Agent operates without knowledge of the concrete JobStore implementation
- **AND** the host may swap JobStore backend without modifying the Agent

### Constraint: Minimal API exposure restricted to intranet

The Minimal API endpoints exposed by the Agent model SHALL be accessible only from internal networks (intranet). Public exposure is not allowed.

#### Scenario: Intranet-only access
- **GIVEN** the Worker application is deployed in an internal network
- **WHEN** an internal host accesses the Agent Minimal API endpoints
- **THEN** access is allowed
- **AND** requests from outside the intranet SHOULD be rejected (403 Forbidden)

### Requirement: Agent Uses Quartz.NET Dependency Injection

The Agent component SHALL integrate with Quartz.NET's dependency injection system.

#### Scenario: Quartz DI integration
- **GIVEN** an application using Quartz.NET's standard DI configuration
- **WHEN** Agent package is added
- **THEN** Agent registers its services through Quartz.Extensions.DependencyInjection
- **AND** Agent services follow Quartz's service lifetime conventions

#### Scenario: Job registration via Quartz API
- **GIVEN** Agent's job discovery finds IJob implementations
- **WHEN** jobs need to be registered
- **THEN** Agent uses `IScheduler.AddJob()` and `IScheduler.TriggerJob()` APIs
- **AND** Agent does NOT bypass Quartz APIs or access internal implementations

### Requirement: Agent Does NOT Depend on Platform

The Agent component SHALL have zero dependencies on Platform.

#### Scenario: No Platform dependency
- **GIVEN** Agent source code
- **WHEN** examining references and using statements
- **THEN** there SHALL be NO references to MinGo.Qap.Platform
- **AND** there SHALL be NO references to PlatformDbContext
- **AND** there SHALL be NO HTTP clients pointing to Platform URLs

#### Scenario: Agent self-contained
- **GIVEN** Agent NuGet package
- **WHEN** installing in a new project
- **THEN** only Quartz.NET and Agent dependencies are pulled in
- **AND** Platform is NOT a transitive dependency

### Requirement: Shared Contracts Library

Platform and Agent SHALL share data contracts through a common library.

#### Scenario: Shared contracts project
- **GIVEN** `MinGo.Qap.Shared` library
- **WHEN** Platform and Agent reference this library
- **THEN** both can use shared DTOs, interfaces, and data models
- **AND** no duplication of contract definitions exists

#### Scenario: Contract boundaries
- **GIVEN** the shared library
- **WHEN** defining API contracts between Platform and Agent
- **THEN** contracts SHALL include:
  - JobDefinitionDto (job metadata)
  - ExecutionLogDto (log reporting)
  - AgentRegistrationRequest/Response
  - HeartbeatDto
  - ClusterManifestDto (job type list)

#### Scenario: Platform-specific vs Shared
- **GIVEN** code in Platform or Agent
- **WHEN** determining where to place a type
- **THEN** types used by BOTH SHALL go in Shared
- **AND** types used by Platform only SHALL stay in Platform
- **AND** types used by Agent only SHALL stay in Agent

---

## Component Architecture (Updated)

```
┌─────────────────────────────────────────────────────────────────────┐
│                    Solution Structure                                │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌─────────────────────┐         ┌─────────────────────┐          │
│  │   MinGo.Qap.Shared  │◄────────│  MinGo.Qap.Platform │          │
│  │   (Class Library)   │         │   (ASP.NET Core)     │          │
│  │                     │         │                     │          │
│  │  • DTOs             │         │  • Web API          │          │
│  │  • Interfaces       │         │  • Dashboard        │          │
│  │  • Data Contracts   │         │  • Agent Management │          │
│  │  • Enums            │         │                     │          │
│  └─────────────────────┘         └─────────────────────┘          │
│           ▲                                  │                       │
│           │              ┌──────────────────┘                       │
│           │              │ Proxy / REST API                          │
│           │              ▼                                           │
│           │    ┌─────────────────────┐                             │
│           └────│   MinGo.Qap.Agent   │                             │
│                │   (Class Library)   │                             │
│                │                     │                             │
│                │  • Job Discovery     │                             │
│                │  • Job Registration  │                             │
│                │  • Log Collection   │                             │
│                │  • Platform Client  │                             │
│                │  • Heartbeat Service │                             │
│                └─────────────────────┘                             │
│                           │                                          │
│                           │ Wraps                                    │
│                           ▼                                          │
│                ┌─────────────────────┐                             │
│                │    Quartz.NET       │                             │
│                │   Scheduler Instance│                             │
│                │                     │                             │
│                │  • Job Store (DB)   │                             │
│                │  • Thread Pool      │                             │
│                │  • Clustering       │                             │
│                └─────────────────────┘                             │
│                                                                     │
│  ┌─────────────────────┐         ┌─────────────────────┐          │
│  │   MinGo.Qap.UI      │◄────────│   Consumer App       │          │
│  │   (React/Vite)      │         │   (Adds Agent pkg)   │          │
│  └─────────────────────┘         └─────────────────────┘          │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### Project Responsibilities

#### MinGo.Qap.Shared
| Responsibility |
|---------------|
| Data transfer objects (DTOs) shared between Platform and Agent |
| Interface definitions (IAgentRegistry, ILogReporter) |
| Enumerations (JobStatus, AgentStatus, ClusterStatus) |
| REST API contract models |
| JSON serialization attributes |
| Validation attributes |

#### MinGo.Qap.Platform
| Responsibility |
|---------------|
| User Web API (CRUD operations) |
| JobDefinition metadata backup |
| Execution log aggregation |
| Cluster/AgentInstance management |
| Agent proxy forwarding |
| NOT directly operating Quartz |
| Reference Shared library |

#### MinGo.Qap.Agent (Library)
| Responsibility |
|---------------|
| Job discovery via assembly scanning |
| Job registration to Quartz |
| Scheduling execution via Quartz.NET |
| Log collection and reporting to Platform |
| Independent Quartz database |
| Heartbeat to Platform |
| NO user-facing UI |
| NO HTTP endpoints |
| Reference Shared library |
| Wraps Quartz.NET Scheduler |

#### Consumer Application
| Responsibility |
|---------------|
| Add MinGo.Qap.Agent NuGet package |
| Configure Quartz.NET (including Agent configuration) |
| Run Quartz.NET Scheduler |
| Agent automatically handles registration, discovery, and reporting |

---

## Technology Stack (Updated)

| Component | Technology | Version |
|-----------|-----------|---------|
| Scheduling framework | Quartz.NET | 3.17.1 |
| Web framework | ASP.NET Core | 10.0 |
| Persistence | PostgreSQL | - |
| Target framework | .NET | 10.0 |
| Agent packaging | NuGet | - |
| DI integration | Quartz.Extensions.DependencyInjection | - |

---

## Design Principles (Enhanced)

| # | Principle |
|---|-----------|
| 1 | **Agent is a Library, not a Web App** - No Kestrel, no Swagger UI, no HTTP listeners |
| 2 | **Agent wraps Quartz.NET** - Thin wrapper adding metadata, not replacing functionality |
| 3 | **Quartz.NET DI first** - Agent integrates via Quartz's dependency injection extensions |
| 4 | **No Platform dependency** - Agent is completely standalone |
| 5 | **Shared contracts via Shared project** - No duplicated DTOs or interfaces |
| 6 | **Agent lifecycle = Scheduler lifecycle** - Automatic registration/unregistration |
| 7 | **Data isolation - Quartz(DB) ⟂ Platform(DB)** - Independent via REST |
| 8 | **Cluster is a group of Agents with same execution capability** |
| 9 | **Agent is an executor instance that registers with Platform** |

---

## REST API Contracts

### Agent → Platform APIs

| Method | Path | Purpose |
|--------|-----|---------|
| POST | /api/clusters/{clusterId}/agents | Register Agent instance |
| POST | /api/agents/{agentId}/heartbeat | Heartbeat |
| POST | /api/logs | Report execution logs |
| GET | /api/manifest | Get job type list |

### Platform → Agent APIs

| Method | Path | Purpose |
|--------|-----|---------|
| POST | /jobs | Create job |
| PUT | /jobs/{key} | Update job |
| DELETE | /jobs/{key} | Delete job |
| POST | /jobs/{key}/trigger | Manual trigger |
| POST | /jobs/{key}/pause | Pause job |
| POST | /jobs/{key}/resume | Resume job |

---

## Agent NuGet Package Structure

```
MinGo.Qap.Agent/
├── MinGo.Qap.Agent.csproj          # Library project, net10.0
├── Services/
│   ├── JobDiscoveryService.cs      # Assembly scanning
│   ├── JobRegistrationService.cs   # Register with Quartz
│   ├── LogCollectionService.cs      # Collect and report logs
│   └── HeartbeatService.cs          # Platform heartbeat
├── Configuration/
│   ├── AgentOptions.cs             # Configuration options
│   └── AgentExtensions.cs          # DI registration
├── Contracts/
│   └── (uses Shared library)        # No local contracts
├── Quartz/
│   ├── QuartzAgentModule.cs        # Quartz module (wrapper)
│   └── JobWrapper.cs               # Wrapped job execution
└── README.md                        # Integration guide
```

---

## Integration Flow

### Application Adding Agent

```csharp
// In Program.cs of consumer application
services.AddQuartzHostedService();
services.AddMinGoAgent();  // Agent configures itself

// appsettings.json
{
  "Agent": {
    "ClusterId": "cls-001",
    "PlatformUrl": "http://platform:5000",
    "Token": "agent-secret"
  }
}
```

### Agent Startup Sequence

```
Application Start
       │
       ▼
Quartz Scheduler Starts
       │
       ▼
Agent JobDiscoveryService Scans Assemblies
       │
       ▼
Agent Registers with Platform
       │
       ▼
Agent Registers Discovered Jobs with Quartz
       │
       ▼
Agent Starts Heartbeat Service
       │
       ▼
Jobs Execute → Logs Reported to Platform
```

---

## Boundary Constraints (Updated)

### Requirement: Agent without Web Interface
- **GIVEN** Agent package
- **WHEN** examining the built artifact
- **THEN** there SHALL be NO controllers, NO routing, NO Kestrel configuration
- **AND** Agent is purely a service library

### Requirement: Agent without Platform Dependency
- **GIVEN** Agent source code
- **WHEN** checking dependencies
- **THEN** there SHALL be NO reference to MinGo.Qap.Platform
- **AND** Agent uses only Shared library for contracts

### Requirement: Quartz.NET is the Foundation
- **GIVEN** an application with Agent
- **WHEN** scheduling jobs
- **THEN** all scheduling goes through Quartz.NET APIs
- **AND** Agent does NOT provide alternative scheduling mechanisms

### Requirement: Fixed scheduling strategies
- **GIVEN** job scheduling configuration
- **WHEN** user configures schedule
- **THEN** only three types are supported: cron, interval, once
- **AND** custom scheduling strategies are NOT allowed

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2026-04-24 | Initial architecture specification |
