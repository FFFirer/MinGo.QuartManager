# Sidebar Navigation Specification — Delta

> Delta spec for change `sidebar-first-level-agents`.
> Modifies `openspec/specs/sidebar-navigation/spec.md`.

**Status:** Updated  
**Last Updated:** 2026-05-04

---

## MODIFIED Requirements

### Requirement: Sidebar displays Dashboard, Agents, Schedulers navigation

The sidebar SHALL display navigation items (Dashboard, Agents, Schedulers, Calendar, Executions, Settings) as primary navigation.

#### Scenario: Base navigation displays
- **WHEN** sidebar is rendered
- **THEN** the sidebar SHALL show:
  - Dashboard (icon: LayoutDashboard, route: /)
  - Agents (icon: Server, route: /agents)
  - Schedulers (icon: Layers, route: /schedulers)
  - Calendar (icon: Calendar, route context-aware)
  - Executions (icon: Activity, route: placeholder)
  - Settings (icon: Settings, route: /settings)

> **Change**: Agents no longer has a dropdown. Changed from "with dropdown of recent agents" to direct route `/agents`.

## REMOVED Requirements

### Requirement: Sidebar agents dropdown shows recent agents

**Reason**: Agents changed from dropdown to first-level navigation item. Quick access to recent agents is no longer provided via sidebar; users navigate to `/agents` list page instead.

**Migration**: Remove all dropdown-related code from Sidebar component. Agents now navigates directly to `/agents`.

### Requirement: Sidebar dropdown closes on outside click

**Reason**: Dropdown no longer exists. This behavior is no longer needed.

**Migration**: Remove click-outside event listener and `dropdownRef`.

### Requirement: Sidebar dropdown closes on navigation

**Reason**: Dropdown no longer exists. This behavior is no longer needed.

**Migration**: Remove `useEffect` that closes dropdown on navigation.
