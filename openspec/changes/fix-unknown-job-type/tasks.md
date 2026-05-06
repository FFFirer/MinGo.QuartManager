## 1. Core Implementation

- [ ] 1.1 Add `ResolveJobType(IJobDetail)` method to `QuartzService` that implements 3-tier fallback (JobDataMap → JobRegistry by Type → CLR type name)
- [ ] 1.2 Update `GetJobAsync()` to use `ResolveJobType()` instead of inline `?? "unknown"`
- [ ] 1.3 Update `GetJobsAsync()` to use `ResolveJobType()` instead of inline `?? "unknown"`

## 2. Verification

- [ ] 2.1 Verify LSP diagnostics are clean on changed files
- [ ] 2.2 Build solution to confirm compilation
