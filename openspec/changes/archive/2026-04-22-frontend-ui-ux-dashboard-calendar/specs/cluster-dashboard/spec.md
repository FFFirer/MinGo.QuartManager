## ADDED Requirements

### Requirement: Cluster Dashboard displays cluster overview
The cluster dashboard SHALL display aggregated statistics for a specific cluster, including job counts, agent counts, and status breakdown.

#### Scenario: Cluster dashboard loads
- **WHEN** user navigates to cluster dashboard (/clusters/:clusterId)
- **THEN** the page SHALL display cluster overview statistics:
  - Total jobs, Active jobs, Paused jobs, Blocked jobs
  - Total agents, Online agents, Warning agents, Offline agents

### Requirement: Cluster Dashboard displays cluster header
The cluster dashboard SHALL display a prominent header showing cluster name, status, environment, and creation date.

#### Scenario: Header displays correctly
- **WHEN** cluster dashboard loads
- **THEN** the header SHALL show:
  - Cluster name (prominent)
  - Status indicator (Online/Warning/Offline)
  - Environment tag (prod/staging/dev)
  - Creation date

### Requirement: Cluster Dashboard displays job status chart
The cluster dashboard SHALL display a visual representation of job status distribution for this cluster.

#### Scenario: Job status chart displays
- **WHEN** cluster dashboard loads
- **THEN** the dashboard SHALL display a bar chart or progress bar showing:
  - Active jobs count
  - Paused jobs count
  - Blocked jobs count

### Requirement: Cluster Dashboard displays recent agents
The cluster dashboard SHALL display a list of recent agent instances in this cluster.

#### Scenario: Agent list displays
- **WHEN** cluster dashboard loads
- **THEN** the dashboard SHALL display up to 5 agent instances
- **AND** each entry SHALL show: name, status indicator, URL, last heartbeat time
- **AND** SHALL provide a "View All" link to agents page

### Requirement: Cluster Dashboard displays upcoming jobs
The cluster dashboard SHALL display a list of upcoming jobs for this cluster in the next 24 hours.

#### Scenario: Upcoming jobs list shows
- **WHEN** cluster dashboard loads
- **THEN** the dashboard SHALL display jobs scheduled in the next 24 hours
- **AND** each entry SHALL show: scheduled time, job key, job type, schedule description
- **AND** SHALL provide a "View Calendar" link

### Requirement: Cluster Dashboard displays execution history placeholder
The cluster dashboard SHALL display a placeholder section for execution history (future feature).

#### Scenario: Execution history section displays
- **WHEN** cluster dashboard loads
- **THEN** the dashboard SHALL display an "Execution History" section
- **AND** SHALL show a placeholder message indicating the feature is not yet available

### Requirement: Cluster Dashboard allows job creation
The cluster dashboard SHALL provide a prominent "Create Job" action button.

#### Scenario: Create job button visible
- **WHEN** cluster dashboard is displayed
- **THEN** a "Create Job" button SHALL be visible in the header area
- **AND** clicking it SHALL open the Create Job modal

### Requirement: Cluster Dashboard allows navigation to sub-pages
The cluster dashboard SHALL provide navigation links to Jobs, Calendar, and Agents pages.

#### Scenario: Navigation links displayed
- **WHEN** cluster dashboard is displayed
- **THEN** navigation links to "Jobs", "Calendar", "Agents" SHALL be visible
- **AND** clicking each SHALL navigate to the corresponding page

### Requirement: Cluster Dashboard handles non-existent cluster
The cluster dashboard SHALL display an appropriate error when the cluster does not exist.

#### Scenario: Cluster not found
- **WHEN** user navigates to a non-existent cluster
- **THEN** the dashboard SHALL display "Cluster not found" message
- **AND** SHALL provide a link back to cluster list

### Requirement: Cluster Dashboard refreshes on interval
The cluster dashboard SHALL automatically refresh data at regular intervals.

#### Scenario: Auto-refresh
- **WHEN** user stays on cluster dashboard
- **THEN** data SHALL refresh every 30 seconds
- **AND** "Last updated" timestamp SHALL be displayed