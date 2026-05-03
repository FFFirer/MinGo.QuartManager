# Job Templates Specification

## Purpose

This specification defines the requirements for creating jobs from templates and copying from existing jobs.

**Status:** New  
**Last Updated:** 2026-05-04

---

## ADDED Requirements

### Requirement: Create job supports template selection

The create job panel SHALL allow users to start from a predefined template or copy from an existing job.

#### Scenario: Template selector visible
- **WHEN** create job panel opens
- **THEN** a template selector SHALL appear at the top with options:
  - "Blank" (start fresh)
  - List of available job type templates from manifest
  - "Copy from existing" option

#### Scenario: Select template pre-fills form
- **WHEN** user selects a job type template
- **THEN** the job type field SHALL be pre-selected
- **AND** parameter fields SHALL show default values from the manifest
- **AND** schedule type SHALL default to Cron with expression "0 0 * * *"

### Requirement: Create job supports copying from existing job

The create job panel SHALL allow users to copy configuration from an existing job.

#### Scenario: Copy from existing job
- **WHEN** user selects "Copy from existing"
- **THEN** a searchable dropdown SHALL show all existing jobs for the current scheduler
- **AND** **WHEN** user selects a source job
- **THEN** all fields SHALL be pre-filled from the source job's configuration
- **AND** the job key field SHALL be cleared (user must enter a new unique key)
- **AND** the user SHALL be able to modify any field before creating
