# None Schedule Type Specification

## ADDED Requirements

### Requirement: Schedule supports "None" type

The Schedule type options SHALL include "None", which means no trigger is created for the job.

#### Scenario: None schedule option in form

- **WHEN** user is on the Create Job page
- **THEN** system SHALL show "None" as a selectable Schedule type alongside Once, Cron, and Interval

#### Scenario: None schedule hides trigger config fields

- **WHEN** user selects Schedule type "None"
- **THEN** system SHALL hide the cron expression input, interval fields, and datetime picker
- **AND** show a message: "Job will be created without a trigger. Use 'Trigger' action to fire manually."

#### Scenario: None schedule with non-durable job

- **WHEN** user selects Schedule type "None" AND does not check "持久化 Job"
- **THEN** the Agent SHALL call `scheduler.AddJob(detail, replace: true, storeNonDurableWhileAwaitingScheduling: true)`
- **AND** the job SHALL remain in the scheduler until a trigger is added

#### Scenario: None schedule with durable job

- **WHEN** user selects Schedule type "None" AND checks "持久化 Job"
- **THEN** the Agent SHALL call `scheduler.AddJob(detail, replace: true)` with StoreDurable=true
- **AND** the job SHALL be permanently stored in the scheduler

### Requirement: Agent handles None schedule type

The Agent's JobConverter and QuartzService SHALL correctly handle Schedule type "None" by not creating any trigger.

#### Scenario: ConvertToTrigger returns null for None

- **WHEN** `schedule.Type` is "none" (case-insensitive)
- **THEN** `ConvertToTrigger` SHALL return null instead of throwing

#### Scenario: CreateJobAsync skips trigger for None

- **WHEN** Schedule type is "None"
- **THEN** `CreateJobAsync` SHALL NOT create or schedule any trigger
- **AND** SHALL return the JobDetailDto without trigger info

#### Scenario: GetScheduleType returns "none" for triggerless jobs

- **WHEN** a job has no trigger (triggers list is empty)
- **THEN** `GetScheduleType` SHALL return "none"
