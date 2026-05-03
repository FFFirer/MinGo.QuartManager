# Sidebar Navigation Specification (Delta)

## Purpose

This delta spec updates the sidebar navigation to support collapsible states, responsive behavior, and integration with the global search and status bar.

**Status:** Delta  
**Last Updated:** 2026-05-04

---

## ADDED Requirements

### Requirement: Sidebar supports collapsed and expanded states

The sidebar SHALL support a collapsed state (w-16, icons only) and expanded state (w-64, icons + labels).

#### Scenario: Toggle collapse
- **WHEN** user clicks the collapse toggle button in the sidebar footer
- **THEN** the sidebar SHALL animate between expanded (w-64) and collapsed (w-16)
- **AND** CSS transition duration SHALL be ~200ms
- **AND** collapsed state SHALL show only navigation icons

#### Scenario: Collapsed sidebar tooltip
- **WHEN** sidebar is collapsed
- **AND** user hovers over a nav item for 500ms
- **THEN** a tooltip SHALL appear showing the nav item label

#### Scenario: Collapse state persisted
- **WHEN** user toggles sidebar
- **THEN** the state SHALL be saved to localStorage
- **AND** restored on next page load

### Requirement: Sidebar auto-collapses on small screens

The sidebar SHALL automatically collapse when viewport width is below the lg breakpoint.

#### Scenario: Responsive collapse
- **WHEN** viewport width is less than 1024px
- **THEN** sidebar SHALL be collapsed
- **AND** a hamburger button SHALL appear in the top-left corner

#### Scenario: Mobile overlay
- **WHEN** viewport width is less than 768px
- **AND** hamburger button is clicked
- **THEN** sidebar SHALL appear as an overlay panel with semi-transparent backdrop
- **AND** clicking backdrop SHALL close the sidebar

### Requirement: Sidebar integrates with status bar

The sidebar SHALL integrate with the bottom status bar.

#### Scenario: Status bar visible
- **WHEN** sidebar is collapsed
- **THEN** the status bar SHALL span the full width
- **AND** **WHEN** sidebar is expanded
- **THEN** the status bar SHALL be offset by the sidebar width

## MODIFIED Requirements

### Requirement: Sidebar displays Dashboard, Agents, Schedulers navigation

The sidebar SHALL display navigation items (Dashboard, Agents, Schedulers, Executions, Settings) as primary navigation.

**Change**: Added "Executions" (icon: Activity, route: /executions) and "Calendar" (icon: Calendar, route: /schedulers/:name/calendar) to navigation items

#### Scenario: Base navigation displays
- **WHEN** sidebar is rendered
- **THEN** the sidebar SHALL show:
  - Dashboard (icon: LayoutDashboard, route: /)
  - Agents (icon: Server) with dropdown of recent agents
  - Schedulers (icon: Layers, route: /schedulers)
  - Calendar (icon: Calendar, route context-aware)
  - Executions (icon: Activity, route: placeholder)
  - Settings (icon: Settings, route: /settings)

### Requirement: Sidebar supports keyboard navigation

The sidebar SHALL support keyboard shortcuts for quick navigation.

**Change**: Added Alt+E for Executions, Alt+C for Calendar shortcuts

#### Scenario: Keyboard shortcuts
- **WHEN** user presses Alt+D
- **THEN** the application SHALL navigate to Dashboard
- **AND** **WHEN** user presses Alt+A
- **THEN** the application SHALL navigate to Agents
- **AND** **WHEN** user presses Alt+S
- **THEN** the application SHALL navigate to Schedulers
- **AND** **WHEN** user presses Alt+E
- **THEN** the application SHALL navigate to Executions
- **AND** **WHEN** user presses Alt+C
- **THEN** the application SHALL navigate to Calendar
