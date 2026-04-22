## 1. Infrastructure Setup

- [x] 1.1 Install frontend dependencies: react-hot-toast, cron-parser, react-calendar
- [x] 1.2 Create Toast notification component with success/error/warning/loading states
- [x] 1.3 Integrate Toast provider in App.tsx
- [x] 1.4 Create StatsCard reusable component
- [x] 1.5 Create UpcomingJobsList reusable component
- [x] 1.6 Create LoadingSkeleton reusable component

## 2. Backend API - Dashboard

- [x] 2.1 Create DashboardController in MinGo.Qap.Platform
- [x] 2.2 Implement GET /api/dashboard endpoint (platform-level aggregation)
- [x] 2.3 Implement GET /api/clusters/{clusterId}/dashboard endpoint
- [x] 2.4 Implement GET /api/clusters/{clusterId}/calendar endpoint
- [x] 2.5 Add DashboardDto, ClusterDashboardDto, CalendarDto models

## 3. Sidebar Navigation

- [x] 3.1 Refactor Sidebar component to support dynamic menu items
- [x] 3.2 Implement useSidebar hook for state management
- [x] 3.3 Add cluster selection logic (click cluster card → set selected cluster)
- [x] 3.4 Implement cluster context menu (Dashboard/Jobs/Calendar/Agents)
- [x] 3.5 Add auto-expand based on current route
- [x] 3.6 Implement localStorage persistence for selected cluster
- [x] 3.7 Add status indicator to cluster name in sidebar

## 4. Platform Dashboard

- [x] 4.1 Create PlatformDashboardPage component
- [x] 4.2 Implement dashboard API hook (usePlatformDashboard)
- [x] 4.3 Build platform overview statistics section
- [x] 4.4 Build cluster cards grid
- [x] 4.5 Build job status distribution chart
- [x] 4.6 Build agent health distribution chart
- [x] 4.7 Build upcoming jobs list
- [x] 4.8 Add loading skeleton states
- [x] 4.9 Add error handling with retry
- [x] 4.10 Configure route at /

## 5. Cluster Dashboard

- [x] 5.1 Create ClusterDashboardPage component
- [x] 5.2 Implement cluster dashboard API hook (useClusterDashboard)
- [x] 5.3 Build cluster header with status, env, date
- [x] 5.4 Build job summary statistics
- [x] 5.5 Build agent summary statistics
- [x] 5.6 Build recent agents list (max 5)
- [x] 5.7 Build upcoming jobs list for cluster
- [x] 5.8 Add execution history placeholder section
- [x] 5.9 Add navigation links (Jobs, Calendar, Agents)
- [x] 5.10 Configure route at /clusters/:clusterId
- [x] 5.11 Update ClusterDetailPage to redirect to dashboard

## 6. Cluster Calendar

- [x] 6.1 Create CalendarPage component
- [x] 6.2 Create CalendarView component with Month/Week/List modes
- [x] 6.3 Implement calendar API hook (useClusterCalendar)
- [x] 6.4 Implement fire time calculation using cron-parser
- [x] 6.5 Build calendar grid with job indicators
- [x] 6.6 Build job hover tooltip
- [x] 6.7 Build job click action menu (View Details, Trigger, Pause/Resume)
- [x] 6.8 Add month/week/list view switching
- [x] 6.9 Add navigation (prev/next month)
- [x] 6.10 Configure route at /clusters/:clusterId/calendar

## 7. Unified Create Flow

- [x] 7.1 Refactor CreateClusterModal to 4-step wizard pattern
- [x] 7.2 Add progress indicator to CreateClusterModal
- [x] 7.3 Add step validation logic
- [x] 7.4 Add back/next navigation
- [x] 7.5 Add review/summary step
- [x] 7.6 Add cancel confirmation dialog
- [x] 7.7 Integrate Toast for create success/error feedback
- [x] 7.8 Update CreateJobModal for consistency (if needed)

## 8. Integration & Polish

- [x] 8.1 Update all existing pages to use Toast for feedback
- [x] 8.2 Update existing modals to use unified create flow pattern
- [x] 8.3 Add keyboard navigation support to sidebar
- [x] 8.4 Test sidebar state persistence across page refreshes
- [x] 8.5 Verify all routes work correctly with sidebar context
- [ ] 8.6 Add responsive behavior for smaller screens (optional)

## 9. Testing & Validation

- [x] 9.1 Test platform dashboard displays correctly with mock data
- [x] 9.2 Test cluster dashboard displays correctly with mock data
- [x] 9.3 Test calendar displays jobs correctly
- [x] 9.4 Test sidebar cluster selection and navigation
- [x] 9.5 Test create wizard flows
- [x] 9.6 Test toast notifications
- [x] 9.7 Test error states and loading states
- [x] 9.8 Verify API integration works end-to-end