# Sidebar Cluster Dropdown Specification

## Purpose

This specification defines the requirements for the simplified sidebar cluster dropdown selector.

**Status:** Added  
**Last Updated:** 2026-04-22

---

## ADDED Requirements

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