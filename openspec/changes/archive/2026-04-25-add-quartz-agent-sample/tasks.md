## 1. Project Setup

- [x] 1.1 Create new ASP.NET Core Web API project in samples/Sample.Agent
- [x] 1.2 Add project reference to MinGo.Qap.Agent
- [x] 1.3 Add project reference to MinGo.Qap.Shared
- [x] 1.4 Add project reference to samples/Sample.Jobs
- [x] 1.5 Update MinGo.Qap.slnx to include new project

## 2. Configuration

- [x] 2.1 Configure Program.cs with Quartz.NET RAMJobStore
- [x] 2.2 Configure Agent services in DI container
- [x] 2.3 Add appsettings.json with Quartz configuration

## 3. Sample Jobs

- [x] 3.1 Create HelloJob (logged, simple job)
- [x] 3.2 Create ScheduledJob (health check job)
- [x] 3.3 Create ManualTriggerJob (API-triggerable job)
- [x] 3.4 Register jobs with Quartz scheduler

## 4. REST API

- [x] 4.1 Create JobsController with GET /api/jobs
- [x] 4.2 Add POST /api/jobs/{key}/trigger endpoint
- [x] 4.3 Add GET /api/jobs/{key} detail endpoint

## 5. Verification

- [x] 5.1 Build project successfully
- [x] 5.2 Run application and verify startup
- [x] 5.3 Test /health endpoint
- [x] 5.4 Test /api/jobs listing (3 jobs: HelloJob, ScheduledJob, ManualTriggerJob)
- [x] 5.5 Test manual job trigger