# Job Templates Specification

## Purpose

This specification defines the job creation flow, supporting creating new jobs from scratch and copying from existing jobs via the full-page Create Job form.

**Status:** Updated  
**Last Updated:** 2026-05-07

---

## Requirements

### Requirement: Create job supports copying from existing job
The system SHALL allow users to copy configuration from an existing job via URL parameter.

#### Scenario: Copy from existing job via URL parameter
- **WHEN** user clicks "Copy" action on a job row in JobsPage
- **THEN** system navigates to `/schedulers/{name}/jobs/create?copyFrom={GROUP.name}`
- **AND** the Create Job page pre-fills all fields from the source job
- **AND** the user SHALL be able to modify any field before creating
- **AND** submitting creates a new job (does not update the source)

### Requirement: Create new job from scratch
The system SHALL allow creating a new job from the full-page form with all fields empty (defaults applied).

#### Scenario: Navigate to create new job
- **WHEN** user clicks "Create Job" button on JobsPage
- **THEN** system navigates to `/schedulers/{name}/jobs/create`
- **AND** the form is empty with default values
