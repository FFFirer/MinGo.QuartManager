# MinGo.Qap.Shared

Shared DTOs and data contracts for MinGo Qap Platform and Agent.

## Contents

- **JobKeyDto** — Strongly-typed job identifier (Name + Group)
- **ExecutionLogDto** — Job execution log entry reported by Agent
- **ExecutionLogEntryDto** — Persisted execution log query result
- **BatchJobRequest / BatchJobResultDto** — Batch job operation contracts
- **PagedResponse<T>** — Generic paginated response wrapper
- **Agent/Scheduler DTOs** — Agent registration, scheduler reporting

## Usage

```bash
dotnet add package MinGo.Qap.Shared
```

This package is consumed by:
- `MinGo.Qap.Platform` — Platform server
- `MinGo.Qap.Agent` — Agent library
