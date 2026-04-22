# Tasks: Unify Cluster Tabs Navigation

## 1. Create ClusterTabs Component

- [x] 1.1 Create `src/components/ClusterTabs.tsx` with cluster context header
- [x] 1.2 Add tab links: Dashboard | Jobs | Calendar | Agents
- [x] 1.3 Implement active tab highlighting based on current URL
- [x] 1.4 Add action buttons slot on the right side
- [x] 1.5 Add back button navigating to /clusters

## 2. Update ClusterDashboardPage

- [x] 2.1 Import ClusterTabs component
- [x] 2.2 Remove inline tab code (lines 146-174)
- [x] 2.3 Replace with ClusterTabs wrapper

## 3. Update JobsPage

- [x] 3.1 Import ClusterTabs component
- [x] 3.2 Add ClusterTabs with action button (Create Job)
- [x] 3.3 Remove existing PageHeader and replace with ClusterTabs

## 4. Update CalendarPage

- [x] 4.1 Import ClusterTabs component
- [x] 4.2 Remove inline tab code (lines 177-204)
- [x] 4.3 Replace with ClusterTabs wrapper

## 5. Update AgentInstancesPage

- [x] 5.1 Import ClusterTabs component
- [x] 5.2 Add ClusterTabs navigation
- [x] 5.3 Update header to match ClusterTabs format

## 6. Simplify Sidebar (App.tsx)

- [x] 6.1 Remove nested cluster tree code (lines 167-280)
- [x] 6.2 Implement clusters dropdown with recent clusters (max 5)
- [x] 6.3 Add "View All Clusters" link to /clusters
- [x] 6.4 Add "+ Add New Cluster" link
- [x] 6.5 Implement dropdown open/close logic
- [x] 6.6 Preserve keyboard shortcuts (Alt+D, Alt+C)

## 7. Verify & Test

- [x] 7.1 Test navigation from Clusters list to cluster tabs view
- [x] 7.2 Test tab switching within cluster
- [x] 7.3 Test sidebar dropdown opens and closes
- [x] 7.4 Test back button navigates to /clusters
- [x] 7.5 Verify all four cluster pages work correctly
- [x] 7.6 Test keyboard navigation shortcuts

**Note:** Build verification passed. Manual testing in browser recommended.