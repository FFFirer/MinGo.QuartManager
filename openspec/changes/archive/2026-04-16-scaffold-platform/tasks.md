# Tasks: MinGo.Qap Platform

## Phase 1: Foundation ✅

### P1-1: Create Project Structure ✅
**Priority: High**

- [x] Create solution file `MinGo.Qap.sln`
- [x] Create project `MinGo.Qap.Shared`
  - [x] Define shared models (DTOs, Enums)
  - [x] Add JSON serialization utilities
  - [~] Add validation helpers (V2)
- [x] Create project `MinGo.Qap.Agent`
  - [x] Setup ASP.NET Core minimal API structure
  - [x] Add Quartz.NET dependency
  - [x] Add configuration model
- [x] Create project `MinGo.Qap.Platform`
  - [x] Setup ASP.NET Core MVC/WebAPI
  - [x] Add EF Core dependencies
  - [x] Setup basic controller structure
- [x] Create project `MinGo.Qap.UI` (React)
  - [x] Initialize React + TypeScript + Vite
  - [x] Setup TailwindCSS
  - [~] Setup shadcn/ui (V2)
  - [x] Add TanStack Query and Table

**Acceptance Criteria:**
- [x] All projects build successfully
- [x] Solution compiles without errors
- [x] Basic project references established

---

### P1-2: Define Shared Models ✅
**Priority: High**
**Depends: P1-1**

In `MinGo.Qap.Shared`:

- [x] Define enums
  - [x] `ClusterStatus`: Pending, Online, Warning, Offline, Deleted
  - [x] `SyncStatus`: Pending, Synced, Failed, Timeout
  - [x] `ScheduleType`: Once, Cron, Interval
  - [x] `MisfirePolicy`: FireAndProceed, IgnoreMisfire, DoNothing
- [x] Define DTOs
  - [x] `ClusterDto`, `CreateClusterRequest`, `ClusterSummary`
  - [x] `JobDefinitionDto`, `CreateJobRequest`, `UpdateJobRequest`
  - [x] `JobManifestDto`, `JobTypeInfoDto`, `ParameterInfoDto`
  - [x] `ScheduleDto`, `QuartzOptionsDto`
  - [x] `HeartbeatDto`, `JobCountsDto`, `SystemMetricsDto`
- [x] Add validation attributes

**Acceptance Criteria:**
- [x] All shared models defined
- [x] JSON serialization configured (camelCase, UTC)
- [~] Unit tests for validation logic (V2)

---

### P1-3: Agent Configuration System ✅
**Priority: High**
**Depends: P1-2**

In `MinGo.Qap.Agent`:

- [x] Create `AgentConfig` class
  - [x] Agent settings (id, clusterId, port)
  - [x] Platform settings (url)
  - [x] Quartz settings (assemblyPath, jobTypes, properties)
- [x] Create `ConfigLoader` service
  - [x] Load from YAML file
  - [x] Support environment variables override
  - [x] Validate required fields
- [x] Create sample `config.yaml`
- [x] Add configuration validation on startup

**Acceptance Criteria:**
- [x] Agent loads configuration on startup
- [x] Invalid config shows clear error message
- [x] Environment variables properly override YAML values

---

## Phase 2: Agent Core ✅

### P2-1: Quartz Scheduler Initialization ✅
**Priority: High**
**Depends: P1-3**

In `MinGo.Qap.Agent`:

- [x] Create `SchedulerInitializer` service
  - [x] Build Quartz scheduler from config properties
  - [x] Support ADO.NET JobStore
  - [x] Handle scheduler startup errors
- [x] Add health check endpoint
  - [x] GET /health returns scheduler status
- [x] Add graceful shutdown
  - [x] Wait for executing jobs on SIGTERM

**Acceptance Criteria:**
- Scheduler starts successfully
- Health endpoint returns status
- Graceful shutdown waits for jobs

---

### P2-2: Job Registry System ✅
**Priority: High**
**Depends: P2-1**

In `MinGo.Qap.Agent`:

- [x] Create `JobManifest` model
- [x] Create `IJobRegistry` interface
- [x] Create `JobRegistry` implementation
  - [x] Load from config (V1: config declaration)
  - [x] Support parameter metadata
- [x] Create manifest endpoint
  - [x] GET /api/jobs/manifest returns registered jobs

**Acceptance Criteria:**
- [x] Agent returns manifest with configured job types
- [x] Each job type has key, description, parameters

---

### P2-3: Job Converter ✅
**Priority: High**
**Depends: P2-2**

In `MinGo.Qap.Agent`:

