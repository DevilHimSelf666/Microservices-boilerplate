# Microservices Boilerplate

This repository is an Aspire-first microservices scaffold targeting .NET 10. It includes an API gateway, Blazor Server UI, Elsa workflow stub, and 11 module APIs orchestrated through the Aspire AppHost.

## Aspire Template Choice
- **Template:** `aspire-starter`
- **Why:** The official Microsoft documentation recommends `dotnet new aspire-starter` for distributed applications that combine an AppHost, ServiceDefaults, and multiple services. It aligns with the need for a gateway, UI, and background services while keeping orchestration first-class in Aspire.

## Projects
- `src/AppHost` — Aspire orchestration of all runnable services and infra containers.
- `src/ServiceDefaults` — OpenTelemetry defaults, logging, health checks, and messaging placeholders.
- `src/Gateway` — YARP reverse proxy with correlation ID propagation and SSO adapter TODO.
- `src/Ui.BlazorServer` — Blazor Server UI.
- `src/Elsa.Server` — Elsa workflow server placeholder.
- Module APIs (ARC, DAR, DBA, BUD, CTR, COS, RAP, RDE, REP, RRE, RST) under `services/<module>.api`.
- Tests for each module under `tests/<module>.api.tests`.

### Elsa two-level approval sample
- The Elsa server exposes a budget request workflow that models a real-world two-step approval for the **BUD** service.
- Start a workflow instance:
  - `POST /workflow/bud/expense-approvals` with body `{ "requestedBy": "jane.doe", "costCenter": "CC-1001", "amount": 12000, "description": "Modernization sprint" }`.
- Complete approvals in order:
  - Department manager: `POST /workflow/bud/expense-approvals/{id}/decisions` with `{ "role": "department-manager", "approver": "team.lead", "approve": true, "comments": "Fits roadmap" }`.
  - Finance controller: `POST /workflow/bud/expense-approvals/{id}/decisions` with `{ "role": "finance-controller", "approver": "finance.ops", "approve": true, "comments": "Budget available" }`.
- Retrieve status (current step, approvals, or rejection reason): `GET /workflow/bud/expense-approvals/{id}`.

## Running Locally (Aspire)
1. Ensure .NET 10 SDK is installed.
2. From the repo root run:
   ```bash
   dotnet restore
   dotnet run --project src/AppHost/AppHost.csproj
   ```
3. The AppHost will orchestrate Gateway, UI, Elsa, and all module services.

## Running with Docker Compose
> Docker MUST NOT be executed in Codex Web, but these instructions are provided for real environments.
1. Copy `infra/.env.template` to `.env` and fill secrets (SQL password, RabbitMQ credentials).
2. Build images:
   ```bash
   docker compose -f infra/docker-compose.yml build
   ```
3. Start the stack:
   ```bash
   docker compose -f infra/docker-compose.yml up
   ```
4. Access the Gateway at `http://localhost:8080`.

## SSO Integration
- Gateway registers `ISsoConfigurator` with `NoopSsoConfigurator` as a placeholder.
- Wire the organization package by replacing the registration with the `AddSso(...)` call once the NuGet package is available.

## Offline Production
- `nuget.config` allows overriding the feed via `NUGET_INTERNAL_FEED`.
- Docker images use explicit tags to permit offline mirroring.
- Service configuration relies on environment variables so no external internet is required at runtime.
