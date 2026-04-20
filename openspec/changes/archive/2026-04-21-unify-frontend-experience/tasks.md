## 1. Component Creation

- [x] 1.1 Create StatusBadge component (src/MinGo.Qap.UI/src/components/StatusBadge.tsx)
- [x] 1.2 Create PageHeader component (src/MinGo.Qap.UI/src/components/PageHeader.tsx)
- [x] 1.3 Create DataTable component (src/MinGo.Qap.UI/src/components/DataTable.tsx)
- [x] 1.4 Create ConfirmDialog component (src/MinGo.Qap.UI/src/components/ConfirmDialog.tsx)
- [x] 1.5 Create LayoutWrapper component (src/MinGo.Qap.UI/src/components/LayoutWrapper.tsx) - optional

## 2. ClustersPage Migration (Simplest Page)

- [x] 2.1 Replace status display with StatusBadge component
- [x] 2.2 Replace header with PageHeader component (no breadcrumbs/back needed)
- [x] 2.3 Standardize loading/error state handling
- [x] 2.4 Verify functionality matches original

## 3. ClusterDetailPage Migration

- [x] 3.1 Replace status display with StatusBadge component
- [x] 3.2 Replace header with PageHeader component (add back navigation to clusters page)
- [x] 3.3 Standardize loading/error state handling
- [x] 3.4 Verify navigation and functionality

## 4. JobsPage Migration

- [x] 4.1 Replace status display with StatusBadge component (fix offline color to match others)
- [x] 4.2 Replace header with PageHeader component (fix back navigation to point to cluster detail)
- [x] 4.3 Replace table implementation with DataTable component
- [x] 4.4 Replace window.confirm with ConfirmDialog for delete actions
- [x] 4.5 Standardize loading/error state handling
- [x] 4.6 Verify navigation, table functionality, and delete confirmation

## 5. JobDetailPage Migration (Most Complex)

- [x] 5.1 Replace status display with StatusBadge component
- [x] 5.2 Replace header with PageHeader component (add back navigation to jobs page)
- [x] 5.3 Replace window.confirm with ConfirmDialog for delete and other actions
- [x] 5.4 Standardize loading/error state handling
- [x] 5.5 Verify all functionality including job actions (pause/resume/trigger/delete)

## 6. AgentInstancesPage Migration

- [x] 6.1 Keep existing breadcrumb structure but fix links (Clusters link should point to clusters list)
- [x] 6.2 Replace status display with StatusBadge component
- [x] 6.3 Standardize header/action area using PageHeader concepts if beneficial
- [x] 6.4 Replace table implementation with DataTable component
- [x] 6.5 Replace inline confirmation with ConfirmDialog for agent deletion
- [x] 6.6 Standardize loading/error state handling
- [x] 6.7 Verify navigation and table functionality

## 7. Cleanup and Finalization

- [x] 7.1 Remove duplicated getStatusColor functions from all pages
- [x] 7.2 Remove duplicated table CSS classes and inline styles where replaced by components
- [x] 7.3 Remove duplicated confirmation dialog patterns
- [x] 7.4 Update imports in all migrated pages
- [x] 7.5 Run application to verify all pages work correctly
- [x] 7.6 Perform final consistency check across all pages
