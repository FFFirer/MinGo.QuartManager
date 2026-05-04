## MODIFIED Requirements

### Requirement: Sidebar displays Dashboard, Agents, Schedulers navigation

The sidebar SHALL display navigation items (Dashboard, Agents, Schedulers, Executions) as primary navigation. Calendar and Settings items are REMOVED from the sidebar.

#### Scenario: Base navigation displays
- **WHEN** sidebar is rendered
- **THEN** the sidebar SHALL show:
  - Dashboard (icon: LayoutDashboard, route: /)
  - Agents (icon: Server, route: /agents)
  - Schedulers (icon: Layers, route: /schedulers)
  - Executions (icon: Activity, route: placeholder)

### Requirement: Sidebar supports keyboard navigation

The sidebar SHALL support keyboard shortcuts for quick navigation. Alt+C shortcut is REMOVED.

#### Scenario: Keyboard shortcuts
- **WHEN** user presses Alt+D
- **THEN** the application SHALL navigate to Dashboard
- **AND** **WHEN** user presses Alt+A
- **THEN** the application SHALL navigate to Agents
- **AND** **WHEN** user presses Alt+S
- **THEN** the application SHALL navigate to Schedulers
- **AND** **WHEN** user presses Alt+E
- **THEN** the application SHALL navigate to Executions

## REMOVED Requirements

### Requirement: Calendar sidebar navigation item

**Reason**: Calendar page removed due to backend API not being implemented. The page was non-functional.
**Migration**: No migration needed. Calendar was never accessible.

### Requirement: Settings sidebar navigation item

**Reason**: Settings page never implemented and no route defined. Link was broken (led to 404).
**Migration**: No migration needed. Settings was never functional.

### Requirement: Alt+C keyboard shortcut

**Reason**: Calendar route removed. Shortcut pointed to /schedulers which was misleading.
**Migration**: Use Alt+S for Schedulers or navigate to Schedulers via sidebar.
