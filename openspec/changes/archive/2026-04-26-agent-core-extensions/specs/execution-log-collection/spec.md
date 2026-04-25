## ADDED Requirements

### Requirement: Agent SHALL implement IJobListener for execution events
Agent SHALL implement `QapJobListener` that intercepts Quartz Job execution lifecycle events.

#### Scenario: Listener registration
- **WHEN** host application initializes Quartz Scheduler
- **THEN** host application MAY resolve `QapJobListener` from DI and register it with `ListenerManager`
- **AND** listener SHALL apply to all Job groups
- **NOTE** Agent library does not initialize Quartz Scheduler; host application is responsible for scheduler lifecycle

#### Scenario: Job execution started
- **WHEN** Quartz triggers a Job execution
- **THEN** `JobToBeExecuted` SHALL record start time and job key
- **AND** invoke `ILogCollectionService.RecordJobStarted`

#### Scenario: Job execution completed
- **WHEN** Quartz Job execution finishes successfully
- **THEN** `JobWasExecuted` SHALL record end time and success status
- **AND** invoke `ILogCollectionService.RecordJobCompleted`

#### Scenario: Job execution failed
- **WHEN** Quartz Job execution throws exception
- **THEN** `JobWasExecuted` SHALL capture exception message and stack trace
- **AND** record failure status via `RecordJobCompleted`

### Requirement: JobListener SHALL be fault-tolerant
Listener implementation SHALL catch and log all internal exceptions without affecting Quartz execution.

#### Scenario: Listener internal error
- **WHEN** log collection service throws exception during listener callback
- **THEN** listener SHALL catch the exception
- **AND** log the error without propagating to Quartz
- **AND** Job execution SHALL continue unaffected

### Requirement: Execution logs SHALL be buffered and flushed
Agent SHALL buffer execution logs in memory and periodically flush to Platform.

#### Scenario: Log buffering
- **WHEN** multiple jobs execute within flush interval
- **THEN** logs SHALL accumulate in memory buffer

#### Scenario: Periodic flush
- **WHEN** flush timer fires (default 30 seconds)
- **THEN** Agent SHALL send buffered logs to Platform `/api/agents/{id}/logs`
- **AND** clear buffer on successful upload

#### Scenario: Flush failure recovery
- **WHEN** log upload fails due to network error
- **THEN** logs SHALL remain in buffer
- **AND** retry on next flush cycle

### Requirement: Execution logs SHALL include timing information
Each execution log entry SHALL contain start time, end time, and derived duration.

#### Scenario: Execution timing
- **WHEN** a Job executes for 1500ms
- **THEN** log SHALL contain accurate start and end timestamps
- **AND** Platform can calculate duration from the difference

### Requirement: Execution logs SHALL reference Job key
Each execution log SHALL be associated with the Job that produced it.

#### Scenario: Job key association
- **WHEN** Job "sync.DataSyncJob" executes
- **THEN** log entry `JobKey` SHALL be "sync.DataSyncJob"
- **AND** Platform can aggregate logs by Job key
