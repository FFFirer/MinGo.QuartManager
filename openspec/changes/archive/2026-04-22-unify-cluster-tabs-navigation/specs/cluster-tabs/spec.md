# Cluster Tabs Specification

## Purpose

This specification defines the requirements for the unified cluster tabs navigation component.

**Status:** Added  
**Last Updated:** 2026-04-22

---

## ADDED Requirements

### Requirement: ClusterTabs component displays cluster context
The ClusterTabs component SHALL display the current cluster's name and status when mounted in cluster context.

#### Scenario: Cluster context header displays
- **WHEN** ClusterTabs is rendered on a cluster page
- **THEN** the component SHALL display:
  - Cluster name (text)
  - Status indicator dot (colored by status: green/amber/red)
  - Optional: environment label

### Requirement: ClusterTabs component renders navigation tabs
The ClusterTabs component SHALL render tab links for navigating within a cluster.

#### Scenario: Tab links render
- **WHEN** ClusterTabs is rendered
- **THEN** the component SHALL display tabs:
  - Dashboard (icon: LayoutDashboard, route: /clusters/:id)
  - Jobs (icon: Clock, route: /clusters/:id/jobs)
  - Calendar (icon: Calendar, route: /clusters/:id/calendar)
  - Agents (icon: Server, route: /clusters/:id/agents)

### Requirement: ClusterTabs component highlights active tab
The ClusterTabs component SHALL highlight the tab corresponding to the current URL.

#### Scenario: Active tab highlighted
- **WHEN** user is on /clusters/:id/jobs
- **THEN** the Jobs tab SHALL have active styling (e.g., blue text, border-bottom)
- **AND** other tabs SHALL have default styling

### Requirement: ClusterTabs component supports action buttons
The ClusterTabs component SHALL support rendering optional action buttons on the right side.

#### Scenario: Action buttons displayed
- **WHEN** action buttons are provided as props
- **THEN** the buttons SHALL be rendered on the right side of the tabs
- **AND** each button SHALL trigger the provided callback when clicked

### Requirement: ClusterTabs component is reusable across cluster pages
The ClusterTabs component SHALL be used consistently on Dashboard, Jobs, Calendar, and Agents pages.

#### Scenario: Consistent usage
- **WHEN** any cluster page is loaded (/clusters/:id/*)
- **THEN** the page SHALL render ClusterTabs instead of inline tab code
- **AND** the tabs SHALL navigate correctly to sibling routes

### Requirement: ClusterTabs component renders back button
The ClusterTabs component SHALL render a back button to navigate to the clusters list.

#### Scenario: Back button present
- **WHEN** ClusterTabs is rendered
- **THEN** a back button SHALL be visible
- **AND** clicking it SHALL navigate to /clusters