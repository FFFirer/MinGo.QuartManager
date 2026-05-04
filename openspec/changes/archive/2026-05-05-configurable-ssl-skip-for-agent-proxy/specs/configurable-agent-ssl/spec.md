## ADDED Requirements

### Requirement: Platform SHALL support configurable SSL verification for Agent API calls

The Platform SHALL allow operators to skip SSL certificate verification when making HTTP calls to Agent APIs, controlled by a configuration setting.

**Configuration key**: `AgentProxy:SkipSslVerify`
**Type**: `boolean`
**Default**: `false`

- When `false` (default): HttpClient SHALL use the system's default TLS certificate chain validation. Self-signed or invalid certificates SHALL cause the request to fail.
- When `true`: HttpClient SHALL skip all TLS certificate verification (self-signed, expired, CN mismatch all allowed).

#### Scenario: Default behavior skips no certificates

- **WHEN** `AgentProxy:SkipSslVerify` is not configured (defaults to `false`)
- **AND** Agent uses a self-signed HTTPS certificate
- **THEN** the HTTP request from Platform to Agent SHALL fail with an authentication/certificate error

#### Scenario: Skip verification allows self-signed certificate

- **WHEN** `AgentProxy:SkipSslVerify` is set to `true`
- **AND** Agent uses a self-signed HTTPS certificate
- **THEN** the HTTP request from Platform to Agent SHALL succeed without certificate validation errors

### Requirement: The configuration SHALL be environment-aware

The Platform SHALL support per-environment configuration of `AgentProxy:SkipSslVerify` through standard ASP.NET Core configuration layering (appsettings.json, appsettings.{Environment}.json, environment variables).

#### Scenario: Development environment has skip enabled by default

- **WHEN** the application runs in `Development` environment
- **AND** `appsettings.Development.json` sets `AgentProxy:SkipSslVerify` to `true`
- **THEN** SSL verification SHALL be skipped for `"AgentApi"` HTTP client requests

#### Scenario: Production environment requires explicit override

- **WHEN** the application runs in `Production` environment
- **AND** no override of `AgentProxy:SkipSslVerify` is configured
- **THEN** SSL verification SHALL use the system default (certificates verified)
