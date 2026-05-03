# Toast Notification Specification (Delta)

## Purpose

This delta spec extends the toast notification system to support batch operation summaries and SSE-triggered notifications.

**Status:** Delta  
**Last Updated:** 2026-05-04

---

## ADDED Requirements

### Requirement: Toast displays batch operation summary

The toast notification system SHALL display a summary toast for batch operations showing success and failure counts.

#### Scenario: Batch operation partial success
- **WHEN** a batch operation (e.g., batch trigger) completes
- **AND** some jobs succeed and some fail
- **THEN** a single summary toast SHALL appear
- **AND** SHALL show: "Triggered 8 of 10 jobs successfully. 2 failed."
- **AND** SHALL use a warning (amber) style
- **AND** SHALL NOT auto-dismiss (user must close)

#### Scenario: Batch operation all success
- **WHEN** a batch operation completes
- **AND** all jobs succeed
- **THEN** a single success toast SHALL appear
- **AND** SHALL show: "Triggered 10 jobs successfully"
- **AND** SHALL auto-dismiss after 4 seconds

#### Scenario: Batch operation all failed
- **WHEN** a batch operation completes
- **AND** all jobs fail
- **THEN** a single error toast SHALL appear
- **AND** SHALL show: "Batch operation failed for all 5 jobs"
- **AND** SHALL NOT auto-dismiss

### Requirement: Toast supports SSE-triggered notifications

The toast notification system SHALL display notification toasts triggered by SSE events.

#### Scenario: Agent status change notification
- **WHEN** an SSE event indicates an agent went offline
- **THEN** a warning toast SHALL appear
- **AND** SHALL show: "Agent {name} went offline"
- **AND** SHALL auto-dismiss after 5 seconds

#### Scenario: Job completion notification
- **WHEN** an SSE event indicates a job execution failed
- **THEN** an error toast SHALL appear
- **AND** SHALL show: "Job {jobKey} failed: {error message}"
- **AND** SHALL NOT auto-dismiss
