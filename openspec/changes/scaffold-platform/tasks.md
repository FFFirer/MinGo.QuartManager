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

### P1-2: Define Shared Models
**Priority: High**
**Depends: P1-1**

In `MinGo.Qap.Shared`:

- [ ] Define enums
  - [ ] `ClusterStatus`: Pending, Online, Warning, Offline, Deleted
  - [ ] `SyncStatus`: Pending, Synced, Failed, Timeout
  - [ ] `ScheduleType`: Once, Cron, Interval
  - [ ] `MisfirePolicy`: FireAndProceed, IgnoreMisfire, DoNothing
- [ ] Define DTOs
  - [ ] `ClusterDto`, `CreateClusterRequest`, `ClusterSummary`
  - [ ] `JobDefinitionDto`, `CreateJobRequest`, `UpdateJobRequest`
  - [ ] `JobManifestDto`, `JobTypeInfoDto`, `ParameterInfoDto`
  - [ ] `ScheduleDto`, `QuartzOptionsDto`
  - [ ] `HeartbeatDto`, `JobCountsDto`, `SystemMetricsDto`
- [ ] Add validation attributes

**Acceptance Criteria:**
- All shared models defined
- JSON serialization configured (camelCase, UTC)
- Unit tests for validation logic

---

### P1-3: Agent Configuration System
**Priority: High**
**Depends: P1-2**

In `MinGo.Qap.Agent`:

- [ ] Create `AgentConfig` class
  - [ ] Agent settings (id, clusterId, port)
  - [ ] Platform settings (url)
  - [ ] Quartz settings (assemblyPath, jobTypes, properties)
- [ ] Create `ConfigLoader` service
  - [ ] Load from YAML file
  - [ ] Support environment variables override
  - [ ] Validate required fields
- [ ] Create sample `config.yaml`
- [ ] Add configuration validation on startup

**Acceptance Criteria:**
- Agent loads configuration on startup
- Invalid config shows clear error message
- Environment variables properly override YAML values

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

### P2-2: Job Registry System
**Priority: High**
**Depends: P2-1**

In `MinGo.Qap.Agent`:

- [ ] Create `JobManifest` model
- [ ] Create `IJobRegistry` interface
- [ ] Create `JobRegistry` implementation
  - [ ] Load from config (V1: config declaration)
  - [ ] Support parameter metadata
- [ ] Create manifest endpoint
  - [ ] GET /api/jobs/manifest returns registered jobs

**Acceptance Criteria:**
- Agent returns manifest with configured job types
- Each job type has key, description, parameters

---

### P2-3: Job Converter
**Priority: High**
**Depends: P2-2**

In `MinGo.Qap.Agent`:

- [ ] Create `IJobConverter` interface
- [ ] Create `JobConverter` implementation
  - [ ] Convert `CreateJobRequest` → `IJobDetail`
  - [ ] Convert `ScheduleDto` → `ITrigger`
  - [ ] Handle JobDataMap population from params
  - [ ] Support replace: true for idempotency
- [ ] Support schedule types
  - [ ] Once: SimpleTrigger, fire once
  - [ ] Cron: CronTrigger
  - [ ] Interval: SimpleTrigger, repeat forever
- [ ] Handle options
  - [ ] DisallowConcurrentExecution attribute
  - [ ] Misfire policy mapping

**Acceptance Criteria:**
- All 3 schedule types convert correctly
- JobDataMap contains all parameters
- Replace flag works for updates

---

### P2-4: Quartz Service Implementation
**Priority: High**
**Depends: P2-3**

In `MinGo.Qap.Agent`:

- [ ] Create `IQuartzService` interface
- [ ] Create `QuartzService` implementation
  - [ ] CreateJobAsync: ScheduleJob with replace
  - [ ] UpdateJobAsync: RescheduleJob
  - [ ] DeleteJobAsync: DeleteJob
  - [ ] TriggerJobAsync: TriggerJob
  - [ ] PauseJobAsync: PauseJob
  - [ ] ResumeJobAsync: ResumeJob
  - [ ] GetJobAsync: GetJobDetail
  - [ ] GetJobsAsync: GetJobGroupNames + GetJobKeys