- [x] Create `IJobConverter` interface
- [x] Create `JobConverter` implementation
  - [x] Convert `CreateJobRequest` → `IJobDetail`
  - [x] Convert `ScheduleDto` → `ITrigger`
  - [x] Handle JobDataMap population from params
  - [x] Support replace: true for idempotency
- [x] Support schedule types
  - [x] Once: SimpleTrigger, fire once
  - [x] Cron: CronTrigger
  - [x] Interval: SimpleTrigger, repeat forever
- [x] Handle options
  - [x] DisallowConcurrentExecution attribute
  - [x] Misfire policy mapping

**Acceptance Criteria:**
- [x] All 3 schedule types convert correctly
- [x] JobDataMap contains all parameters
- [x] Replace flag works for updates

---

### P2-4: Quartz Service Implementation ✅
**Priority: High**
**Depends: P2-3**

In `MinGo.Qap.Agent`:

- [x] Create `IQuartzService` interface
- [x] Create `QuartzService` implementation
  - [x] CreateJobAsync: ScheduleJob with replace
  - [x] UpdateJobAsync: RescheduleJob
  - [x] DeleteJobAsync: DeleteJob
  - [x] TriggerJobAsync: TriggerJob
  - [x] PauseJobAsync: PauseJob
  - [x] ResumeJobAsync: ResumeJob
  - [x] GetJobAsync: GetJobDetail
  - [x] GetJobsAsync: GetJobGroupNames + GetJobKeys
- [x] Create JobsController
  - [x] POST /api/jobs → CreateJob
  - [x] GET /api/jobs → GetJobs
  - [x] GET /api/jobs/{jobKey} → GetJob
  - [x] PUT /api/jobs/{jobKey} → UpdateJob
  - [x] DELETE /api/jobs/{jobKey} → DeleteJob
  - [x] POST /api/jobs/{jobKey}/trigger → TriggerJob
  - [x] POST /api/jobs/{jobKey}/pause → PauseJob
  - [x] POST /api/jobs/{jobKey}/resume → ResumeJob

**Acceptance Criteria:**
- [x] All CRUD operations work
- [x] Trigger/Pause/Resume work
- [x] Controller returns proper HTTP status codes

---

### P2-5: Heartbeat Service ✅
**Priority: Medium**
**Depends: P2-1**

In `MinGo.Qap.Agent`:

- [x] Create `HeartbeatService` (BackgroundService)
  - [x] Run every 30s
  - [x] Collect scheduler metrics
  - [x] Collect job counts
  - [x] POST to Platform
- [x] Create heartbeat model builder
  - [x] Get scheduler status
  - [x] Count jobs by state
  - [x] Get system metrics (memory)

**Acceptance Criteria:**
- [x] Heartbeat sends every 30s
- [x] Contains correct job counts
- [x] Handles Platform unavailability gracefully

---

## Phase 3: Platform Core ✅

### P3-1: Database Setup ✅
**Priority: High**
**Depends: P1-2**

In `MinGo.Qap.Platform`:

- [x] Create `PlatformDbContext`
  - [x] Cluster entity
  - [x] JobDefinition entity
- [~] Create EF Core migrations (auto-migrate in dev)
- [x] Setup connection string configuration
- [ ] Add seed data (optional)

**Acceptance Criteria:**
- [x] Database schema configured
- [x] Entities properly configured

---

### P3-2: Cluster Service ✅
**Priority: High**
**Depends: P3-1**

In `MinGo.Qap.Platform`:

- [x] Create `IClusterService` interface
- [x] Create `ClusterService` implementation
  - [x] CreateAsync: Generate token, persist
  - [x] GetAsync: Retrieve by id
  - [x] GetAllAsync: List with filters
  - [x] UpdateHeartbeatAsync: Update status and metrics
  - [x] DeleteAsync: Soft delete
- [x] Create `ClustersController`
  - [x] POST /api/clusters → Create
  - [x] GET /api/clusters → List
  - [x] GET /api/clusters/{id} → Get
  - [x] DELETE /api/clusters/{id} → Delete
  - [x] POST /api/clusters/{id}/heartbeat → Heartbeat

**Acceptance Criteria:**
- [x] Cluster CRUD works
- [x] Heartbeat updates status correctly
- [x] Token generated on create

---

### P3-3: Agent Proxy Service ✅
**Priority: High**
**Depends: P3-2**

In `MinGo.Qap.Platform`:

- [x] Create `IAgentProxyService` interface
- [x] Create `AgentProxyService` implementation
  - [x] HTTP client configuration per cluster
  - [x] GET/POST/PUT/DELETE methods
  - [x] Timeout handling
  - [x] Error handling
