# Durable Job Option Specification

## ADDED Requirements

### Requirement: User can set StoreDurable on job creation

The system SHALL provide a "持久化 Job" (StoreDurable) checkbox in the Options section of the Create Job form, independent of the Schedule type selection.

#### Scenario: StoreDurable checkbox present

- **WHEN** user is on the Create Job page
- **THEN** system SHALL show a "持久化 Job" checkbox in the Options section
- **AND** it is unchecked by default

#### Scenario: StoreDurable with Schedule=None

- **WHEN** user checks "持久化 Job" AND selects Schedule type "None"
- **THEN** the job SHALL be stored with StoreDurable=true and no trigger
- **AND** the job SHALL persist permanently in the scheduler

#### Scenario: StoreDurable with Schedule=Cron/Once/Interval

- **WHEN** user checks "持久化 Job" AND selects a non-None Schedule type
- **THEN** the job SHALL be stored with StoreDurable=true AND its trigger created normally

### Requirement: Agent persists StoreDurable option

The Agent SHALL pass the StoreDurable option to Quartz's JobBuilder when creating a JobDetail.

#### Scenario: StoreDurable=true passed to JobBuilder

- **WHEN** `request.Options.StoreDurable` is true
- **THEN** `JobBuilder.StoreDurable(true)` SHALL be called when building the JobDetail

#### Scenario: StoreDurable=false (default)

- **WHEN** `request.Options.StoreDurable` is false or not set
- **THEN** `JobBuilder.StoreDurable` SHALL NOT be called (default behavior)
