# Cluster Calendar Specification

## Purpose

This specification defines the requirements for the scheduler calendar view feature that visualizes job schedules in a calendar format.

**Status:** Updated  
**Last Updated:** 2026-05-03

---

## MODIFIED Requirements

### Requirement: Calendar displays month view by default
The scheduler calendar SHALL display a monthly calendar view showing job execution schedule.

**Change**: "cluster" → "scheduler"; route /clusters/:clusterId/calendar → /schedulers/:name/calendar

#### Scenario: Month view displays
- **WHEN** user navigates to scheduler calendar (/schedulers/:name/calendar)
- **THEN** the calendar SHALL display the current month by default
- **AND** SHALL highlight days with scheduled job executions
- **AND** SHALL show job indicators on days with executions

### Requirement: Calendar supports month navigation
The scheduler calendar SHALL allow users to navigate between months.

#### Scenario: Navigate to next month
- **WHEN** user clicks the "Next" arrow button
- **THEN** the calendar SHALL display the next month
- **AND** SHALL update the month/year header
- **AND** SHALL recalculate job fire times for the new month

#### Scenario: Navigate to previous month
- **WHEN** user clicks the "Previous" arrow button
- **THEN** the calendar SHALL display the previous month
- **AND** SHALL update the month/year header

#### Scenario: Navigate to current month
- **WHEN** user clicks "Today" button
- **THEN** the calendar SHALL display the current month

### Requirement: Calendar supports multiple view modes
The scheduler calendar SHALL support Month, Week, and List (Agenda) view modes.

#### Scenario: Switch to List view
- **WHEN** user selects "List" view mode
- **THEN** the calendar SHALL display jobs grouped by date
- **AND** SHALL show schedule description for each job

### Requirement: Calendar displays job details on hover/click
The scheduler calendar SHALL show job details when user interacts with a job indicator.

#### Scenario: Job click shows menu
- **WHEN** user clicks on a job indicator
- **THEN** a context menu SHALL appear with options:
  - View Details (navigate to job detail page)
  - Trigger Now
  - Copy job key

### Requirement: Calendar highlights today
The scheduler calendar SHALL visually highlight the current day.

#### Scenario: Today highlight
- **WHEN** calendar displays current month
- **THEN** the current day SHALL have a distinct visual indicator
- **AND** SHALL be more prominent than other days

### Requirement: Calendar handles no jobs gracefully
The scheduler calendar SHALL display a message when no jobs are scheduled.

#### Scenario: No jobs scheduled
- **WHEN** calendar has no jobs with scheduled executions
- **THEN** the calendar SHALL display "No scheduled jobs" message

### Requirement: Calendar API uses schedulerName
The calendar SHALL use schedulerName in its API requests.

**Change**: API endpoint from /api/clusters/{clusterId}/calendar to /api/schedulers/{schedulerName}/calendar

#### Scenario: API call uses schedulerName
- **WHEN** calendar fetches job data
- **THEN** it SHALL call /api/schedulers/{schedulerName}/calendar?year=&month=
- **AND** SHALL pass schedulerName as the identifier

## REMOVED Requirements

### Requirement: Calendar navigation from sidebar
**Reason**: Cluster context sidebar removed. Calendar reached via dashboard or scheduler detail links.
**Migration**: Navigate to /schedulers/{name}/calendar directly.
