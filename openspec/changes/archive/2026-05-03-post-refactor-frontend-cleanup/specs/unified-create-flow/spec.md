# Unified Create Flow Specification

## Purpose

This specification defines the requirements for the unified 4-step wizard pattern used for all resource creation in the application.

**Status:** Updated  
**Last Updated:** 2026-05-03

---

## MODIFIED Requirements

### Requirement: Create job follows unified pattern
The job creation wizard SHALL follow the 4-step pattern using schedulerName instead of clusterId.

**Change**: clusterId replaced by schedulerName in all references

#### Scenario: Job create wizard
- **WHEN** user clicks "Create Job" button on Jobs page
- **THEN** the 4-step wizard SHALL open
- **AND** Step 1: Select job type and job key (from manifest)
- **AND** Step 2: Configure parameters
- **AND** Step 3: Schedule configuration
- **AND** Step 4: Summary and Create
- **AND** the wizard SHALL pass schedulerName to API calls

## REMOVED Requirements

### Requirement: Create cluster follows unified pattern
**Reason**: Cluster concept removed in v2.0.0. No cluster creation needed.
**Migration**: No replacement. Agent registration happens automatically via HostedAgentService.
