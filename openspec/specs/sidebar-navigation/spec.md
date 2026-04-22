# Sidebar Navigation Specification

## Purpose

This specification defines the requirements for the Portainer-style sidebar navigation with cluster context switching.

**Status:** Implemented  
**Last Updated:** 2026-04-22

---

## Requirements

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

### Requirement: Sidebar supports keyboard navigation
The sidebar SHALL support keyboard shortcuts for quick navigation.

#### Scenario: Keyboard shortcuts
- **WHEN** user presses Alt+D
- **THEN** the application SHALL navigate to Dashboard
- **AND** **WHEN** user presses Alt+C
- **THEN** the application SHALL navigate to Clusters
- **AND** **WHEN** user presses Escape
- **THEN** the sidebar SHALL clear the selected cluster

---

## ADDED (2026-04-22) - Simplified Clusters Dropdown

### Requirement: Sidebar displays simplified clusters entry
The sidebar SHALL display a simplified "Clusters" entry without expanding to show all cluster navigation.

#### Scenario: Simplified clusters entry
- **WHEN** sidebar is rendered
- **THEN** the Clusters item SHALL show only:
  - Layers icon
  - "Clusters" label
  - Expand/collapse chevron

### Requirement: Sidebar clusters dropdown shows recent clusters
The sidebar SHALL show a dropdown of recently used clusters (up to 5) when Clusters is clicked.

#### Scenario: Recent clusters displayed
- **WHEN** user clicks Clusters
- **THEN** a dropdown SHALL appear showing:
  - Up to 5 most recently accessed clusters
  - Each cluster shows: name, status indicator
  - "View All Clusters" option linking to /clusters
  - "+ Add New Cluster" option linking to /clusters

### Requirement: Sidebar dropdown cluster items navigate correctly
Clicking a cluster in the dropdown SHALL navigate to that cluster's dashboard.

#### Scenario: Cluster navigation
- **WHEN** user clicks a cluster in the dropdown
- **THEN** the application SHALL navigate to /clusters/:clusterId
- **AND** the cluster tabs view SHALL be displayed

### Requirement: Sidebar dropdown closes on outside click
The sidebar dropdown SHALL close when clicking outside of it.

#### Scenario: Dropdown closes on outside click
- **WHEN** dropdown is open
- **AND** user clicks outside the dropdown
- **THEN** the dropdown SHALL close

### Requirement: Sidebar dropdown closes on navigation
The sidebar dropdown SHALL close when user navigates to a cluster.

#### Scenario: Dropdown closes on navigation
- **WHEN** dropdown is open
- **AND** user clicks a cluster
- **THEN** the dropdown SHALL close
- **AND** navigation to the cluster SHALL occur

### Requirement: Sidebar cluster tree is removed
The sidebar SHALL NOT display the nested cluster tree with Dashboard/Jobs/Calendar/Agents sub-items.

#### Scenario: No nested cluster tree
- **WHEN** sidebar is rendered
- **THEN** there SHALL NOT be sub-items under Clusters showing:
  - Dashboard
  - Jobs
  - Calendar
  - Agents
- **AND** navigation to these pages SHALL happen via ClusterTabs component in main content