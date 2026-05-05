## Context

The scheduler jobs page at `/schedulers/:name/jobs` (`JobsPage.tsx`) uses React Query to fetch jobs from the Platform API. Currently:

- **No auto-refresh**: `refetchInterval` is not set; jobs only refresh on mutation (trigger/pause/resume/delete).
- **No manual refresh**: No refresh button exists.
- **Broken pagination**: The Agent's `QuartzService.GetJobsAsync` returns a flat `List<JobSummaryDto>` with Skip/Take, but no total count. The frontend computes `totalItems = jobs.length`, so the pagination bar never shows.

The data flow is:

```
JobsPage → jobApi.getAll → Platform JobsController → JobService
  → AgentProxyService → Agent HTTP API → QuartzService.GetJobsAsync
```

## Goals / Non-Goals

**Goals:**
- Add a refresh button to the Jobs page that re-fetches the current job list
- Fix server-side pagination so users can navigate beyond page 1
- Show a loading/spinning indicator during refresh

**Non-Goals:**
- Auto-refresh (`refetchInterval`) — not requested, adds unnecessary API load
- Invalidation of other pages' caches — only the current scheduler's jobs
- Adding refresh to other pages (agents, schedulers) — scope limited to jobs

## Decisions

### D1: Agent returns `PagedResponse<T>` instead of `List<T>`

The Agent's `QuartzService.GetJobsAsync` currently fetches ALL jobs from Quartz, filters by status/group/keyword, then applies Skip/Take. We modify it to:
1. Apply filters first
2. Count total BEFORE pagination
3. Return `PagedResponse<JobSummaryDto>` with `Items`, `Total`, `Page`, `PageSize`

**Alternatives considered:**
- Custom HTTP header (`X-Total-Count`) on Agent response — rejected because the proxy (`ReadFromApiResponseAsync`) would need special handling; `PagedResponse<T>` works with the existing proxy pipeline
- Client-side pagination — rejected because the Agent could return hundreds of jobs; server-side is more scalable

### D2: Platform proxies `PagedResponse<T>` transparently

The existing `AgentProxyService.GetAsync<PagedResponse<JobSummaryDto>>` works because `ReadFromApiResponseAsync` extracts `.data` from `ApiResponse<T>` and deserializes it as `T`. The Agent already wraps in `ApiResponse<PagedResponse<...>>`, so the unwrapping is automatic.

### D3: Refresh button in PageHeader actions

Place the refresh button alongside the existing "Create Job" button in `PageHeader.actions`. Use a `RefreshCw` icon from `lucide-react` that spins during loading via a CSS animation.

**Alternatives considered:**
- Separate toolbar above the table — adds visual noise
- Auto-refresh interval — not requested by user

### D4: Use `refetch()` from `useQuery`

The existing `useQuery` returns a `refetch` function. We use it directly — no need for `queryClient.invalidateQueries`.

## Risks / Trade-offs

- **API response shape change**: The Agent and Platform now return `PagedResponse<JobSummaryDto>` instead of `List<JobSummaryDto>`. Any API consumers besides the frontend (if any) will need updates. **Mitigation**: Check if any other clients consume `/api/schedulers/{name}/jobs`; this is minor since only the UI consumes this endpoint.
- **Pagination count correctness**: The total count comes from the Agent's in-memory filter, which is accurate at query time. **Not a risk** since the Agent is the source of truth.
- **Refresh during other operations**: If a user triggers a job and immediately clicks refresh, the trigger response and refreshed list could race. **Mitigation**: React Query handles this gracefully; the last resolved promise wins.