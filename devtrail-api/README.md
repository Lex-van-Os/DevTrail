# DevTrail API

## What it is
`devtrail-api` is DevTrail's backend: an ASP.NET Core Minimal API. Per the project's [roadmap](../docs/roadmap.md), it's intended to serve cached portfolio data to `devtrail-web`.

## How it works
- Built with the Minimal API pattern (no MVC controllers), on **.NET 10**.
- Currently exposes:
  - `GET /health` — health check; writes and reads back a row in an Azure Table Storage `healthcheck` table via `Azure.Data.Tables`, so a 200 also confirms storage connectivity.
  - `GET /weatherforecast` — template sample endpoint left over from scaffolding.
- OpenAPI is enabled via `Microsoft.AspNetCore.OpenApi` and served at `/openapi/v1.json` in the Development environment.
- Tests live in the [`devtrail-api.Tests`](../devtrail-api.Tests) project, using `WebApplicationFactory<Program>` for in-process integration tests. May include unit tests in the future.

## Architecture
See the root [README](../README.md#architecture) and [`docs/roadmap.md`](../docs/roadmap.md) for the wider system's architecture; formal ADRs are indexed in [`docs/architecture/architecture-decisions.md`](../docs/architecture/architecture-decisions.md), with the records under [`docs/architecture/adr/`](../docs/architecture/adr/). This project's container image is built on Docker Official / vendor images, not Docker Hardened Images ([ADR-0003](../docs/architecture/adr/0003-do-not-adopt-docker-hardened-images.md)).

## Prerequisites
- .NET 10 SDK
- [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite) running locally — `/health` connects to Table Storage, so it (and the tests that call it) needs a reachable emulator

## Install
```bash
dotnet restore
```

## Common commands
```bash
dotnet run --project devtrail-api      # run locally
dotnet watch --project devtrail-api    # run with hot reload
dotnet test                            # run devtrail-api.Tests (from repo root)
docker compose up devtrail-api azurite # run against a containerized Azurite instead (from repo root)
```

## Troubleshooting
- Open the repo via `DevTrail.slnx` (not this folder alone, and not the bare `.csproj`) — opening it standalone can prevent Rider from finding the project when adding run configurations.
- `/health` (and `devtrail-api.Tests`) failing with a storage connection error means Azurite isn't running — start it with `azurite --silent --location ~/.azurite` in a separate terminal, or use `docker compose up`.
