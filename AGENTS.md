# AGENTS.md — Codex Web Rules (Aspire-first, Offline-Prod Microservices)

## Mission
Generate and maintain a **working, verifiable boilerplate repository** for an enterprise microservices platform.

The repository MUST:
- Build successfully using `dotnet build`
- Pass tests using `dotnet test`

Docker-related artifacts MUST be generated correctly but MUST NOT be executed,
because the Codex Web environment does NOT have Docker.

---

## FIRST PRINCIPLE — Aspire Template Selection (MANDATORY)

Before generating ANY code, the agent MUST:

1. Review the **latest official Microsoft documentation** for .NET Aspire templates.
2. Identify which Aspire template is **most appropriate** for:
   - A multi-service microservices solution
   - API Gateway + UI + background services
   - Local orchestration with offline production deployment
3. Explicitly choose and briefly justify one of:
   - `aspire-starter`
   - Any newer or more specific official Aspire template (if documented)

Only AFTER this evaluation may code generation begin.

The selected template MUST be an **official Microsoft Aspire template**.

---

## Target Stack
- .NET: .NET 10
- UI: Blazor Server
- Gateway: YARP Reverse Proxy
- Dev orchestration & defaults: .NET Aspire
  - AppHost
  - ServiceDefaults
- Workflow: Elsa (separate service; compile-safe stub acceptable)
- Messaging: RabbitMQ
- Primary Database: SQL Server (SQL Authentication ONLY)
- Centralized logging: MongoDB
- Observability: OpenTelemetry (OTLP exporter)

---

## Environment Constraints
- Development environment has internet access.
- Production environment is OFFLINE (no outbound internet).
- Deployment target (current): Linux + Docker Compose.
- Codex Web environment DOES NOT have Docker.
- SQL connections from containers MUST use hostname/IP + port.
- NEVER use `.\Sql2022` or Windows Integrated Authentication.

---

## Repository Structure (EXACT — DO NOT DEVIATE)
/
├─ src
│  ├─ AppHost
│  ├─ ServiceDefaults
│  ├─ Gateway
│  ├─ Ui.BlazorServer
│  ├─ Elsa.Server
├─ services
│  ├─ arc.api
│  ├─ dar.api
│  ├─ dba.api
│  ├─ bud.api
│  ├─ ctr.api
│  ├─ cos.api
│  ├─ rap.api
│  ├─ rde.api
│  ├─ rep.api
│  ├─ rre.api
│  ├─ rst.api
├─ tests
│  ├─ <service>.api.tests (xUnit)
├─ infra
│  ├─ docker-compose.yml
│  ├─ .env.template
│  ├─ otel-collector-config.yaml
├─ nuget.config
├─ azure-pipelines.yml
├─ README.md

---

## Module Services (Required)

Modules (one microservice each):
ARC, DAR, DBA, BUD, CTR, COS, RAP, RDE, REP, RRE, RST

Each module service MUST:
- Be ASP.NET Core Web API targeting .NET 10
- Use minimal APIs
- Reference `ServiceDefaults`
- Expose:
  - GET `/`
  - GET `/health`
  - GET `/api/{module}/ping`

---

## Aspire Service Registration Rules (MANDATORY)

The Aspire **AppHost is the authoritative local orchestrator**.

Requirements:
- EVERY runnable service MUST be:
  - Added to AppHost
  - Referenced explicitly (project reference)
- AppHost MUST register and orchestrate:
  - Gateway
  - Blazor Server UI
  - Elsa.Server
  - All module services (ARC, DAR, DBA, BUD, CTR, COS, RAP, RDE, REP, RRE, RST)

Best-practice expectations:
- Use the **recommended service registration APIs** for the selected Aspire template/version.
- Avoid hardcoding service URLs where Aspire-managed configuration is appropriate.
- Use AppHost to define service relationships and environment variables for local development.
- Docker Compose is for runtime parity; Aspire is for dev orchestration and defaults.

If Aspire APIs differ by version:
- Choose the latest documented pattern
- Briefly explain the choice in comments

---

## Gateway Rules (YARP)
- Gateway is the ONLY external entry point.
- Configure YARP using `LoadFromConfig()` with routes/clusters in `appsettings.json`.
- Provide routing for all module services under `/api/{module}/...`.
- Implement Correlation ID behavior:
  - Generate `X-Correlation-ID` if missing
  - Propagate to downstream services
  - Include in structured logs

---

## SSO Rules (Organization Package)
- Organization provides internal NuGet package exposing `AddSso(...)`.
- Repository MUST compile WITHOUT that package.

Implementation requirement:
- Create an authentication adapter layer:
  - `ISsoConfigurator`
  - `NoopSsoConfigurator`
- Add TODO markers where `AddSso` must be wired (Gateway and optionally services).
- Never hardcode secrets.

---

## Observability Rules
- OpenTelemetry configured in `ServiceDefaults`
- Traces + metrics enabled
- OTLP exporter
- Exporter endpoint read from `OTEL_EXPORTER_OTLP_ENDPOINT`
- Docker Compose MUST define:
  - otel-collector
  - optional jaeger (UI only; not executed by Codex)

---

## Messaging Rules (RabbitMQ)
- Docker Compose MUST define RabbitMQ (management enabled).
- Code MUST include skeletons for:
  - Event contracts (versioned)
  - Publisher
  - Consumer
- Include TODO stubs for:
  - Transactional Outbox
  - Idempotent consumers
  - Dead-letter queues

---

## Offline Production Guardrails
- `nuget.config` must support internal feed override.
- Docker images must use explicit tags.
- `infra/.env.template` must include:
  - SQL passwords per service
  - RabbitMQ credentials
- No runtime dependency on external internet.

---

## CI/CD (Azure DevOps)
- Provide `azure-pipelines.yml` that:
  - Restores, builds, tests
  - Defines Docker build steps (definition only; not executed here)
- Must be compatible with self-hosted agents.

---

## Codex Web Verification Rules (CRITICAL)
Codex MUST run:
- `dotnet build`
- `dotnet test`

Codex MUST NOT attempt:
- `docker`
- `docker compose`

Docker correctness is validated by:
- File structure
- Configuration correctness
- README instructions

---

## Output Expectations
When generating code:
- Maintain exact structure
- Ensure build/tests pass
- If anything is ambiguous, ASK BEFORE generating
