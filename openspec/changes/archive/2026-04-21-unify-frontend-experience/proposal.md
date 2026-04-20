## Why

The MinGo.Qap.UI frontend suffers from significant inconsistencies in user experience due to duplicated implementation patterns across pages. Status displays use different color mappings, navigation/back buttons behave inconsistently, and common UI elements like tables and headers are reimplemented in each page. This creates a confusing user experience, increases maintenance burden, and makes the application feel unpolished. Unifying these patterns will improve usability and development efficiency.

## What Changes

- Create reusable UI components: StatusBadge, PageHeader, DataTable, ConfirmDialog
- Standardize page header layout with consistent title, breadcrumb, and action placement
- Fix navigation logic to ensure back buttons and breadcrumbs point to correct parent pages
- Unify status display logic with a single StatusBadge component
- Replace duplicated table implementations with a unified DataTable component
- Standardize loading/error states across all pages
- Ensure consistent button styling and interaction patterns

## Capabilities

### New Capabilities
- `ui-status-badge`: Unified status display component with consistent color mapping
- `ui-page-header`: Standardized page header with title, breadcrumbs, back navigation, and actions
- `ui-data-table`: Reusable data table component with consistent styling and behavior
- `ui-confirm-dialog`: Standardized confirmation dialog for destructive actions
- `ui-layout-wrapper`: Layout component providing consistent page structure

### Modified Capabilities
- None (this change focuses on implementation consistency, not requirement changes)

## Impact

- Affects all pages in MinGo.Qap.UI: ClustersPage, ClusterDetailPage, JobsPage, JobDetailPage, AgentInstancesPage
- Replaces duplicated status color functions, header layouts, table implementations, and confirmation dialogs
- Modifies routing/navigation logic in pages to use consistent breadcrumb patterns
- Updates component imports and JSX across approximately 5-6 files
- No API or backend changes required
- No breaking changes to existing functionality - purely UI/UX consistency improvements