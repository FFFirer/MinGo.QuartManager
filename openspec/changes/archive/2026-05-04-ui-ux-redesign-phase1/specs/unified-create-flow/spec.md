# Unified Create Flow Specification (Delta)

## Purpose

This delta spec updates the create job flow from a centered modal to a side slide panel with template support.

**Status:** Delta  
**Last Updated:** 2026-05-04

---

## ADDED Requirements

### Requirement: Create job panel slides in from the right

The job creation form SHALL appear as a slide-in panel from the right side of the screen, pushing the main content to the left.

#### Scenario: Slide panel opens
- **WHEN** user clicks "Create Job"
- **THEN** a panel SHALL slide in from the right edge of the screen
- **AND** the main content area SHALL be pushed left (not covered)
- **AND** the panel SHALL be 384px wide on desktop, full screen on mobile
- **AND** the panel SHALL animate with CSS transform (translateX 0% → 100%)

#### Scenario: Slide panel closes
- **WHEN** user clicks the close button
- **OR** presses Escape
- **THEN** the panel SHALL slide out to the right
- **AND** the main content SHALL return to its original position

### Requirement: Create job supports template selection

The create job panel SHALL allow users to start from a template or copy from an existing job.

#### Scenario: Template selector at top
- **WHEN** create job panel opens
- **THEN** a template selector dropdown SHALL appear at the top with:
  - "Blank" option (default)
  - Job type templates from manifest
  - "Copy from existing" option
- **AND** selecting a template SHALL pre-fill job type and parameter fields

## MODIFIED Requirements

### Requirement: Create job follows unified pattern

The job creation wizard SHALL follow the 4-step pattern using schedulerName, rendered as a slide-in panel instead of a centered modal.

**Change**: Rendering from centered Modal to right slide-in panel; added template support

#### Scenario: Job create wizard
- **WHEN** user clicks "Create Job" button on Jobs page
- **THEN** the 4-step wizard SHALL open as a slide-in panel
- **AND** Step 1: Select job type (from manifest or from template) and job key
- **AND** Step 2: Configure parameters
- **AND** Step 3: Schedule configuration
- **AND** Step 4: Summary and Create
- **AND** the wizard SHALL pass schedulerName to API calls
- **AND** the panel header SHALL show step progress
- **AND** the panel footer SHALL show Back/Next/Create buttons
