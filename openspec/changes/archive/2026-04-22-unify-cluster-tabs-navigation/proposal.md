# Proposal: Unify Cluster Tabs Navigation

## Why

The current UI has inconsistent navigation patterns for cluster operations. The sidebar shows a nested tree structure per cluster, while each cluster page has its own duplicate tab navigation. This creates cognitive load and maintenance overhead—the user doesn't know where to look, and code changes require updating multiple places.

## What Changes

- **Simplify Sidebar**: Remove the nested cluster tree from the sidebar. Keep only global entries (Dashboard, Clusters, Settings) and show a dropdown for recent clusters.

- **Create Unified ClusterTabs Component**: Extract the duplicate tab navigation from each cluster page into a single reusable component.

- **Refactor Cluster Pages**: Update Dashboard, Jobs, Calendar, and Agents pages to use the unified ClusterTabs component instead of inline tabs.

- **Enhance Clusters List Page**: The `/clusters` route already supports CRUD operations and acts as the entry point to individual clusters—no changes needed here.

## Capabilities

### New Capabilities

- `cluster-tabs`: Unified tab navigation component for cluster-level pages
  - Provides consistent look and behavior across Dashboard, Jobs, Calendar, and Agents pages
  - Includes cluster context header with status indicator
  - Accessible action buttons (Create Job, View Agents, etc.)

- `sidebar-cluster-dropdown`: Simplified sidebar cluster selector
  - Shows dropdown of recently used clusters (max 5)
  - Click to navigate directly to that cluster's Dashboard
  - Entry point to full cluster list at `/clusters`

### Modified Capabilities

None. This is a pure UI/UX refactor with no requirement changes to existing specs.

## Impact

- **Code**: `App.tsx` (sidebar), new `components/ClusterTabs.tsx`, four modified page files
- **Routes**: Unchanged—`/clusters/:id/*` continues to work
- **User Experience**: Consistent navigation regardless of where the user is in the cluster context