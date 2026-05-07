## ADDED Requirements

### Requirement: All entity DateTime fields use DateTimeOffset
Every entity in the Platform data layer SHALL use `DateTimeOffset` (or `DateTimeOffset?`) for date/time fields. This ensures consistent UTC handling via the global `ValueConverter` and prevents `InvalidCastException` in `UtcAuditInterceptor`.

| Entity | Field | Current Type | Target Type |
|--------|-------|-------------|-------------|
| JobDefinition | CreatedAt | DateTime | DateTimeOffset |
| JobDefinition | UpdatedAt | DateTime? | DateTimeOffset? |
| Cluster | CreatedAt | DateTime | DateTimeOffset |
| Cluster | UpdatedAt | DateTime? | DateTimeOffset? |
| Cluster | DeletedAt | DateTime? | DateTimeOffset? |
| Cluster | LastHeartbeat | DateTime? | DateTimeOffset? |
| AgentInstance | CreatedAt | DateTime | DateTimeOffset |
| AgentInstance | UpdatedAt | DateTime? | DateTimeOffset? |
| AgentInstance | DeletedAt | DateTime? | DateTimeOffset? |
| AgentInstance | LastHeartbeat | DateTime? | DateTimeOffset? |
| AgentInstance | StartedAt | DateTime? | DateTimeOffset? |

#### Scenario: Entity Creation with DateTimeOffset
- **WHEN** creating a new entity (Cluster, AgentInstance, or JobDefinition) having `CreatedAt` and `UpdatedAt` properties
- **THEN** the service layer SHALL assign `DateTimeOffset.UtcNow` for initial creation time
- **AND** the interceptor SHALL be able to assign `DateTimeOffset.UtcNow` without `InvalidCastException`

#### Scenario: Values flow through ValueConverter
- **WHEN** a DateTimeOffset value is written to the database
- **THEN** the global `ValueConverter<DateTimeOffset, DateTimeOffset>` SHALL convert it to UTC before persisting
- **AND** when reading back, SHALL return the value normalized to UTC

### Requirement: DTO DateTime properties use DateTimeOffset
DTOs that map to or from entity DateTime fields SHALL use `DateTimeOffset` to maintain type consistency through the full service layer.

| DTO | Fields | Current Type | Target Type |
|-----|--------|-------------|-------------|
| AgentInstanceDto | LastHeartbeat, StartedAt, CreatedAt, UpdatedAt | DateTime | DateTimeOffset |
| AgentSummaryDto | LastHeartbeat, StartedAt, CreatedAt | DateTime | DateTimeOffset |
| JobDefinitionDto | CreatedAt, UpdatedAt | DateTime | DateTimeOffset |
| ClusterDashboardDto | CreatedAt, LastUpdated | DateTime | DateTimeOffset |
| DashboardDto | LastUpdated | DateTime | DateTimeOffset |
| UpcomingJobDto | NextFireTime | DateTime | DateTimeOffset |
| ApiResponse<T> | Timestamp | DateTime | DateTimeOffset |

#### Scenario: DTO mapping consistency
- **WHEN** mapping from entity to DTO (e.g., `JobService.MapToDto`)
- **THEN** the DateTimeOffset entity property SHALL be assignable directly to the DateTimeOffset DTO property without type conversion

#### Scenario: ApiResponse timestamp
- **WHEN** creating an `ApiResponse<T>` with default timestamp
- **THEN** `Timestamp` SHALL be `DateTimeOffset.UtcNow`
- **AND** JSON serialization SHALL produce ISO 8601 format with offset (e.g., `"2026-05-07T22:29:06+00:00"`)

### Requirement: UtcAuditInterceptor handles both DateTimeOffset and null checks correctly
The interceptor's CreatedAt auto-fill logic SHALL correctly detect unset DateTimeOffset values and apply `DateTimeOffset.UtcNow`.

#### Scenario: CreatedAt auto-fill for DateTimeOffset
- **WHEN** a new entity has `DateTimeOffset CreatedAt` with `default` value (`0001-01-01T00:00:00+00:00`)
- **THEN** the interceptor SHALL detect this via `currentValue is DateTimeOffset dto && dto == default`
- **AND** SHALL set it to `DateTimeOffset.UtcNow`

#### Scenario: UpdatedAt auto-fill runs for all modified entities
- **WHEN** any entity (Added or Modified state) has an `UpdatedAt` property
- **THEN** the interceptor SHALL set it to the appropriate UTC value matching the property's CLR type
- **AND** SHALL NOT throw `InvalidCastException` regardless of whether the property is `DateTimeOffset`, `DateTimeOffset?`, `DateTime`, or `DateTime?`

### Requirement: Service layer uses DateTimeOffset.UtcNow
All service code assigning UTC timestamps to entity properties SHALL use `DateTimeOffset.UtcNow`.

#### Scenario: JobService assignments
- **WHEN** `JobService.CreateAsync` creates a `JobDefinition`
- **THEN** `CreatedAt` and `UpdatedAt` SHALL use `DateTimeOffset.UtcNow`

#### Scenario: DashboardController assignments
- **WHEN** `DashboardController.GetPlatformDashboard` sets `LastUpdated`
- **THEN** it SHALL use `DateTimeOffset.UtcNow`

### Requirement: Database migration preserves timestamptz columns
The EF Core migration SHALL update the CLR type mapping while preserving the existing `timestamptz` PostgreSQL column type.

#### Scenario: Migration is no-op on column type
- **WHEN** generating the migration for `DateTime` → `DateTimeOffset` changes
- **THEN** the PostgreSQL column type SHALL remain `timestamp with time zone` (`timestamptz`)
- **AND** no `ALTER COLUMN TYPE` statements SHOULD be generated (column type is already compatible)
