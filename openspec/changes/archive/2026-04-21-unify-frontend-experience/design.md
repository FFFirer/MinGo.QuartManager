## Context

The MinGo.Qap.UI frontend is a React application using TypeScript, React Router, and Tailwind CSS. Currently, each page (ClustersPage, ClusterDetailPage, JobsPage, JobDetailPage, AgentInstancesPage) reimplements similar UI patterns:

1. **Status Display**: Each page has its own `getStatusColor` function mapping status strings to Tailwind color classes, with slight variations (e.g., Jobs page uses red for offline while others use slate).
2. **Page Headers**: Each page manually constructs its header area with varying combinations of title, back navigation, breadcrumbs, status indicators, and action buttons.
3. **Data Tables**: JobsPage and AgentInstancesPage both implement tables but with different class names and structures.
4. **Confirmation Dialogs**: Delete actions use inline `window.confirm()` calls with different wording.
5. **Loading/Error States**: Each page handles loading and error states with similar but duplicated patterns.

This duplication leads to inconsistencies in appearance and behavior, increased maintenance effort, and higher risk of bugs when changes need to be made across multiple places.

## Goals / Non-Goals

**Goals:**
- Create a consistent user interface across all pages by extracting common UI patterns into reusable components
- Ensure navigation logic is uniform and correct (back buttons and breadcrumbs point to appropriate parent pages)
- Reduce code duplication and improve maintainability
- Establish a foundation for future UI consistency improvements
- Maintain all existing functionality and visual design where possible

**Non-Goals:**
- Changing the underlying application architecture or state management
- Modifying API contracts or backend services
- Redesigning the visual appearance beyond achieving consistency
- Introducing new UI libraries or frameworks beyond what's already used (React, Tailwind, Lucide icons)
- Addressing performance optimization beyond what's needed for consistency

## Decisions

### Component Architecture Decision
**Choice**: Create a set of focused, single-responsibility UI components rather than a monolithic UI library
**Rationale**: 
- Allows incremental adoption (pages can adopt components one at a time)
- Keeps components simple and easy to understand
- Reduces coupling between UI elements
- Alternatives considered: 
  - Monolithic UI library (rejected: overkill for current scope, higher adoption barrier)
  - CSS-only utility classes (rejected: doesn't solve behavioral inconsistencies like navigation logic)
  - Higher-order components (rejected: adds complexity for simple use cases)

### Status Badge Implementation Decision
**Choice**: Create a `StatusBadge` component that accepts a status string and returns appropriate styling based on a centralized mapping
**Rationale**: 
- Eliminates duplicated `getStatusColor` functions
- Ensures consistent color mapping across all status types (cluster, job, agent)
- Allows for easy updates to the color scheme in one place
- Alternatives considered:
  - CSS classes based on status strings directly (rejected: less flexible, harder to maintain mapping)
  - Passing color props directly (rejected: pushes responsibility to callers, defeats purpose)

### Page Header Implementation Decision
**Choice**: Create a `PageHeader` component that accepts title, breadcrumbs, back path, status, and actions as props
**Rationale**: 
- Standardizes the header layout while allowing flexibility
- Handles the common pattern of title + actions + optional navigation aids
- Alternatives considered:
  - Using React Router's `useNavigation` or similar (rejected: still requires manual construction in each page)
  - CSS grid/template approach (rejected: doesn't encapsulate the logic, still duplicated)

### Data Table Implementation Decision
**Choice**: Create a `DataTable` component that takes column definitions and data rows
**Rationale**: 
- Eliminates duplicated table markup and class names
- Provides consistent styling, sorting, and interaction patterns
- Alternatives considered:
  - Using a third-party table library (rejected: overkill for simple tables, bundle size concerns)
  - Continuing with manual tables but extracting common CSS classes (rejected: doesn't eliminate duplicated JSX structure)

### Navigation Fix Decision
**Choice**: Implement breadcrumb-based navigation in `PageHeader` and ensure all links point to correct parent routes
**Rationale**: 
- Addresses the core user experience issue of incorrect back navigation
- Provides clear visual hierarchy of current location
- Alternatives considered:
  - Keeping current navigation but fixing links only (rejected: doesn't provide the improved UX of breadcrumbs)
  - Using a navigation library (rejected: adds dependency for simple need)

## Risks / Trade-offs

[Component adoption risk] → Mitigation: Design components to be easily adoptable with clear documentation and examples. Start with least complex pages (ClustersPage) first.

[Style consistency risk] → Mitigation: Use existing Tailwind classes from index.css where possible. Create visual regression tests if needed in future.

[Performance risk] → Mitigation: Components are simple and lightweight. Use React.memo where appropriate if performance profiling shows need.

[Incomplete coverage risk] → Mitigation: Clearly document which pages have been migrated. Allow for gradual migration over multiple PRs.

## Migration Plan

1. **Create component files** in `src/MinGo.Qap.UI/src/components/`:
   - `StatusBadge.tsx`
   - `PageHeader.tsx`
   - `DataTable.tsx`
   - `ConfirmDialog.tsx`
   - `LayoutWrapper.tsx` (optional, for page structure consistency)

2. **Migrate pages in order of complexity**:
   - Start with `ClustersPage` (simplest, no complex navigation)
   - Then `ClusterDetailPage` (simple back navigation)
   - Then `JobsPage` (fix back navigation, replace table)
   - Then `JobDetailPage` (most complex, multiple actions and status)
   - Finally `AgentInstancesPage` (already has breadcrumbs, needs fixes)

3. **For each page**:
   - Replace status display with `<StatusBadge />`
   - Replace header with `<PageHeader />` 
   - Replace table with `<DataTable />` where applicable
   - Replace `window.confirm` with `<ConfirmDialog />`
   - Ensure all navigation links use correct paths
   - Standardize loading/error state handling

4. **Update imports** and remove duplicated utility functions (getStatusColor, etc.)

5. **Verify functionality** after each page migration

## Open Questions

- Should the `LayoutWrapper` component be used to enforce consistent page padding/margins, or should this be handled via CSS classes on individual pages?
- Should we create a unified loading spinner component, or continue using the existing Tailwind-based approach?
- How should we handle the agent deletion confirmation in AgentInstancesPage which currently uses inline confirmation without a modal?
