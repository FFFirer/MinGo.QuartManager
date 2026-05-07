# Cluster Dashboard Schedule

## REMOVED Requirements

### All requirements
**Reason**: Cluster concept removed. Dashboard controller endpoints `GET /api/clusters/{clusterId}/dashboard` and related code deleted.
**Migration**: Use platform dashboard at `GET /api/dashboard` for overview. Scheduler detail at `GET /api/schedulers/{name}` for scheduler-specific information.
