# Unified Create Flow Schedule

## REMOVED Requirements

### Requirement: Create job passes clusterId to API
**Reason**: ClusterId concept removed. The creation flow uses schedulerName directly.
**Migration**: The wizard continues to pass schedulerName as before.