- [x] Handle Agent unavailability
  - [x] Return 503 if Agent offline
  - [x] Log errors

**Acceptance Criteria:**
- [x] Proxy forwards requests correctly
- [x] Proper error handling for offline Agents
- [x] Timeout configurable

---

### P3-4: Job Service ✅
**Priority: High**
**Depends: P3-2, P3-3**

In `MinGo.Qap.Platform`:

- [x] Create `IJobService` interface
- [x] Create `JobService` implementation
  - [x] CreateAsync: Validate, save backup, proxy to Agent
  - [x] UpdateAsync: Save backup, proxy to Agent
  - [x] DeleteAsync: Proxy to Agent, mark deleted
  - [x] GetAsync: Proxy to Agent (realtime)
  - [x] GetByClusterAsync: Proxy to Agent (realtime)
  - [x] Trigger/Pause/Resume: Proxy to Agent
- [x] Handle sync status
  - [x] Mark Pending before proxy
  - [x] Update to Synced on success
  - [x] Update to Failed/Timeout on error

**Acceptance Criteria:**
- [x] All operations proxy correctly
- [x] Status tracked properly
- [x] Error states handled

---

### P3-5: Manifest Controller ✅
**Priority: Medium**
**Depends: P3-4**

In `MinGo.Qap.Platform`:

- [x] Create `ManifestController`
  - [x] POST /api/clusters/{id}/manifest → Store manifest
  - [x] GET /api/clusters/{id}/manifest → Get manifest
- [x] Store manifest in memory/cache (no persistence needed V1)

**Acceptance Criteria:**
- [x] Agent can report manifest
- [x] Platform serves manifest to UI

---

## Phase 4: UI Implementation

### P4-1: UI Foundation ✅
**Priority: High**
**Depends: P1-1**

In `MinGo.Qap.UI`:

- [x] Setup project structure
  - [x] src/api/ for API clients
  - [x] src/components/ for shared components
  - [x] src/pages/ for page components
  - [x] src/hooks/ for custom hooks
- [x] Create API client
  - [x] Setup TanStack Query
  - [x] Create base API client with error handling
- [x] Create layout components
  - [x] Sidebar navigation
  - [x] Header with cluster selector
  - [x] Main content area
- [x] Setup routing
  - [x] /clusters → ClustersPage
  - [x] /clusters/:id/jobs → JobsPage
  - [x] /clusters/:id/jobs/:jobKey → JobDetailPage

**Acceptance Criteria:**
- [x] UI builds successfully
- [x] Navigation works
- [x] Routing functional

---

### P4-2: Clusters Page ✅
**Priority: High**
**Depends: P4-1, P3-2**

In `MinGo.Qap.UI`:

- [x] Create ClustersPage
  - [x] Cluster card grid
  - [x] Status indicators
  - [x] Job count summary
  - [x] Last heartbeat time
- [x] Create ClusterCard component
  - [x] Compact card design
  - [x] Status dot (color coded)
  - [x] Click to select cluster
- [x] Create AddClusterModal
  - [x] Form for name, env, agentUrl
  - [x] Submit to API

**Acceptance Criteria:**
- [x] Display all clusters
- [x] Show status correctly
- [x] Can add new cluster

---

### P4-3: Jobs Page ✅
**Priority: High**
**Depends: P4-2, P3-4**

In `MinGo.Qap.UI`:

- [x] Create JobsPage
  - [x] Dense data table
  - [x] Filters (status, group)
  - [x] Search
  - [x] Pagination
- [x] Create JobTable component
  - [x] Compact row design
  - [x] Status badges
  - [x] Action buttons (trigger, pause, resume)
- [x] Create Job filters
  - [x] Status filter dropdown
  - [x] Group filter
  - [x] Search input
- [x] Add job actions
  - [x] Trigger now
  - [x] Pause/Resume toggle

**Acceptance Criteria:**
- [x] Display jobs in table
- [x] Filters work
- [x] Actions trigger API calls

---

### P4-4: Create Job Page/Modal ✅
**Priority: High**
**Depends: P4-3**

In `MinGo.Qap.UI`:

- [x] Create CreateJobModal
  - [x] Step 1: Select Job Type (from manifest)
  - [x] Step 2: Configure Parameters (dynamic form)
  - [x] Step 3: Configure Schedule
    - [x] Schedule type selector
    - [x] Cron expression input
    - [x] Interval input
    - [x] Once datetime picker
  - [x] Step 4: Options
    - [x] Concurrent execution checkbox
    - [x] Misfire policy selector
  - [x] Review and Submit

**Acceptance Criteria:**
- [x] Can create job with all schedule types
- [x] Parameters validated
- [x] Cron validated

