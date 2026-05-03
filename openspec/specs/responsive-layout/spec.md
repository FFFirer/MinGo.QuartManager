# Responsive Layout Specification

## Purpose

This specification defines the requirements for the responsive layout system, including collapsible sidebar, breadcrumb bar, and bottom status bar.

**Status:** New  
**Last Updated:** 2026-05-04

---

## ADDED Requirements

### Requirement: Sidebar supports collapsed and expanded states

The sidebar SHALL support two visual states: expanded (w-64, showing icons + labels) and collapsed (w-16, showing icons only).

#### Scenario: Toggle sidebar collapse
- **WHEN** user clicks the collapse toggle button in the sidebar
- **THEN** the sidebar SHALL animate between expanded and collapsed states
- **AND** the transition SHALL use CSS transition (smooth, ~200ms)

#### Scenario: Collapsed sidebar shows tooltips
- **WHEN** sidebar is collapsed
- **AND** user hovers over a navigation icon
- **THEN** a tooltip SHALL appear showing the navigation item label

#### Scenario: Sidebar collapse state persists
- **WHEN** user toggles sidebar collapse state
- **AND** user navigates to another page
- **THEN** the sidebar SHALL maintain its collapsed/expanded state
- **AND** the preference SHALL be persisted in localStorage

### Requirement: Layout auto-collapses on small screens

The sidebar SHALL automatically collapse when viewport width is below the lg breakpoint (< 1024px).

#### Scenario: Auto-collapse on small screen
- **WHEN** viewport width is less than 1024px
- **THEN** the sidebar SHALL be in collapsed state
- **AND** a hamburger menu button SHALL appear in the top-left corner
- **AND** clicking the hamburger button SHALL temporarily expand the sidebar as an overlay

#### Scenario: Mobile overlay sidebar
- **WHEN** viewport width is less than 768px
- **AND** hamburger button is clicked
- **THEN** the sidebar SHALL appear as a full-height overlay panel
- **AND** a semi-transparent backdrop SHALL cover the main content
- **AND** clicking the backdrop SHALL close the sidebar

### Requirement: Application displays bottom status bar

The application SHALL display a bottom status bar showing system health and connection information.

#### Scenario: Status bar information
- **WHEN** application renders
- **THEN** the status bar SHALL display at the bottom of the window
- **AND** SHALL show: system health indicator (green/amber/red), last refresh timestamp, and application version
- **AND** clicking the refresh timestamp SHALL trigger a data refresh

### Requirement: Application shows dynamic breadcrumb navigation

The application SHALL display breadcrumb navigation derived from the current route.

#### Scenario: Breadcrumb derived from route
- **WHEN** user navigates to /schedulers/DefaultScheduler/jobs
- **THEN** breadcrumb SHALL show: Schedulers > DefaultScheduler > Jobs
- **AND** each part SHALL be a clickable link except the current page
- **AND** breadcrumb SHALL update automatically on route change
