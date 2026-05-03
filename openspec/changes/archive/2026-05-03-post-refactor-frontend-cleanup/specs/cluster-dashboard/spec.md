# Cluster Dashboard Specification

## REMOVED Requirements

### All requirements
**Reason**: Cluster concept removed in v2.0.0 architecture refactor. Cluster dashboard page has been deleted. Scheduler detail page at /schedulers/{name} replaces the concept with scheduler-specific metadata (status, job counts, associated agents).
**Migration**: Use SchedulerDetailPage at /schedulers/{name} for scheduler-specific information. Scheduler detail provides: status, instance ID, version, job counts breakdown, associated agents, and job store metadata.
