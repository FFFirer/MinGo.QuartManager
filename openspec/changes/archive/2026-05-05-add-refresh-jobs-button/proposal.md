## Why

The scheduler Jobs page lacks a manual refresh button — users must navigate away and back to see updated job states. Additionally, the server-side pagination for jobs is broken: the frontend computes `totalItems` from the returned page-sized array, so the pagination bar never displays and users can only see the first page.

## What Changes

- **Frontend**: Add a "Refresh Jobs" button with `RefreshCw` icon in the PageHeader actions on `JobsPage.tsx`. Clicking it calls `refetch()` on the existing React Query, with a spinning animation during loading.
- **Backend (Agent)**: `QuartzService.GetJobsAsync` returns `PagedResponse<JobSummaryDto>` instead of `List<JobSummaryDto>`, providing the total job count for proper pagination.
- **Backend (Agent)**: The Agent's `GetJobsHandler` endpoint returns `ApiResponse<PagedResponse<JobSummaryDto>>`.
- **Backend (Platform)**: `IJobService.GetBySchedulerAsync` returns `PagedResponse<JobSummaryDto>`. The proxy fetches from Agent as `PagedResponse`, with fallback to DB also returning proper pagination.
- **Backend (Platform)**: `JobsController.GetList` returns `ApiResponse<PagedResponse<JobSummaryDto>>`.
- **Frontend**: `jobApi.getAll` return type changes to `ApiResponse<PagedResponse<JobSummaryDto>>`. The JobsPage reads jobs from `.data.items` and total count from `.data.total`.
- No breaking changes to the public API contract — the response shape changes from `{ data: JobSummaryDto[] }` to `{ data: { items: JobSummaryDto[], total, page, pageSize, totalPages } }`.

## Capabilities

### New Capabilities

- `refresh-jobs`: Manual refresh of scheduler jobs list via button click

### Modified Capabilities

- `job-list`: Job list API now returns `PagedResponse<JobSummaryDto>` with pagination metadata instead of a flat array.

## Impact

- **Agent**: `QuartzService.GetJobsAsync` — return type change
- **Agent**: `GetJobsHandler` — response shape change
- **Platform**: `IJobService`, `JobService.GetBySchedulerAsync` — return type change
- **Platform**: `JobsController.GetList` — response type change
- **Frontend**: `jobApi.getAll` — return type change
- **Frontend**: `JobsPage.tsx` — add refresh button, fix pagination logic
- **Frontend types**: `index.ts` — no change needed, `PagedResponse<T>` already exists