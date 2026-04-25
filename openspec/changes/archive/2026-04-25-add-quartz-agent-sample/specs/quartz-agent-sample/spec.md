## ADDED Requirements

### Requirement: Quartz Agent Sample runs as ASP.NET Core Web API
The sample application SHALL be an ASP.NET Core Web API project that can be started with `dotnet run` and accessed via HTTP endpoints.

#### Scenario: Application starts successfully
- **WHEN** developer runs `dotnet run` in Sample.Agent directory
- **THEN** application starts listening on a configurable port (default 5000)

#### Scenario: Application responds to health check
- **WHEN** developer sends GET request to /health
- **THEN** system returns 200 OK with health status

### Requirement: Quartz.NET configured with RAMJobStore
The application SHALL use Quartz.NET RAMJobStore for job storage (in-memory), without external database dependencies.

#### Scenario: Scheduler uses RAMJobStore
- **WHEN** application starts
- **THEN** Quartz scheduler uses RAMJobStore (verifiable via Quartz configuration)

#### Scenario: Jobs persist only during application runtime
- **WHEN** application is stopped and restarted
- **THEN** previously scheduled jobs are not persisted (expected RAMJobStore behavior)

### Requirement: MinGo.Qap.Agent library integrated via DI
The application SHALL integrate MinGo.Qap.Agent library using ASP.NET Core dependency injection.

#### Scenario: Agent services registered in DI container
- **WHEN** application starts
- **THEN** MinGo.Qap.Agent services are registered in DI container

### Requirement: Sample jobs included and registered
The application SHALL include at least 2 sample jobs (HelloJob, ManualTriggerJob) that are automatically registered with Quartz scheduler.

#### Scenario: HelloJob executes on schedule
- **WHEN** HelloJob trigger fires (every 30 seconds)
- **THEN** HelloJob executes and logs output

#### Scenario: ManualTriggerJob can be triggered via API
- **WHEN** POST request sent to /api/jobs/trigger/manual-trigger
- **THEN** ManualTriggerJob executes immediately

### Requirement: REST API endpoints for job management
The application SHALL provide REST API endpoints to view and trigger jobs.

#### Scenario: List all jobs
- **WHEN** GET request sent to /api/jobs
- **THEN** returns JSON array of registered jobs with name, group, and next fire time

#### Scenario: Trigger job by key
- **WHEN** POST request sent to /api/jobs/{jobKey}/trigger
- **THEN** triggers the specified job and returns 200 OK