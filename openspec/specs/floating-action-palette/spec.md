# Floating Action Palette Specification

## Purpose

This specification defines the requirements for the floating action palette (FAB) providing quick access to common operations.

**Status:** New  
**Last Updated:** 2026-05-04

---

## ADDED Requirements

### Requirement: Floating action button displays in bottom-right corner

A floating action button SHALL be displayed in the bottom-right corner of the application, providing quick access to common operations.

#### Scenario: FAB visible on all pages
- **WHEN** application renders any page
- **THEN** a floating action button SHALL be visible in the bottom-right corner
- **AND** SHALL display a lightning bolt icon (⚡) by default
- **AND** SHALL have a distinct blue background with shadow

### Requirement: FAB expands to show action menu

Clicking the FAB SHALL expand it to show a contextual action menu.

#### Scenario: FAB menu expands
- **WHEN** user clicks the FAB
- **THEN** the FAB SHALL expand upward showing action items
- **AND** SHALL display: "Create Job", "View Recent" submenu
- **AND** menu items SHALL have icons and labels
- **AND** clicking outside or pressing Escape SHALL close the menu

#### Scenario: Create Job from FAB
- **WHEN** user is on a Scheduler detail or Jobs page
- **AND** clicks "Create Job" from the FAB
- **THEN** the create job slide panel SHALL open for the current scheduler
- **AND** **WHEN** user is on Dashboard or Agents page
- **THEN** clicking "Create Job" SHALL first ask to select a scheduler

#### Scenario: Recent actions in FAB
- **WHEN** FAB is expanded
- **THEN** it SHALL show the 3 most recent operations (e.g., "Triggered daily-sync", "Paused weekly-report")
- **AND** clicking a recent action SHALL navigate to the relevant job or resource
