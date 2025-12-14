# AGENTS.md — Repository Rules for Codex (Offline-Prod Microservices)

## Mission
Generate and maintain a working boilerplate repository for an enterprise microservices platform.

The repository MUST:
- Build successfully (`dotnet build`)
- Pass tests (`dotnet test`)
- Build Docker images via compose (`docker compose -f infra/docker-compose.yml --env-file infra/.env.template build`)

## Target Stack
- .NET: .NET 10
- UI: Blazor Server
- Gateway: YARP Reverse Proxy
- Dev orchestration & defaults: .NET Aspire (AppHost + ServiceDefaults)
- Workflow: Elsa (separate service; compile-safe stub is acceptable if version is unknown)
- Messaging: RabbitMQ
- Primary DB: SQL Server (SQL Authentication only)
- Centralized logs: MongoDB
- Observability: OpenTelemetry (OTLP exporter)

## Environment Constraints
- Development has internet access.
- Production is OFFLINE (no outbound internet).
- Deployment target (for now): Linux + Docker Compose.
- SQL connection from containers MUST use hostname/IP + port (never use `.\Sql2022`).

## Repository Structure (Mandatory)
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
│  ├─ con.api
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

## Module Services (Required)
Modules (one microservice each):
ARC, DAR, DBA, BUD, CON, COS, RAP, RDE, REP, RRE, RST

Each module service MUST:
- Be ASP.NET Core Web API targeting .NET 10
- Use minimal APIs
- Reference `ServiceDefaults`
- Expose these endpoints:
  - GET `/`                     → service marker
  - GET `/health`               → health checks
  - GET `/api/{module}/ping`    → module marker

## Gateway Rules (YARP)
- Gateway is the ONLY external entry point.
- Configure YARP using `LoadFromConfig()` and `ReverseProxy` section in appsettings.json.
- Provide complete routes/clusters for all module services under `/api/{module}/...`.
- Implement correlation-id behavior:
  - If request lacks `X-Correlation-ID`, generate one
  - Forward it to downstream services
  - Include it in logs

## SSO Rules (Org Package)
- The organization provides an internal NuGet package with an extension method `AddSso(...)`.
- The repository MUST compile WITHOUT this package.
Implementation requirement:
- Create an `Auth` adapter layer with:
  - `ISsoConfigurator` interface
  - `NoopSsoConfigurator` default implementation
- Add TODO markers where `AddSso` is expected to be invoked (Gateway and optionally services).
- Never hardcode secrets.

## Observability Rules
- Implement OpenTelemetry in `ServiceDefaults`:
  - traces + metrics
  - OTLP exporter
- Read exporter endpoint from environment variable:
  - `OTEL_EXPORTER_OTLP_ENDPOINT`
- Compose must include `otel-collector` and optional `jaeger` UI.
- Logs should be structured and include:
  - service name
  - environment
  - correlation id
  - request id (if available)

## Messaging Rules (RabbitMQ)
- Compose includes RabbitMQ (management enabled).
- Provide code skeleton in each service:
  - Publisher
  - Consumer
  - Event contracts (versioned, e.g. v1)
- Include skeleton notes for:
  - Outbox pattern
  - Idempotent consumer handling
  - Dead-letter queues (DLQ)
These can be TODO but must exist as stubs.

## Offline Production Guardrails
- `nuget.config` must support an internal feed override and deterministic restores.
- `docker-compose.yml` should use explicit image tags.
- `infra/.env.template` must include:
  - RabbitMQ user/pass
  - SQL passwords per service
- No runtime dependency on external internet.

## CI/CD (Azure DevOps)
- Provide `azure-pipelines.yml` that:
  - restores, builds, tests
  - builds Docker images
  - publishes artifacts (compose + env template)
- Must work with self-hosted agents (no cloud dependencies assumed).

## Output Expectations for Any Code Generation
When generating code or modifying repo:
- Keep structure exact
- Ensure build/test/compose build succeed
- If something is ambiguous, ask before generating