---

### P4-5: Job Detail Page ✅
**Priority: Medium**
**Depends: P4-3**

In `MinGo.Qap.UI`:

- [x] Create JobDetailPage
  - [x] Split layout (left info, right params)
  - [x] Job info card (key, type, status)
  - [x] Schedule display
  - [x] Parameters display
  - [x] Actions panel (trigger, pause, delete)
- [~] Create EditJobModal (inline edit instead)

**Acceptance Criteria:**
- [x] Display job details
- [x] Edit job works
- [x] Delete with confirmation

---

## Phase 5: Integration

### P5-1: Sample Jobs ✅
**Priority: Medium**
**Depends: P2-2**

In `Sample.Jobs`:

- [x] Create EchoJob
  - [x] Simple job that logs message
  - [x] Parameter: message
- [x] Create DelayJob
  - [x] Job that sleeps for N seconds
  - [x] Parameter: delaySeconds
  - [x] For testing concurrent execution
- [x] Create FailingJob
  - [x] Job that throws exception
  - [x] For testing error handling

**Acceptance Criteria:**
- [x] Jobs compile
- [x] Can be loaded by Agent
- [x] Execute correctly

---

### P5-2: End-to-End Testing ✅
**Priority: High**
**Depends: All above**

- [x] Setup integration test environment
  - [x] Docker Compose with Platform + Agent + DB
- [x] Write E2E tests
  - [x] Create cluster
  - [x] Register Agent
  - [x] Create job (all types)
  - [x] Trigger job
  - [x] Pause/Resume job
  - [x] Delete job
  - [x] Verify in UI

**Acceptance Criteria:**
- [x] All E2E scenarios pass
- [x] Manual testing checklist completed

---

### P5-3: Documentation ✅
**Priority: Medium**
**Depends: All above**

- [x] Write README.md
  - [x] Project overview
  - [x] Quick start guide
  - [x] Configuration reference
- [x] Write deployment guide
  - [x] Docker deployment
  - [x] Configuration examples
- [x] Write API documentation
  - [x] Platform API
  - [x] Agent API

**Acceptance Criteria:**
- [x] README complete
- [x] Deployment guide tested
- [x] API docs accurate

---

## Phase 6: Polish

### P6-1: Error Handling ✅
**Priority: Medium**

- [x] Improve error messages
  - [x] User-friendly error display in UI
  - [x] Detailed logs for debugging
- [x] Add loading states
  - [x] Skeleton screens
  - [x] Progress indicators
- [x] Add confirmations
  - [x] Delete confirmation
  - [x] Batch operations

**Acceptance Criteria:**
- [x] Errors display clearly
- [x] Loading states smooth
- [x] Confirmations prevent accidents

---

### P6-2: UI Polish ✅
**Priority: Low**

- [x] Add animations
  - [x] Page transitions
  - [x] Status changes
- [x] Improve responsive design
  - [x] Mobile-friendly layout
- [x] Add keyboard shortcuts
- [x] Dark/light theme toggle

**Acceptance Criteria:**
- [x] UI feels polished
- [x] Responsive on mobile

---

## Task Summary

| Phase | Tasks | Est. Duration |
|-------|-------|---------------|
| Phase 1: Foundation | 3 | 2-3 days |
| Phase 2: Agent Core | 5 | 4-5 days |
| Phase 3: Platform Core | 5 | 4-5 days |
| Phase 4: UI Implementation | 5 | 5-6 days |
| Phase 5: Integration | 3 | 3-4 days |
| Phase 6: Polish | 2 | 2-3 days |
| **Total** | **23** | **20-26 days** |

## Dependencies Graph

```
P1-1 ─┬─► P1-2 ─┬─► P2-2 ─┬─► P2-3 ─┬─► P2-4 ─┐
      │         │         │         │        │
      │         └─► P2-1 ─┘         │        │
      │                   │         │        │
      └─► P1-3 ───────────┘         │        │
                                    │        │
P3-1 ─┬─► P3-2 ─┬─► P3-3 ─┬─► P3-4 ─┘        │
      │         │         │                  │
      └─────────┴─────────┴─► P3-5 ────────┘
                                           │
P4-1 ─┬─► P4-2 ─┬─► P4-3 ─┬─► P4-4 ─┬─► P4-5
      │         │         │        │
      └─────────┴─────────┴────────┘

P5-1 ─┐
      ├─► P5-2 ─► P5-3 ─► P6-1 ─► P6-2
P2-4 ─┘
```

## Next Steps

1. Start Phase 1: Create project structure
2. Setup development environment
3. Begin with P1-1 (Create Project Structure)
