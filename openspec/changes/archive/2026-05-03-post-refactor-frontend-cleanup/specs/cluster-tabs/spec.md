# Cluster Tabs Specification

## REMOVED Requirements

### All requirements
**Reason**: ClusterTabs component has been removed in v2.0.0 architecture refactor. The component was deleted as Cluster concept was removed. Cluster pages (Dashboard, Jobs, Calendar, Agents) no longer exist under /clusters/:id.
**Migration**: Navigation to scheduler resources happens via sidebar (Agents, Schedulers links) rather than inline tabs. Calendar page renders its own header with scheduler name/status.
