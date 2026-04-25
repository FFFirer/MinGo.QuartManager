## ADDED Requirements

### Requirement: Agent SHALL expose Minimal API endpoints
Agent SHALL provide `MapMinGoAgentApi()` extension method on `IEndpointRouteBuilder` that registers all Agent HTTP endpoints under `/api/agent` prefix.

#### Scenario: Host application enables Agent API
- **WHEN** developer calls `app.MapMinGoAgentApi()` in `Program.cs`
- **THEN** all Agent endpoints SHALL be registered and accessible
- **AND** the endpoint group prefix SHALL be `/api/agent`

### Requirement: Agent SHALL support Job CRUD operations
Agent API SHALL expose endpoints for creating, reading, updating, and deleting Quartz jobs.

#### Scenario: Create job via Agent API
- **WHEN** Platform sends `POST /api/agent/jobs` with valid `CreateJobRequest`
- **THEN** Agent SHALL create the job in Quartz Scheduler
- **AND** return `JobDetailDto` with job details

#### Scenario: Get job list via Agent API
- **WHEN** Platform sends `GET /api/agent/jobs?page=1&pageSize=20`
- **THEN** Agent SHALL return paginated list of `JobSummaryDto`

#### Scenario: Get job detail via Agent API
- **WHEN** Platform sends `GET /api/agent/jobs/{jobKey}`
- **THEN** Agent SHALL return `JobDetailDto` for the specified job
- **AND** return 404 if job does not exist

#### Scenario: Update job via Agent API
- **WHEN** Platform sends `PUT /api/agent/jobs/{jobKey}` with `UpdateJobRequest`
- **THEN** Agent SHALL update the job schedule, parameters, or options

#### Scenario: Delete job via Agent API
- **WHEN** Platform sends `DELETE /api/agent/jobs/{jobKey}`
- **THEN** Agent SHALL remove the job from Quartz Scheduler

### Requirement: Agent SHALL support job control operations
Agent API SHALL expose endpoints for triggering, pausing, and resuming jobs.

#### Scenario: Trigger job via Agent API
- **WHEN** Platform sends `POST /api/agent/jobs/{jobKey}/trigger`
- **THEN** Agent SHALL immediately execute the job via `TriggerJob`

#### Scenario: Pause job via Agent API
- **WHEN** Platform sends `POST /api/agent/jobs/{jobKey}/pause`
- **THEN** Agent SHALL pause the job's triggers

#### Scenario: Resume job via Agent API
- **WHEN** Platform sends `POST /api/agent/jobs/{jobKey}/resume`
- **THEN** Agent SHALL resume the job's triggers

### Requirement: Agent SHALL expose scheduler state endpoint
Agent API SHALL provide an endpoint to query Quartz Scheduler runtime state.

#### Scenario: Query scheduler state
- **WHEN** Platform sends `GET /api/agent/scheduler`
- **THEN** Agent SHALL return `SchedulerStateDto` containing name, instanceId, status, job counts, and cluster mode

### Requirement: Agent SHALL expose job manifest endpoint
Agent API SHALL provide an endpoint to retrieve the registered job type manifest.

#### Scenario: Query job manifest
- **WHEN** Platform sends `GET /api/agent/manifest`
- **THEN** Agent SHALL return `JobManifestDto` with all discovered job types and their parameter schemas

### Requirement: Agent API SHALL use standardized response format
All Agent API endpoints SHALL return responses wrapped in `ApiResponse<T>` format.

#### Scenario: Successful response
- **WHEN** an API operation succeeds
- **THEN** the response SHALL contain `success: true`, `data`, and optional `message`

#### Scenario: Error response
- **WHEN** an API operation fails due to invalid input
- **THEN** the response SHALL contain `success: false` and `error` details
- **AND** return appropriate HTTP status code (400, 404, 500)

### Requirement: Agent API SHALL not require authentication
Agent endpoints SHALL be accessible without API key or token validation, relying on network segmentation for security.

#### Scenario: Unauthenticated access
- **WHEN** Platform sends requests without authentication headers
- **THEN** Agent SHALL process the request normally
- **AND** not perform any auth validation
