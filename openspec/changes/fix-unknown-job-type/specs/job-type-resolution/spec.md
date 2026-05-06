## ADDED Requirements

### Requirement: Job type resolution for all registration paths
The system SHALL resolve a meaningful JobType string for every job in a Quartz scheduler, using a fallback chain when direct metadata is unavailable.

#### Scenario: Job created via Platform API returns its manifest key
- **WHEN** a job was created through `POST /api/schedulers/{name}/jobs` with a specific `JobType`
- **THEN** `GET /api/schedulers/{name}/jobs` and `GET /api/schedulers/{name}/jobs/{key}` SHALL return that same `JobType` value

#### Scenario: Discovered job returns its registry key
- **WHEN** a job was registered by `JobDiscoveryService` (assembly scanning or config-based) and `JobRegistry` contains a `JobTypeInfoDto` whose `JobTypeFullName` matches `jobDetail.JobType.FullName`
- **THEN** `GET /api/schedulers/{name}/jobs` and `GET /api/schedulers/{name}/jobs/{key}` SHALL return the matching `JobTypeInfoDto.Key`

#### Scenario: Directly-registered job returns CLR type name
- **WHEN** a job was registered via Quartz DI (`ScheduleJob<T>()`, `AddJob<T>()`) and is NOT in `JobRegistry`
- **THEN** `GET /api/schedulers/{name}/jobs` and `GET /api/schedulers/{name}/jobs/{key}` SHALL return `jobDetail.JobType.Name` (the simple CLR class name)

#### Scenario: Job with no type information returns "unknown"
- **WHEN** a job has neither `JobDataMap["jobType"]`, nor a `JobRegistry` match, nor a resolvable `JobType` on the `IJobDetail`
- **THEN** `GET /api/schedulers/{name}/jobs` and `GET /api/schedulers/{name}/jobs/{key}` SHALL return `"unknown"`

### Requirement: No API contract change
The resolution logic SHALL be internal to the Agent's `QuartzService`. The `JobSummaryDto.JobType` and `JobDetailDto.JobType` fields SHALL remain `string` type with no new fields.

#### Scenario: Existing API clients are unaffected
- **WHEN** an existing API client calls any job endpoint
- **THEN** the response format SHALL be identical except `JobType` may now contain a non-`"unknown"` value for previously unresolvable jobs