- [ ] Create JobsController
  - [ ] POST /api/jobs → CreateJob
  - [ ] GET /api/jobs → GetJobs
  - [ ] GET /api/jobs/{jobKey} → GetJob
  - [ ] PUT /api/jobs/{jobKey} → UpdateJob
  - [ ] DELETE /api/jobs/{jobKey} → DeleteJob
  - [ ] POST /api/jobs/{jobKey}/trigger → TriggerJob
  - [ ] POST /api/jobs/{jobKey}/pause → PauseJob
  - [ ] POST /api/jobs/{jobKey}/resume → ResumeJob

**Acceptance Criteria:**
- All CRUD operations work
- Trigger/Pause/Resume work
- Controller returns proper HTTP status codes

---

### P2-5: Heartbeat Service
**Priority: Medium**
**Depends: P2-1**

In `MinGo.Qap.Agent`:

- [ ] Create `HeartbeatService` (BackgroundService)
  - [ ] Run every 30s
  - [ ] Collect scheduler metrics
  - [ ] Collect job counts
  - [ ] POST to Platform
- [ ] Create heartbeat model builder
  - [ ] Get scheduler status
  - [ ] Count jobs by state
  - [ ] Get system metrics (memory)

**Acceptance Criteria:**
- Heartbeat sends every 30s
- Contains correct job counts
- Handles Platform unavailability gracefully

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

### P3-3: Agent Proxy Service
**Priority: High**
**Depends: P3-2**

In `MinGo.Qap.Platform`:

- [ ] Create `IAgentProxyService` interface
- [ ] Create `AgentProxyService` implementation
  - [ ] HTTP client configuration per cluster
  - [ ] GET/POST/PUT/DELETE methods
  - [ ] Timeout handling
  - [ ] Error handling
- [ ] Handle Agent unavailability
  - [ ] Return 503 if Agent offline
  - [ ] Log errors

**Acceptance Criteria:**
- Proxy forwards requests correctly
- Proper error handling for offline Agents
- Timeout configurable

---

### P3-4: Job Service
**Priority: High**
**Depends: P3-2, P3-3**

In `MinGo.Qap.Platform`:

- [ ] Create `IJobService` interface
- [ ] Create `JobService` implementation
  - [ ] CreateAsync: Validate, save backup, proxy to Agent
  - [ ] UpdateAsync: Save backup, proxy to Agent
  - [ ] DeleteAsync: Proxy to Agent, mark deleted
  - [ ] GetAsync: Proxy to Agent (realtime)
  - [ ] GetByClusterAsync: Proxy to Agent (realtime)
  - [ ] Trigger/Pause/Resume: Proxy to Agent
- [ ] Handle sync status
  - [ ] Mark Pending before proxy
  - [ ] Update to Synced on success
  - [ ] Update to Failed/Timeout on error

**Acceptance Criteria:**
- All operations proxy correctly
- Status tracked properly
- Error states handled

---

### P3-5: Manifest Controller
**Priority: Medium**
**Depends: P3-4**

In `MinGo.Qap.Platform`:

- [ ] Create `ManifestController`
  - [ ] POST /api/clusters/{id}/manifest → Store manifest
  - [ ] GET /api/clusters/{id}/manifest → Get manifest
- [ ] Store manifest in memory/cache (no persistence needed V1)

**Acceptance Criteria:**
- Agent can report manifest
- Platform serves manifest to UI

---

## Phase 4: UI Implementation

### P4-1: UI Foundation
**Priority: High**
**Depends: P1-1**

In `MinGo.Qap.UI`:

- [ ] Setup project structure
  - [ ] src/api/ for API clients
  - [ ] src/components/ for shared components
  - [ ] src/pages/ for page components
  - [ ] src/hooks/ for custom hooks
- [ ] Create API client
  - [ ] Setup TanStack Query
  - [ ] Create base API client with error handling
- [ ] Create layout components
  - [ ] Sidebar navigation
  - [ ] Header with cluster selector
  - [ ] Main content area
- [ ] Setup routing
  - [ ] /clusters → ClustersPage
  - [ ] /clusters/:id/jobs → JobsPage
  - [ ] /clusters/:id/jobs/:jobKey → JobDetailPage

