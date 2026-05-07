## Why

Job type (JobType) returned by Agent API for scheduler jobs is `"unknown"` when the job was not created through the Platform API. This affects all jobs registered via Quartz DI (`AddQuartz`), discovered from assemblies by `JobDiscoveryService`, or any pre-existing jobs in the scheduler. The root cause is that `jobType` is only read from Quartz `JobDataMap["jobType"]`, which is only populated during the Platform API create-job flow — never for other registration paths.

## What Changes

- Add a `ResolveJobType()` helper method to `QuartzService` with a 3-tier fallback strategy:
  1. `JobDataMap["jobType"]` (existing behavior, for Platform-created jobs)
  2. `JobRegistry` lookup by matching `jobDetail.JobType.FullName` to `JobTypeFullName` (for discovered/registered jobs)
  3. `jobDetail.JobType.Name` (final fallback instead of hardcoded `"unknown"`)
- Update `GetJobAsync()` and `GetJobsAsync()` in `QuartzService` to use `ResolveJobType()` instead of inline `?? "unknown"`

## Capabilities

### New Capabilities

- `job-type-resolution`: Reliable job type resolution for all jobs in a scheduler, regardless of how they were registered, with graceful fallback when type information is unavailable.

### Modified Capabilities

<!-- No existing capabilities are modified — behaviour change is internal to Agent's QuartzService. -->

## Impact

- **File changes**: Only `src/MinGo.Qap.Agent/Services/QuartzService.cs`
- **New method**: `ResolveJobType(IJobDetail)` on `QuartzService`
- **Modified methods**: `GetJobAsync()`, `GetJobsAsync()`
- **No API contract change**: `JobSummaryDto.JobType` and `JobDetailDto.JobType` remain `string`; the set of possible values expands from `{actual, "unknown"}` to `{actual, CLR class name, "unknown"}`
- **No database, config, or frontend changes required**
