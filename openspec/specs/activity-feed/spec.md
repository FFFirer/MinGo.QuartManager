# Activity Feed Specification

## Purpose

This specification defines the requirements for the real-time activity feed displayed on the Platform Dashboard, showing live events from Agents, Schedulers, and Jobs.

**Status:** New  
**Last Updated:** 2026-05-04

---

## ADDED Requirements

### Requirement: Dashboard displays real-time activity feed

The Platform Dashboard SHALL display a real-time activity feed showing recent system events.

#### Scenario: Activity feed renders
- **WHEN** Platform Dashboard loads
- **THEN** an "Activity Feed" section SHALL be displayed
- **AND** SHALL show the most recent 20 events
- **AND** each event SHALL show: event icon (color-coded), description, and relative timestamp ("2s ago", "1m ago")

#### Scenario: Event types and icons
- **WHEN** an event is displayed
- **THEN** SHALL use color-coded icons based on event type:
  - Agent online: green up-arrow icon
  - Agent offline: red down-arrow icon
  - Agent warning: amber warning icon
  - Job triggered: blue play icon
  - Job completed: green check icon
  - Job failed: red X icon
  - Job paused: amber pause icon

### Requirement: Activity feed updates in real-time (SSE)

The activity feed SHALL receive new events via Server-Sent Events (SSE) when available, with polling fallback.

#### Scenario: SSE connection established
- **WHEN** dashboard loads
- **THEN** a connection SHALL be opened to /api/events (SSE endpoint)
- **AND** new events SHALL appear in the feed without page refresh
- **AND** new events SHALL animate in (slide from top)

#### Scenario: SSE fallback to polling
- **WHEN** SSE connection fails or is not available
- **THEN** the feed SHALL fall back to polling every 15 seconds
- **AND** a subtle indicator SHALL show "Live" status (green dot when SSE active, grey dot when polling)

### Requirement: Activity feed supports auto-scroll and pause

The activity feed SHALL auto-scroll to show new events, with pause capability.

#### Scenario: Auto-scroll new events
- **WHEN** a new event arrives
- **AND** user has not manually scrolled up
- **THEN** the feed SHALL auto-scroll to show the new event

#### Scenario: Pause auto-scroll
- **WHEN** user scrolls up to view older events
- **THEN** auto-scroll SHALL pause
- **AND** a "New events" button SHALL appear
- **AND** clicking it SHALL scroll to the latest event and resume auto-scroll
