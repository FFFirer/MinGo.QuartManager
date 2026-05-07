## 1. Cleanup

- [x] 1.1 Delete `CreateJobModal.tsx` (dead code)
- [x] 1.2 Delete `CreateJobPanel.tsx` (replaced by new page)

## 2. Route & Navigation Setup

- [x] 2.1 Add route `/schedulers/:schedulerName/jobs/create` in `App.tsx`
- [x] 2.2 Update `JobsPage.tsx`: "Create Job" button navigates to create page
- [x] 2.3 Add "Copy" action button in JobsPage job rows

## 3. API & Types

- [x] 3.1 Add `get()` method to `jobApi` in `api/index.ts` (for copy feature)
- [x] 3.2 Update `types/index.ts` if needed (ensure parseJobKey helper exists)

## 4. CreateJobPage - Core Form

- [x] 4.1 Create `CreateJobPage.tsx` with page layout, back navigation
- [x] 4.2 Page header with breadcrumbs and Scheduler context
- [x] 4.3 Load manifest and existing jobs on mount
- [x] 4.4 Load existing job for copy if `?copyFrom=` query param present
- [x] 4.5 Group field: dropdown of existing groups + custom input
- [x] 4.6 Job Name field with validation (required, no dots)
- [x] 4.7 Job Type selection list (vertical, fixed-height scrollable)
  - [x] 4.7.1 Change grid layout to single-column vertical list with `space-y-1`
  - [x] 4.7.2 Wrap list in fixed-height scrollable container (`max-h-[340px] overflow-y-auto`)
  - [x] 4.7.3 Line 1: short name (last segment of `fullName`), bold, `truncate` + `title`
  - [x] 4.7.4 Line 2: full name using `JobTypeDisplay` component with `size="sm"`, `truncate`
  - [x] 4.7.5 Line 3: `job.description` (hidden when empty), `truncate` + `title`
  - [x] 4.7.6 Line 4: parameter count info, formatted as `X parameters (Y required)`
  - [x] 4.7.7 Add border separator between items (`border-b border-slate-700/50`)
  - [x] 4.7.8 Preserve existing selection visual (blue border + check icon) on full-row click

## 5. CreateJobPage - Parameters

- [x] 5.1 Dynamic parameter form rendering (string→input, int→number, bool→select)
- [x] 5.2 JSON textarea for complex parameter types with validation
- [x] 5.3 Required parameter marking (red *) and validation on submit

## 6. CreateJobPage - Schedule

- [x] 6.1 Schedule type tabs: Cron / Interval / Once
- [x] 6.2 Cron expression input with preset buttons
- [x] 6.3 Cron format validation
- [x] 6.4 Interval input (hours/minutes/seconds)
- [x] 6.5 Once datetime-local input

## 7. CreateJobPage - Options & Submit

- [x] 7.1 Disallow Concurrent Execution checkbox
- [x] 7.2 Misfire Policy dropdown
- [x] 7.3 Submit button with validation (required fields, params, schedule)
- [x] 7.4 Compose `jobKey` as `"{group}.{name}"` and call `jobApi.create()`
- [x] 7.5 Success toast + navigate back to Jobs list
- [x] 7.6 Error handling with toast

## 8. Verification

- [x] 8.1 LSP diagnostics clean on all changed files
- [x] 8.2 Build passes (`dotnet build` for backend, `pnpm build` for frontend)
