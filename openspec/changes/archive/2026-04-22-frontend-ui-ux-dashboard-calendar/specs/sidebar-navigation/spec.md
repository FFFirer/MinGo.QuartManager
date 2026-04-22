## ADDED Requirements

### Requirement: Sidebar displays base navigation when no cluster selected
The sidebar SHALL display base navigation items (Dashboard, Clusters, Settings) when no cluster is selected.

#### Scenario: Base navigation displays
- **WHEN** user is on platform dashboard or clusters list
- **THEN** the sidebar SHALL show:
  - Dashboard (icon: LayoutDashboard)
  - Clusters (icon: Layers) with expandable sub-menu
  - Settings (icon: Settings)

### Requirement: Sidebar displays cluster context after selection
The sidebar SHALL switch to cluster context mode when a cluster is selected.

#### Scenario: Cluster context activates
- **WHEN** user selects a cluster (clicks cluster card)
- **THEN** the sidebar SHALL:
  - Collapse the base "Clusters" item
  - Add a new cluster context item showing cluster name
  - Expand to show cluster sub-menu: Dashboard, Jobs, Calendar, Agents

### Requirement: Sidebar highlights active menu item
The sidebar SHALL highlight the currently active page in the navigation.

#### Scenario: Active item highlighted
- **WHEN** user navigates to a page
- **THEN** the corresponding sidebar item SHALL be visually highlighted
- **AND** the parent sub-menu SHALL be expanded

### Requirement: Sidebar allows returning to cluster list
The sidebar SHALL allow users to return to the clusters list from cluster context.

#### Scenario: Return to clusters
- **WHEN** user is in cluster context
- **AND** user clicks on "Clusters" at top of sidebar
- **THEN** the application SHALL navigate to /clusters
- **AND** the sidebar SHALL return to base navigation mode

### Requirement: Sidebar expands sub-menus based on current route
The sidebar SHALL automatically expand relevant sub-menus based on the current URL.

#### Scenario: Auto-expand on route change
- **WHEN** user navigates to /clusters/:id/jobs
- **THEN** the Clusters sub-menu SHALL be expanded
- **AND** the Jobs item SHALL be highlighted

### Requirement: Sidebar shows cluster status indicator
The sidebar SHALL display a status indicator for the selected cluster.

#### Scenario: Status indicator visible
- **WHEN** a cluster is selected in sidebar
- **THEN** a status dot SHALL be shown next to cluster name
- **AND** color SHALL reflect status (green=Online, amber=Warning, red=Offline)

### Requirement: Sidebar persists selection in localStorage
The sidebar SHALL persist the selected cluster in localStorage.

#### Scenario: Selection persisted
- **WHEN** user selects a cluster
- **THEN** the selection SHALL be saved to localStorage
- **AND** on page reload, the sidebar SHALL restore the previous selection

### Requirement: Sidebar shows cluster list under Clusters
The Clusters menu SHALL display a list of available clusters.

#### Scenario: Clusters list displays
- **WHEN** user expands Clusters menu
- **THEN** the menu SHALL show all available clusters
- **AND** each cluster SHALL show name and status indicator

### Requirement: Sidebar handles no clusters gracefully
The sidebar SHALL display appropriate message when no clusters exist.

#### Scenario: No clusters
- **WHEN** no clusters exist
- **AND** user expands Clusters menu
- **THEN** the menu SHALL show "No clusters" message

### Requirement: Sidebar is responsive to window resize
The sidebar SHALL adjust its appearance based on window width.

#### Scenario: Responsive collapse
- **WHEN** window width is less than 768px
- **THEN** the sidebar SHALL collapse to icon-only mode
- **AND** SHALL show full text when hovered or clicked