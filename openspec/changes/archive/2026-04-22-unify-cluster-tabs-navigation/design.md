# Design: Unify Cluster Tabs Navigation

## Context

**Current State:**
- Sidebar contains nested tree structure per cluster (lines 167-280 in App.tsx)
- Each cluster page has inline duplicate tab code
- Inconsistent navigation: sidebar for some actions, tabs for others

**Stakeholders:**
- Users who manage multiple clusters
- Developers maintaining navigation code

## Goals / Non-Goals

**Goals:**
- Unified, consistent navigation across all cluster pages
- Reduced code duplication (extract once, use everywhere)
- Simplified sidebar focusing on global navigation
- Clear entry point to clusters list at `/clusters`

**Non-Goals:**
- Route changes (keep `/clusters/:id/*`)
- Platform dashboard removal (keep `/`)
- Full responsive/mobile design (deferred)

## Decisions

### 1. Unified ClusterTabs Component

**Decision:** Create `components/ClusterTabs.tsx` that encapsulates:
- Cluster name and status badge
- Tab links: Dashboard | Jobs | Calendar | Agents
- Action buttons (Create Job, View Agents, etc.)

**Rationale:** Single source of truth for cluster navigation. Changes only need to happen in one place.

### 2. Sidebar Simplified

**Decision:** Remove nested cluster tree, use dropdown:
```
Clusters ▼  →  [Cluster A] [Cluster B] ... [+ Add]
```

**Rationale:**
- Reduced visual clutter
- Recent clusters are more common workflow than switching between many
- Clear path to full list via `/clusters`

**Alternatives Considered:**
- A) Keep tree for ALL clusters - rejected, too cluttered
- B) Use tabs without cluster selector - rejected, lose cluster context

### 3. Preserve Route Structure

**Decision:** Keep existing routes unchanged:
- `/` → PlatformDashboard
- `/clusters` → ClustersPage (list CRUD)
- `/clusters/:id` → ClusterDashboard
- `/clusters/:id/jobs` → Jobs
- `/clusters/:id/calendar` → Calendar
- `/clusters/:id/agents` → Agents

**Rationale:** Minimize risk. No backend changes needed—just UI refactor.

### 4. Cluster Context via React Context

**Decision:** Maintain `SidebarContext` for selected cluster state:
- Persisted in localStorage for convenience
- Not required (URL takes precedence)

**Rationale:** Smooth UX, not required for core functionality.

## Risks / Trade-offs

| Risk | Mitigation |
|------|-----------|
| **Tabs not fully aligned** | Use Component for ALL pages |
| **Back button goes wrong place** | Back to `/clusters` (per user input) |
| **Breaking existing shortcuts** | Keep Alt+D, Alt+C, Esc hotkeys |

## Migration Plan

1. Create `ClusterTabs.tsx` component
2. Update 4 cluster pages to use it
3. Simplify Sidebar in App.tsx
4. Verify navigation works end-to-end

**Rollback:** Each step is independent—we can keep inline tabs as fallback if needed. Since this is pure UI with no data changes, rollback is straightforward (revert files).