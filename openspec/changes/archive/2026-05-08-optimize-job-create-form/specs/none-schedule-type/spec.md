## MODIFIED Requirements

### Requirement: None schedule with non-durable job
- **WHEN** user selects Schedule type "None" AND does not check "持久化 Job" (`storeDurable=false`)
- **THEN** the Agent SHALL call `scheduler.AddJob(detail, replace: true, storeNonDurableWhileAwaitingScheduling: true)`
- **AND** the job SHALL remain in the scheduler until a trigger is added

### Requirement: None schedule with durable job
- **WHEN** user selects Schedule type "None" AND checks "持久化 Job" (`storeDurable=true`)
- **THEN** the Agent SHALL call `scheduler.AddJob(detail, replace: true)` WITHOUT `storeNonDurableWhileAwaitingScheduling`
- **AND** the job SHALL be permanently stored in the scheduler (StoreDurable=true)
