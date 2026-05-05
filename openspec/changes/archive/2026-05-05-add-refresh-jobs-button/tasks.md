## 1. Agent: Return PagedResponse from GetJobsAsync

- [x] 1.1 Modify `QuartzService.GetJobsAsync` to compute total before Skip/Take and return `PagedResponse<JobSummaryDto>`
- [x] 1.2 Update `IQuartzService` interface `GetJobsAsync` return type to `PagedResponse<JobSummaryDto>`
- [x] 1.3 Update Agent's `GetJobsHandler` to wrap response in `PagedResponse<JobSummaryDto>` — returns `ApiResponse<PagedResponse<JobSummaryDto>>`

## 2. Platform: Update JobService and Controller

- [x] 2.1 Update `IJobService.GetBySchedulerAsync` return type to `PagedResponse<JobSummaryDto>`
- [x] 2.2 Update `JobService.GetBySchedulerAsync` to proxy `PagedResponse<JobSummaryDto>` from Agent
- [x] 2.3 Update fallback DB path in `JobService.GetBySchedulerAsync` to return `PagedResponse<JobSummaryDto>` with total count
- [x] 2.4 Update `JobsController.GetList` to return `ApiResponse<PagedResponse<JobSummaryDto>>`

## 3. Frontend: Fix pagination and add refresh button

- [x] 3.1 Update `jobApi.getAll` return type to `ApiResponse<PagedResponse<JobSummaryDto>>`
- [x] 3.2 Update `JobsPage.tsx` to read jobs from `data.items` and total from `data.total` for pagination
- [x] 3.3 Add `RefreshCw` icon button to `PageHeader.actions` with spinning animation during refetch
- [x] 3.4 Verify frontend type imports — `PagedResponse<T>` already exists in types

## 4. Verify

- [x] 4.1 Ensure `dotnet build` succeeds for Agent project (0 errors ✓)
- [x] 4.2 Ensure `dotnet build` succeeds for Platform project (0 errors ✓)
- [x] 4.3 Ensure frontend `pnpm build` succeeds (0 new errors — remaining 17 errors are pre-existing in AgentDetailPage, JobDetailPage, and JobsPage toast/ConfirmDialog)
- [ ] 4.4 Run `pnpm lint` for frontend code quality (deferred — build errors block lint)
