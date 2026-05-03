# Platform Dashboard Specification (Delta)

## Purpose

This delta spec updates the Platform Dashboard to add real-time activity feed, health matrix view, and job execution trend chart.

**Status:** Delta  
**Last Updated:** 2026-05-04

---

## ADDED Requirements

### Requirement: Dashboard displays scheduler health matrix

The platform dashboard SHALL display a visual health matrix showing all schedulers with their status and key metrics.

#### Scenario: Health matrix renders
- **WHEN** platform dashboard loads
- **THEN** the dashboard SHALL display a health matrix section
- **AND** each scheduler SHALL be shown as a card with:
  - Scheduler name
  - Status indicator (running: green, standby: amber, unknown: grey)
  - Job count
  - Agent count (total and healthy)
  - Last reported timestamp
  - Clicking a card SHALL navigate to the scheduler detail page

### Requirement: Dashboard displays job execution trend chart

The platform dashboard SHALL display a trend chart showing job execution frequency over the past 24 hours.

#### Scenario: Trend chart renders
- **WHEN** platform dashboard loads
- **THEN** the dashboard SHALL display a bar chart of job executions per hour for the last 24 hours
- **AND** the chart SHALL use the current hour as reference point
- **AND** hover over a bar SHALL show exact execution count for that hour
- **AND** if no execution data available, SHALL display "No execution data for the past 24 hours"

### Requirement: Dashboard fetches data from v2 API endpoints

The platform dashboard SHALL fetch data from the v2 Agent and Scheduler APIs instead of the legacy /api/dashboard endpoint.

#### Scenario: Dashboard uses aggregated queries
- **WHEN** platform dashboard loads
- **THEN** it SHALL fetch data via:
  - GET /api/schedulers (for scheduler list and counts)
  - GET /api/agents (for agent health breakdown)
  - Individual scheduler job endpoints (for upcoming jobs)
- **AND** SHALL NOT call the legacy /api/dashboard endpoint
- **AND** all Cluster-related data structures SHALL be removed from the component

## MODIFIED Requirements

### Requirement: Platform Dashboard displays overview statistics

The platform dashboard SHALL display aggregated statistics from all schedulers, including total schedulers, total jobs, and agent health breakdown. Data SHALL be derived from v2 API endpoints.

**Change**: Data source changed from /api/dashboard to aggregated v2 API calls

#### Scenario: Dashboard loads successfully
- **WHEN** user navigates to the platform dashboard (/)
- **THEN** the page SHALL display platform overview statistics
- **AND** SHALL show total schedulers count, total jobs count, total agents count
- **AND** SHALL show agent health breakdown (online, warning, offline counts)
- **AND** data SHALL be derived from GET /api/schedulers and GET /api/agents
- **AND** the total jobs count SHALL be the sum of jobCounts.totalJobs across all schedulers

### Requirement: Platform Dashboard displays scheduler list

The platform dashboard SHALL display a list of all schedulers as a health matrix with their status, job count, and agent information.

**Change**: Data format changed from simple list to health matrix cards

#### Scenario: Display scheduler cards
- **WHEN** platform dashboard loads
- **THEN** for each scheduler, the dashboard SHALL display a card showing:
  - Scheduler name and status indicator (color-coded)
  - Job count (from scheduler.jobCounts?.totalJobs)
  - Agent count (total from scheduler.agentCount)
  - Last heartbeat time
  - Cards SHALL link to /schedulers/{name}

### Requirement: Platform Dashboard displays job status distribution

The platform dashboard SHALL show a visual representation of job status distribution across all schedulers, using data from scheduler detail jobCounts.

**Change**: Data source from v2 scheduler jobCounts

#### Scenario: Job status chart displays
- **WHEN** platform dashboard loads
- **THEN** the dashboard SHALL display job status breakdown:
  - Active/waiting jobs count (jobCounts.waitingJobs)
  - Paused jobs count (jobCounts.pausedJobs)
  - Blocked jobs count (jobCounts.blockedJobs)
  - Executing jobs count (jobCounts.runningJobs)
