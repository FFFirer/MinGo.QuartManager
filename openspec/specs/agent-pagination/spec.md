# Agent Pagination Specification

## Purpose

Defines the pagination requirements for Agent list API and frontend display, ensuring consistent paginated access to agent data across the application.

**Status:** New  
**Last Updated:** 2026-05-04

---

## Requirements

### Requirement: Agent list supports paginated queries
The backend `GET /api/agents` endpoint SHALL support pagination via `page` and `pageSize` query parameters.

#### Scenario: Default pagination
- **WHEN** client sends `GET /api/agents` without pagination parameters
- **THEN** server SHALL return the first page with default page size (20)
- **AND** response SHALL include total count for pagination metadata

#### Scenario: Custom page and page size
- **WHEN** client sends `GET /api/agents?page=2&pageSize=10`
- **THEN** server SHALL return agents 11-20 (second page of 10)
- **AND** response SHALL include the total count across all pages

#### Scenario: Page beyond available data
- **WHEN** client sends `GET /api/agents?page=999`
- **THEN** server SHALL return an empty items array
- **AND** response SHALL include the correct total count

### Requirement: Agent pagination response includes metadata
The paginated response SHALL follow the existing `PagedResponse<T>` format used by Jobs API.

#### Scenario: Response format
- **WHEN** client receives a paginated agent list response
- **THEN** the response body SHALL contain: `items` (array of AgentSummaryDto), `total` (int), `page` (int), `pageSize` (int), `totalPages` (int)

### Requirement: Frontend agent API supports pagination parameters
The `agentApi.getAll()` function SHALL accept `page` and `pageSize` parameters and pass them to the backend API.

#### Scenario: With pagination params
- **WHEN** `agentApi.getAll(2, 10)` is called
- **THEN** it SHALL send `GET /api/agents?page=2&pageSize=10`

#### Scenario: Default values
- **WHEN** `agentApi.getAll()` is called without arguments
- **THEN** it SHALL default to page=1, pageSize=20

### Requirement: AgentsPage displays paginated agent data
AgentsPage SHALL use the paginated API and show pagination controls.

#### Scenario: AgentsPage shows pagination
- **WHEN** AgentsPage loads
- **THEN** it SHALL display a paginated list of agents using DataTable
- **AND** SHALL show pagination controls (Previous/Next, page numbers, page size selector)

#### Scenario: Page change
- **WHEN** user clicks "Next" or a page number
- **THEN** AgentsPage SHALL fetch the corresponding page from the API
- **AND** SHALL update the displayed data
