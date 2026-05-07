# Declarative Job Creation Specification

## Purpose

This specification defines the declarative job creation flow: the Platform side first records the intent (JobDefinition with Pending status), then invokes the Agent's idempotent job replace API, and writes back the result. The JobDefinition entity serves as a declaration record, not a scheduling definition.

---

## Requirements

### Requirement: Job creation follows declarative flow

The system SHALL follow a declarative creation flow: record intent → save as Pending → invoke agent → write back result.

#### Scenario: Full declarative creation flow
- **WHEN** a `POST /api/schedulers/{schedulerName}/jobs` request is received
- **THEN** the system SHALL check for an existing JobDefinition with the same (SchedulerName, JobKey)
- **AND** if none exists, create a new JobDefinition with `Status = Pending`
- **AND** save to database (`SaveChanges`)
- **AND** forward the request to the Agent's PUT `/api/agent/jobs` endpoint
- **AND** on Agent success, update `Status = Synced` and save `ResultJson` with returned `JobDetailDto`
- **AND** on Agent failure, update `Status = Failed` and save `ErrorMessage`

### Requirement: Duplicate detection by (SchedulerName, JobKey)

The system SHALL use (SchedulerName, JobKey) as the unique identifier for job deduplication, where JobKey is the full "GroupName.JobName" qualified name.

#### Scenario: Synced declaration returns 409 Conflict
- **WHEN** a creation request has the same (SchedulerName, JobKey) as an existing JobDefinition with `Status = Synced`
- **THEN** the system SHALL return HTTP 409 Conflict with error message "Job已存在"

#### Scenario: Pending declaration is updated
- **WHEN** a creation request has the same (SchedulerName, JobKey) as an existing JobDefinition with `Status = Pending`
- **THEN** the system SHALL update the existing JobDefinition fields (Params, Schedule, Options, UpdatedAt)
- **AND** keep `Status = Pending`
- **AND** re-invoke the Agent replace API

#### Scenario: Failed declaration is retried
- **WHEN** a creation request has the same (SchedulerName, JobKey) as an existing JobDefinition with `Status = Failed`
- **THEN** the system SHALL update `Status = Pending` and clear `ErrorMessage`
- **AND** re-invoke the Agent replace API

### Requirement: JobDefinition entity stores scheduler and job identity

The JobDefinition entity SHALL use SchedulerName (not ClusterId) to identify the target scheduler.

#### Scenario: SchedulerName replaces ClusterId
- **WHEN** a JobDefinition is created
- **THEN** the `SchedulerName` field SHALL store the target scheduler name
- **AND** the `ClusterId` field name SHALL no longer exist
- **AND** the unique index SHALL be on `(SchedulerName, JobKey)`

### Requirement: Agent API uses PUT for idempotent job replacement

The Agent endpoint SHALL use `PUT /api/agent/jobs` for job creation, with internal replace semantics.

#### Scenario: Agent replaces existing job
- **WHEN** Agent receives a PUT /api/agent/jobs request
- **THEN** QuartzService.AddJob(jobDetail, replace: true) SHALL replace any existing job with the same key
- **AND** the trigger SHALL also be replaced
- **AND** the operation SHALL be idempotent (repeated calls produce the same result)

### Requirement: Failed declarations persist for traceability

A failed job declaration SHALL remain in the database with Status=Failed for traceability and retry capability.

#### Scenario: Failed declaration is not deleted
- **WHEN** Agent returns an error
- **THEN** the JobDefinition SHALL have `Status = Failed` and `ErrorMessage` populated
- **AND** the record SHALL NOT be deleted from the database
