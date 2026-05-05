# Sidebar Navigation Specification

## Purpose

This specification defines the requirements for the sidebar navigation with Agent and Scheduler navigation, including collapsible states and responsive behavior.

**Status:** Updated  
**Last Updated:** 2026-05-05

---

## MODIFIED Requirements

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

#### Scenario: Collapsed sidebar hides scrollbar
- **WHEN** sidebar is in collapsed state
- **THEN** the nav area SHALL NOT display a vertical scrollbar
- **AND** the sidebar `<aside>` SHALL clip any overflowing content

#### Scenario: Expanded sidebar shows scrollbar when needed
- **WHEN** sidebar is in expanded state
- **AND** navigation content overflows vertically
- **THEN** the nav area SHALL show a vertical scrollbar

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
