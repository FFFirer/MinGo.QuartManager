# Global Search Specification

## Purpose

This specification defines the requirements for the global search feature that allows users to quickly find Agents, Schedulers, and Jobs across the entire platform using a keyboard-activated search panel.

**Status:** New  
**Last Updated:** 2026-05-04

---

## ADDED Requirements

### Requirement: Global search is activated via keyboard shortcut

The global search panel SHALL be activated by pressing Ctrl+K (Windows/Linux) or ⌘K (macOS).

#### Scenario: Open search with keyboard
- **WHEN** user presses Ctrl+K or ⌘K
- **AND** no input element is currently focused
- **THEN** the global search panel SHALL open
- **AND** the search input SHALL be auto-focused

#### Scenario: Close search with Escape
- **WHEN** global search panel is open
- **AND** user presses Escape
- **THEN** the global search panel SHALL close

#### Scenario: Close search on click outside
- **WHEN** global search panel is open
- **AND** user clicks outside the panel
- **THEN** the global search panel SHALL close

### Requirement: Global search searches across Agents, Schedulers, and Jobs

The global search panel SHALL search across all three resource types simultaneously, using Fuse.js for client-side fuzzy matching.

#### Scenario: Search results grouped by type
- **WHEN** user types a search query
- **THEN** results SHALL be displayed grouped by resource type (Agents, Schedulers, Jobs)
- **AND** each group SHALL show up to 5 results by default
- **AND** "View all X results" link SHALL be shown when more results exist

#### Scenario: Agent search matching
- **WHEN** user types a query matching agent name or ID
- **THEN** matching agents SHALL appear in the Agents group
- **AND** each result SHALL show: agent name, status indicator, and URL

#### Scenario: Scheduler search matching
- **WHEN** user types a query matching scheduler name or instance ID
- **THEN** matching schedulers SHALL appear in the Schedulers group
- **AND** each result SHALL show: scheduler name and status

#### Scenario: Job search matching
- **WHEN** user types a query matching job key or job type
- **THEN** matching jobs SHALL appear in the Jobs group
- **AND** each result SHALL show: job key, type, status, and scheduler name

### Requirement: Search results support keyboard navigation

The global search panel SHALL support navigating results using arrow keys.

#### Scenario: Navigate results with keyboard
- **WHEN** global search panel is open
- **AND** user presses Arrow Down
- **THEN** the next result SHALL be highlighted
- **AND** **WHEN** user presses Arrow Up
- **THEN** the previous result SHALL be highlighted
- **AND** **WHEN** user presses Enter
- **THEN** the highlighted result SHALL open in the appropriate detail page
