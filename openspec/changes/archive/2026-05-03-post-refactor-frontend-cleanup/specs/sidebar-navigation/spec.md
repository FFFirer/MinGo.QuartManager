# Sidebar Navigation Specification

## Purpose

This specification defines the requirements for the sidebar navigation with Agent and Scheduler navigation.

**Status:** Updated  
**Last Updated:** 2026-05-03

---

## ADDED Requirements

### Requirement: Sidebar displays Dashboard, Agents, Schedulers navigation
The sidebar SHALL display navigation items (Dashboard, Agents, Schedulers, Settings) as primary navigation.

#### Scenario: Base navigation displays
- **WHEN** sidebar is rendered
- **THEN** the sidebar SHALL show:
  - Dashboard (icon: LayoutDashboard, route: /)
  - Agents (icon: Server) with dropdown of recent agents
  - Schedulers (icon: Layers, route: /schedulers)
  - Settings (icon: Settings, route: /settings)

### Requirement: Sidebar agents dropdown shows recent agents
The sidebar SHALL show a dropdown of recently accessed agents (up to 5) when Agents is clicked.

#### Scenario: Recent agents displayed
- **WHEN** user clicks Agents
- **THEN** a dropdown SHALL appear showing:
  - Up to 5 most recently accessed agents
  - Each agent shows: name, status indicator (dot: green/amber/red)
  - "View All Agents" option linking to /agents

### Requirement: Sidebar dropdown closes on outside click
The sidebar dropdown SHALL close when clicking outside of it.

#### Scenario: Dropdown closes on outside click
- **WHEN** dropdown is open
- **AND** user clicks outside the dropdown
- **THEN** the dropdown SHALL close

### Requirement: Sidebar dropdown closes on navigation
The sidebar dropdown SHALL close when user navigates to an agent.

#### Scenario: Dropdown closes on navigation
- **WHEN** dropdown is open
- **AND** user clicks an agent
- **THEN** the dropdown SHALL close
- **AND** navigation to the agent SHALL occur

### Requirement: Sidebar supports keyboard navigation
The sidebar SHALL support keyboard shortcuts for quick navigation.

#### Scenario: Keyboard shortcuts
- **WHEN** user presses Alt+D
- **THEN** the application SHALL navigate to Dashboard
- **AND** **WHEN** user presses Alt+A
- **THEN** the application SHALL navigate to Agents
- **AND** **WHEN** user presses Alt+S
- **THEN** the application SHALL navigate to Schedulers

## MODIFIED Requirements

### Requirement: Sidebar highlights active menu item
The sidebar SHALL highlight the currently active page in the navigation.

**Change**: Active detection applies to /agents/* and /schedulers/* routes

#### Scenario: Active item highlighted
- **WHEN** user is on a page matching a sidebar route
- **THEN** the corresponding sidebar item SHALL be visually highlighted

## REMOVED Requirements

### Requirement: Sidebar displays base navigation when no cluster selected
**Reason**: Cluster concept removed in v2.0.0. Replaced by direct Agents/Schedulers navigation.
**Migration**: See "Sidebar displays Dashboard, Agents, Schedulers navigation" above.

### Requirement: Sidebar displays cluster context after selection
**Reason**: Cluster concept removed. No cluster context mode.
**Migration**: Navigation is flat (Dashboard → Agents → Schedulers).

### Requirement: Sidebar allows returning to cluster list
**Reason**: Cluster concept removed.
**Migration**: Schedulers link navigates to /schedulers.

### Requirement: Sidebar shows cluster status indicator
**Reason**: Cluster concept removed.
**Migration**: Agent dropdown shows per-agent status indicators.

### Requirement: Sidebar persists selection in localStorage
**Reason**: Cluster concept removed.
**Migration**: No selection persistence needed for flat navigation.

### Requirement: Sidebar shows cluster list under Clusters
**Reason**: Cluster concept removed. Agents replaced cluster in sidebar.
**Migration**: See agents dropdown above.

### Requirement: Sidebar handles no clusters gracefully
**Reason**: Cluster concept removed.
**Migration**: Agent dropdown handles empty state naturally.

### Requirement: Sidebar displays simplified clusters entry
**Reason**: Cluster concept removed.
**Migration**: See Agents entry.

### Requirement: Sidebar clusters dropdown shows recent clusters
**Reason**: Cluster concept removed.
**Migration**: See agents dropdown.

### Requirement: Sidebar dropdown cluster items navigate correctly
**Reason**: Cluster concept removed.
**Migration**: Agent items navigate to /agents/{agentId}.

### Requirement: Sidebar cluster tree is removed
**Reason**: This requirement is now implicit (no cluster tree exists).
**Migration**: No additional action needed.
