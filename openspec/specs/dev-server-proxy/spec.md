## Purpose

Configuration for frontend development server proxy to enable seamless API requests during development without CORS issues.

## ADDED Requirements

### Requirement: Vite dev server proxy configuration

The development server SHALL proxy `/api` path requests to the backend development server at `http://localhost:5256` during development mode.

#### Scenario: Proxy /api requests to backend
- **WHEN** frontend makes a request to `/api/users`
- **THEN** Vite development server proxies the request to `http://localhost:5256/api/users`

#### Scenario: Proxy preserves request headers
- **WHEN** frontend makes an authenticated request to `/api/data`
- **THEN** Vite development server SHALL forward all request headers (including Authorization) to the backend

#### Scenario: Proxy handles CORS implicitly
- **WHEN** frontend makes a cross-origin request to `/api/*`
- **THEN** The proxy bypasses browser CORS restrictions since requests are to the same origin

### Requirement: Relative API path support

The frontend API client SHALL use relative paths (`/api`) instead of absolute URLs for all API requests during development.

#### Scenario: API client uses relative path
- **WHEN** frontend code calls `api.get('/users')`
- **THEN** the actual request is sent to `/api/users` which gets proxied

#### Scenario: Environment variable fallback for API URL
- **WHEN** `VITE_API_URL` environment variable is set
- **THEN** the API client SHALL use that value as base URL
- **WHEN** `VITE_API_URL` environment variable is not set
- **THEN** the API client SHALL use relative path `/api`

### Requirement: Development-only proxy behavior

The proxy configuration SHALL only be active during development mode and SHALL NOT affect production builds.

#### Scenario: Production build excludes proxy
- **WHEN** running `npm run build` for production
- **THEN** the proxy configuration SHALL NOT be included in the production bundle

#### Scenario: Preview mode uses built assets
- **WHEN** running `npm run preview`
- **THEN** requests go directly to the configured API URL without proxy

### Requirement: Hot module replacement compatibility

The proxy configuration SHALL support hot module replacement without requiring server restart.

#### Scenario: HMR preserves proxy during file changes
- **WHEN** a frontend file is changed and HMR triggers
- **THEN** the proxy configuration SHALL remain active without interruption
