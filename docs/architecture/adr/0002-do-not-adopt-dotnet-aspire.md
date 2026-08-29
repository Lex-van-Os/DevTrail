---
status: "accepted"
date: 2026-08-28
decision-makers: [Lex van Os]
---

# Do not adopt .NET Aspire

## Context and Problem Statement

DevTrail is three compute components — `devtrail-api` (ASP.NET Core Minimal API), `devtrail-sync`
(Azure Functions worker), and `devtrail-web` (Next.js) — plus Azure Storage, Key Vault, and
Application Insights. .NET Aspire targets exactly this shape: an AppHost project that orchestrates
the components for local development, a ServiceDefaults project for shared telemetry/health/
resilience wiring, and integrations for backing services. Aspire was evaluated before scaffolding
to decide whether DevTrail should be built around it or use plain project references plus Docker
Compose for local orchestration.

## Decision Drivers

* Keep the moving parts proportional to a three-service portfolio project.
* Local dev / orchestration story that a solo developer (and coding agents) can run and reason about.
* Fit with the already-chosen deployment pipeline: Terraform (`azurerm`) → Azure Container Apps
  for API/web + a Function App for sync, deployed via GitHub Actions.
* A polyglot stack — the frontend is Next.js, not .NET.
* A weighing factor of wanting more hands-oon experience with technologies such as Terraform, Docker and Azure services.

## Considered Options

* Adopt .NET Aspire (AppHost + ServiceDefaults + integrations).
* No Aspire — plain project references, Docker Compose for local orchestration, platform
  emulators (Azurite) for backing services, telemetry wired per-project.

## Decision Outcome

Chosen option: **no Aspire**.

* Frontend Next.js is only partially covered by Aspire orchestration, which means that a full fledged coverage cannot be guaranteed when using Aspire only.
* Addoption of Aspire may take away possibilities to implement CI/CD with Terraform, which is a technology that the developer wants to get more familiar with through this project.
* Immediate addoption of Aspire may add to project development time, which is undesirable, considering the smaller scale of the project at the moment.
* Aspire adds two new projects (AppHost + ServiceDefaults) to the solution, which is disproportionate for three to four services.

### Consequences

* Local development is **Docker Compose**, covering `devtrail-api` + Azurite + the Next.js dev
  server, per [`docs/roadmap.md`](../../roadmap.md).
* **Azurite** is the local Table Storage emulator, used both for running the app locally and for
  tests — `devtrail-sync`'s Timer trigger listener needs a working `AzureWebJobsStorage`
  connection to start at all. This is the project's answer to "how do we develop and test Azure
  Storage functionality locally"; it is documented as a prerequisite in
  [`devtrail-sync/README.md`](../../../devtrail-sync/README.md).
* No ServiceDefaults project, so OpenTelemetry / Application Insights is wired by hand in each
  component — already done in
  [`devtrail-api/Program.cs`](../../../devtrail-api/Program.cs) and
  [`devtrail-sync/Program.cs`](../../../devtrail-sync/Program.cs).
* If the system grows well beyond three services, or local orchestration becomes painful,
  re-evaluate — Aspire can be introduced later without rewriting the components.

### Confirmation

No `*.AppHost` or `*.ServiceDefaults` project exists in `DevTrail.slnx`, and no
`Aspire.*` / `Aspire.Hosting.*` package references appear in any `.csproj`.

## More Information

.NET Aspire docs: <https://learn.microsoft.com/en-us/dotnet/aspire/>. This decision pairs with
the planned "mixed hosting model" ADR (Functions for sync, Container Apps for API/web).
