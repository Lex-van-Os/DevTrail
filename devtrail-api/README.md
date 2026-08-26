# DevTrail API

## What it is
`devtrail-api` is DevTrail's backend: an ASP.NET Core Minimal API. Per the project's [roadmap](../docs/roadmap.md), it's intended to serve cached portfolio data to `devtrail-web`.

## How it works
- Built with the Minimal API pattern (no MVC controllers), on **.NET 10**.
- Currently exposes:
  - `GET /health` — health check, returns 200.
  - `GET /weatherforecast` — template sample endpoint left over from scaffolding.
- OpenAPI is enabled via `Microsoft.AspNetCore.OpenApi` and served at `/openapi/v1.json` in the Development environment.
- Tests live in the [`devtrail-api.Tests`](../devtrail-api.Tests) project, using `WebApplicationFactory<Program>` for in-process integration tests. May include unit tests in the future.

## Architecture
See the root [README](../README.md#architecture) and [`docs/roadmap.md`](../docs/roadmap.md) for the wider system's architecture; formal ADRs live in [`docs/architecture/architecture-decisions.md`](../docs/architecture/architecture-decisions.md) (currently a work in progress).

## Prerequisites
- .NET 10 SDK

## Install
```bash
dotnet restore
```

## Common commands
```bash
dotnet run --project devtrail-api      # run locally
dotnet watch --project devtrail-api    # run with hot reload
dotnet test                            # run devtrail-api.Tests (from repo root)
```

## Troubleshooting
- Open the repo via `DevTrail.slnx` (not this folder alone, and not the bare `.csproj`) — opening it standalone can prevent Rider from finding the project when adding run configurations.
