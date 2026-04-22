# Platform Dashboard Specification

## Purpose

This specification defines the requirements for the platform-level dashboard feature that provides an aggregated overview of all clusters in the MinGo QAP system.

**Status:** Implemented  
**Last Updated:** 2026-04-22

---

## Requirements

### Requirement: Platform Dashboard displays overview statistics
The platform dashboard SHALL display aggregated statistics from all clusters, including total clusters, total jobs, and agent health breakdown.

#### Scenario: Dashboard loads successfully
- **WHEN** user navigates to the platform dashboard (/)
- **THEN** the page SHALL display platform overview statistics
- **AND** SHALL show total clusters count, total jobs count, total agents count
- **AND** SHALL show agent health breakdown (online, warning, offline counts)

### Requirement: Platform Dashboard displays cluster list
The platform dashboard SHALL display a list of all clusters with their status, job count, and agent information.

#### Scenario: Display cluster cards
- **WHEN** platform dashboard loads
- **THEN** for each cluster, the dashboard SHALL display a card showing:
  - Cluster name and status indicator
  - Job count
  - Agent count (total and healthy)
  - Last heartbeat time

### Requirement: Platform Dashboard displays job status distribution
The platform dashboard SHALL show a visual representation of job status distribution across all clusters.

#### Scenario: Job status chart displays
- **WHEN** platform dashboard loads
- **THEN** the dashboard SHALL display job status breakdown:
  - Active jobs count
  - Paused jobs count
  - Blocked jobs count (if available)
  - Executing jobs count (if available)

### Requirement: Platform Dashboard displays upcoming jobs
The platform dashboard SHALL display a list of upcoming jobs from all clusters in the next 24 hours.

#### Scenario: Upcoming jobs list shows
- **WHEN** platform dashboard loads
- **THEN** the dashboard SHALL display a list of jobs scheduled to run in the next 24 hours
- **AND** each entry SHALL show: scheduled time, job key, job type, and cluster name
- **AND** SHALL be limited to a maximum of 10 entries by default

### Requirement: Platform Dashboard auto-refreshes
The platform dashboard SHALL automatically refresh data at regular intervals.

#### Scenario: Data refreshes periodically
- **WHEN** user stays on platform dashboard
- **THEN** the dashboard SHALL refresh data every 30 seconds
- **AND** SHALL display "Last updated" timestamp
- **AND** SHALL show a refresh indicator during data fetch

### Requirement: Platform Dashboard allows manual refresh
The platform dashboard SHALL allow users to manually trigger a data refresh.

#### Scenario: Manual refresh triggered
- **WHEN** user clicks the refresh button
- **THEN** the dashboard SHALL immediately fetch fresh data
- **AND** SHALL update the "Last updated" timestamp
- **AND** SHALL disable the refresh button during the fetch

### Requirement: Platform Dashboard shows loading state
The platform dashboard SHALL display appropriate loading indicators while fetching data.

#### Scenario: Loading state displayed
- **WHEN** dashboard is fetching initial data
- **THEN** the dashboard SHALL display skeleton loaders for statistics cards
- **AND** SHALL display a loading spinner for cluster list
- **AND** SHALL NOT show stale data

### Requirement: Platform Dashboard handles errors gracefully
The platform dashboard SHALL display user-friendly error messages when data fetching fails.

#### Scenario: API error occurs
- **WHEN** dashboard fails to fetch data from API
- **THEN** the dashboard SHALL display an error message
- **AND** SHALL provide a "Retry" button
- **AND** SHALL NOT crash or show blank page

### Requirement: User can navigate to cluster from dashboard
The platform dashboard SHALL allow users to navigate to individual cluster pages.

#### Scenario: Click cluster card
- **WHEN** user clicks on a cluster card
- **THEN** the application SHALL navigate to the cluster dashboard page
- **AND** the sidebar SHALL update to show cluster context

### Requirement: Platform Dashboard shows empty state
The platform dashboard SHALL display appropriate message when no clusters exist.

#### Scenario: No clusters exist
- **WHEN** platform has no clusters
- **THEN** the dashboard SHALL display "No clusters yet" message
- **AND** SHALL provide a "Add Cluster" button or link