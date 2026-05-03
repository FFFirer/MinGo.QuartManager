# Platform Dashboard Specification

## Purpose

This specification defines the requirements for the platform-level dashboard feature that provides an aggregated overview of all schedulers in the MinGo QAP system.

**Status:** Updated  
**Last Updated:** 2026-05-03

---

## MODIFIED Requirements

### Requirement: Platform Dashboard displays overview statistics
The platform dashboard SHALL display aggregated statistics from all schedulers, including total schedulers, total jobs, and agent health breakdown.

**Change**: "clusters" → "schedulers"

#### Scenario: Dashboard loads successfully
- **WHEN** user navigates to the platform dashboard (/)
- **THEN** the page SHALL display platform overview statistics
- **AND** SHALL show total schedulers count, total jobs count, total agents count
- **AND** SHALL show agent health breakdown (online, warning, offline counts)

### Requirement: Platform Dashboard displays scheduler list
The platform dashboard SHALL display a list of all schedulers with their status, job count, and agent information.

**Change**: "cluster" → "scheduler"; route from /clusters/{id} → /schedulers/{name}

#### Scenario: Display scheduler cards
- **WHEN** platform dashboard loads
- **THEN** for each scheduler, the dashboard SHALL display a card showing:
  - Scheduler name and status indicator
  - Job count
  - Agent count (total and healthy)
  - Last heartbeat time

### Requirement: Platform Dashboard displays job status distribution
The platform dashboard SHALL show a visual representation of job status distribution across all schedulers.

**Change**: "clusters" → "schedulers"

#### Scenario: Job status chart displays
- **WHEN** platform dashboard loads
- **THEN** the dashboard SHALL display job status breakdown:
  - Active jobs count
  - Paused jobs count
  - Blocked jobs count (if available)
  - Executing jobs count (if available)

### Requirement: Platform Dashboard displays upcoming jobs
The platform dashboard SHALL display a list of upcoming jobs from all schedulers in the next 24 hours.

**Change**: "clusters" → "schedulers"; "cluster name" → "scheduler name"

#### Scenario: Upcoming jobs list shows
- **WHEN** platform dashboard loads
- **THEN** the dashboard SHALL display a list of jobs scheduled to run in the next 24 hours
- **AND** each entry SHALL show: scheduled time, job key, job type, and scheduler name
- **AND** SHALL be limited to a maximum of 10 entries by default

### Requirement: User can navigate to scheduler from dashboard
The platform dashboard SHALL allow users to navigate to individual scheduler pages.

**Change**: "cluster" → "scheduler"; route from /clusters/{id} → /schedulers/{name}

#### Scenario: Click scheduler card
- **WHEN** user clicks on a scheduler card
- **THEN** the application SHALL navigate to the scheduler detail page at /schedulers/{name}

## REMOVED Requirements

### Requirement: Platform Dashboard displays cluster list
**Reason**: Cluster concept removed in v2.0.0 architecture refactor. Replaced by scheduler list.
**Migration**: See "Platform Dashboard displays scheduler list" above.

### Requirement: User can navigate to cluster from dashboard
**Reason**: Cluster pages removed, replaced by SchedulerDetailPage at /schedulers/{name}.
**Migration**: Scheduler cards link to /schedulers/{name} instead.

### Requirement: Platform Dashboard shows empty state
**Reason**: Cluster concept removed. Empty state now applies to schedulers, handled in implementation.
**Migration**: Dashboard SHALL display "No schedulers" message when no schedulers exist.
