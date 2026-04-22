## ADDED Requirements

### Requirement: Calendar displays month view by default
The cluster calendar SHALL display a monthly calendar view showing job execution schedule.

#### Scenario: Month view displays
- **WHEN** user navigates to cluster calendar (/clusters/:clusterId/calendar)
- **THEN** the calendar SHALL display the current month by default
- **AND** SHALL highlight days with scheduled job executions
- **AND** SHALL show job indicators on days with executions

### Requirement: Calendar supports month navigation
The cluster calendar SHALL allow users to navigate between months.

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
- **WHEN** user clicks "Today" or current month button
- **THEN** the calendar SHALL display the current month

### Requirement: Calendar supports multiple view modes
The cluster calendar SHALL support Month, Week, and List (Agenda) view modes.

#### Scenario: Switch to Week view
- **WHEN** user selects "Week" view mode
- **THEN** the calendar SHALL display a weekly timeline
- **AND** SHALL show time slots vertically
- **AND** SHALL show job bars positioned at their scheduled times

#### Scenario: Switch to List view
- **WHEN** user selects "List" view mode
- **THEN** the calendar SHALL display jobs grouped by date
- **AND** SHALL show schedule description for each job

### Requirement: Calendar displays job details on hover
The cluster calendar SHALL show job details when user hovers over a job indicator.

#### Scenario: Job hover tooltip
- **WHEN** user hovers over a job indicator on the calendar
- **THEN** a tooltip SHALL appear showing:
  - Job key
  - Job type
  - Schedule description
  - Next execution time

### Requirement: Calendar displays job menu on click
The cluster calendar SHALL display an action menu when user clicks on a job indicator.

#### Scenario: Job click shows menu
- **WHEN** user clicks on a job indicator
- **THEN** a context menu SHALL appear with options:
  - View Details (navigate to job detail page)
  - Trigger Now
  - Pause/Resume (depending on current status)
  - Copy job key

### Requirement: Calendar highlights today
The cluster calendar SHALL visually highlight the current day.

#### Scenario: Today highlight
- **WHEN** calendar displays current month
- **THEN** the current day SHALL have a distinct visual indicator
- **AND** SHALL be more prominent than other days

### Requirement: Calendar handles no jobs gracefully
The cluster calendar SHALL display a message when no jobs are scheduled.

#### Scenario: No jobs scheduled
- **WHEN** calendar has no jobs with scheduled executions
- **THEN** the calendar SHALL display "No scheduled jobs" message
- **AND** SHALL provide a "Create Job" link

### Requirement: Calendar shows job count per day
The cluster calendar SHALL display the number of jobs executing on each day.

#### Scenario: Day shows job count
- **WHEN** calendar displays a day with multiple jobs
- **THEN** the day cell SHALL show the number of scheduled jobs
- **AND** clicking the day SHALL expand to show job list

### Requirement: Calendar navigation from sidebar
The cluster calendar SHALL be accessible from the cluster context sidebar.

#### Scenario: Navigate to calendar from sidebar
- **WHEN** user is in cluster context (sidebar shows cluster menu)
- **AND** user clicks "Calendar" in sidebar
- **THEN** the application SHALL navigate to /clusters/:clusterId/calendar
- **AND** the calendar page SHALL load with cluster context