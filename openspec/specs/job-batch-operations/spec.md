# Job Batch Operations Specification

## Purpose

This specification defines the requirements for batch selection and batch operations on the Jobs page.

**Status:** New  
**Last Updated:** 2026-05-04

---

## ADDED Requirements

### Requirement: Jobs table shows checkboxes for multi-select

The Jobs table SHALL display a checkbox column for selecting individual jobs, and a header checkbox for select-all.

#### Scenario: Select individual job
- **WHEN** user clicks a job row's checkbox
- **THEN** that job SHALL be selected (checkbox checked)
- **AND** the row SHALL be visually highlighted

#### Scenario: Select all jobs on page
- **WHEN** user clicks the header checkbox
- **THEN** all jobs on the current page SHALL be selected
- **AND** the batch action toolbar SHALL appear

#### Scenario: Deselect all
- **WHEN** user clicks the header checkbox while all are selected
- **THEN** all jobs SHALL be deselected
- **AND** the batch action toolbar SHALL hide

### Requirement: Batch action toolbar appears when items selected

A batch action toolbar SHALL appear above the table when one or more jobs are selected.

#### Scenario: Batch toolbar display
- **WHEN** at least one job is selected
- **THEN** a toolbar SHALL appear showing: "X selected" count and action buttons
- **AND** toolbar SHALL contain: Trigger, Pause, Resume, Delete buttons
- **AND** the Delete button SHALL have a distinct danger style
- **AND** toolbar SHALL be clearly separated from the table

#### Scenario: Batch Trigger
- **WHEN** user clicks "Trigger" in batch toolbar
- **THEN** all selected jobs SHALL be triggered
- **AND** success toast SHALL show "Triggered X jobs successfully"
- **AND** if any jobs fail, error toast SHALL show "X jobs failed: ..."

#### Scenario: Batch Pause
- **WHEN** user clicks "Pause" in batch toolbar
- **THEN** all selected jobs SHALL be paused

#### Scenario: Batch Resume
- **WHEN** user clicks "Resume" in batch toolbar
- **THEN** all selected jobs SHALL be resumed

#### Scenario: Batch Delete with confirmation
- **WHEN** user clicks "Delete" in batch toolbar
- **THEN** a confirmation dialog SHALL appear
- **AND** SHALL display "Are you sure you want to delete X jobs?"
- **AND** **WHEN** user confirms, all selected jobs SHALL be deleted
- **AND** **WHEN** user cancels, no action SHALL be taken

### Requirement: Pagination component supports page size selection

The pagination component SHALL support configurable page size and direct page number input.

#### Scenario: Page size selector
- **WHEN** user clicks the page size selector
- **THEN** a dropdown SHALL show options: 10, 20, 50, 100
- **AND** selecting a new page size SHALL reload the jobs list

#### Scenario: Page number display
- **WHEN** pagination renders
- **THEN** it SHALL show: "Showing X-Y of Z items"
- **AND** page buttons SHALL be numbered (with ellipsis for large page counts)