**Acceptance Criteria:**
- UI builds successfully
- Navigation works
- Routing functional

---

### P4-2: Clusters Page
**Priority: High**
**Depends: P4-1, P3-2**

In `MinGo.Qap.UI`:

- [ ] Create ClustersPage
  - [ ] Cluster card grid
  - [ ] Status indicators
  - [ ] Job count summary
  - [ ] Last heartbeat time
- [ ] Create ClusterCard component
  - [ ] Compact card design
  - [ ] Status dot (color coded)
  - [ ] Click to select cluster
- [ ] Create AddClusterModal
  - [ ] Form for name, env, agentUrl
  - [ ] Submit to API

**Acceptance Criteria:**
- Display all clusters
- Show status correctly
- Can add new cluster

---

### P4-3: Jobs Page
**Priority: High**
**Depends: P4-2, P3-4**

In `MinGo.Qap.UI`:

- [ ] Create JobsPage
  - [ ] Dense data table
  - [ ] Filters (status, group)
  - [ ] Search
  - [ ] Pagination
- [ ] Create JobTable component
  - [ ] Compact row design
  - [ ] Status badges
  - [ ] Action buttons (trigger, pause, resume)
- [ ] Create Job filters
  - [ ] Status filter dropdown
  - [ ] Group filter
  - [ ] Search input
- [ ] Add job actions
  - [ ] Trigger now
  - [ ] Pause/Resume toggle

**Acceptance Criteria:**
- Display jobs in table
- Filters work
- Actions trigger API calls

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
- [~] Cron validated (V2)

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

### P5-1: Sample Jobs
**Priority: Medium**
**Depends: P2-2**

In `Sample.Jobs`:

- [ ] Create EchoJob
  - [ ] Simple job that logs message
  - [ ] Parameter: message
- [ ] Create DelayJob
  - [ ] Job that sleeps for N seconds
  - [ ] Parameter: delaySeconds
  - [ ] For testing concurrent execution
- [ ] Create FailingJob
  - [ ] Job that throws exception
  - [ ] For testing error handling

**Acceptance Criteria:**
- Jobs compile
- Can be loaded by Agent
- Execute correctly

---

### P5-2: End-to-End Testing
**Priority: High**
**Depends: All above**

- [ ] Setup integration test environment
  - [ ] Docker Compose with Platform + Agent + DB
- [ ] Write E2E tests
  - [ ] Create cluster
  - [ ] Register Agent
  - [ ] Create job (all types)
  - [ ] Trigger job
  - [ ] Pause/Resume job
  - [ ] Delete job
  - [ ] Verify in UI

**Acceptance Criteria:**
- All E2E scenarios pass
- Manual testing checklist completed

---

### P5-3: Documentation
**Priority: Medium**
**Depends: All above**

- [ ] Write README.md
  - [ ] Project overview
  - [ ] Quick start guide
  - [ ] Configuration reference
- [ ] Write deployment guide
  - [ ] Docker deployment
  - [ ] Configuration examples
- [ ] Write API documentation
  - [ ] Platform API
  - [ ] Agent API

**Acceptance Criteria:**
- README complete
- Deployment guide tested
- API docs accurate

---

## Phase 6: Polish

### P6-1: Error Handling
**Priority: Medium**

- [ ] Improve error messages
  - [ ] User-friendly error display in UI
  - [ ] Detailed logs for debugging
- [ ] Add loading states
  - [ ] Skeleton screens
  - [ ] Progress indicators
- [ ] Add confirmations
  - [ ] Delete confirmation
  - [ ] Batch operations

**Acceptance Criteria:**
- Errors display clearly
- Loading states smooth
- Confirmations prevent accidents

---

### P6-2: UI Polish
**Priority: Low**

- [ ] Add animations
  - [ ] Page transitions
  - [ ] Status changes
- [ ] Improve responsive design
  - [ ] Mobile-friendly layout
- [ ] Add keyboard shortcuts
- [ ] Dark/light theme toggle

**Acceptance Criteria:**
- UI feels polished
- Responsive on mobile

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
