## Context

Agent's `QuartzService.GetJobAsync()` and `GetJobsAsync()` read `JobType` exclusively from `JobDataMap["jobType"]`, defaulting to `"unknown"` when absent. This key is only written by `JobConverter.ConvertToDetail()` during the Platform API create-job flow.

However, jobs can enter the Quartz scheduler through multiple paths:
- Quartz DI registration (`AddQuartz` → `ScheduleJob<T>` / `AddJob<T>`)
- Assembly scanning by `JobDiscoveryService`
- Direct Quartz API calls by the host application
- Pre-existing jobs from a previous session (persistent job store)

All these paths bypass the `JobDataMap["jobType"]` assignment, producing `"unknown"` in responses.

The `JobRegistry` already stores the mapping from CLR type (`JobTypeFullName`) to user-visible key (`Key`) for discovered jobs. The `IJobDetail.JobType` property exposes the CLR type of the job implementation.

## Goals / Non-Goals

**Goals:**
- Return a meaningful `JobType` value for ALL jobs in a scheduler, regardless of registration path
- Preserve `"unknown"` only when truly no type information is available
- Minimize code changes (single file, no new dependencies)
- No API contract changes or frontend updates

**Non-Goals:**
- Retroactively writing `jobType` into `JobDataMap` for existing jobs (not feasible without re-registration)
- Changing the `JobSummaryDto` / `JobDetailDto` type definitions
- Introducing a new enum or type system for job types

## Decisions

### Decision: 3-tier fallback resolution instead of populating JobDataMap eagerly

**Option A (chosen): Resolve at query time.**
Add `ResolveJobType(IJobDetail)` that tries:
1. `JobDataMap["jobType"]` — exists for Platform-created jobs
2. `JobRegistry` lookup by `JobTypeFullName` → returns manifest `Key` — works for discovered jobs
3. `jobDetail.JobType.Name` — CLR class name as human-readable fallback

**Option B (rejected): Write `jobType` to JobDataMap during discovery/registration.**
Would require modifying the registration pipeline, which is more invasive and assumes all jobs are registered through MinGo's discovery mechanism. Jobs registered directly via `AddQuartz` in `Program.cs` would still be missed.

**Option C (rejected): Always return CLR type name.**
Loses the user-friendly manifest key that maps to parameter metadata in the UI.

### Decision: Keep "unknown" as ultimate fallback

Even with CLR `Type.Name`, there are edge cases (e.g., dynamic proxy types, AOP-wrapped jobs) where `JobType` may be null or have a non-meaningful name. `"unknown"` communicates this failure state clearly.

## Risks / Trade-offs

- **[Low] JobRegistry lookup cost**: Iterating `GetAll()` on every job in the list adds O(n) per job. Mitigation: JobRegistry contains at most dozens of entries (1-2 orders of magnitude fewer than jobs). Could optimize with a dictionary if profiling shows issues.
- **[Low] Race condition**: Jobs added between registry refresh and query. Mitigation: CLR type name fallback still produces a meaningful value. This is a transient condition.
- **[None] No migration needed**: Change is purely in query-time resolution. Existing jobs, data, and API contracts are unaffected.
