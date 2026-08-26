# DevTrail API Tests

## What it is
xUnit test project for [`devtrail-api`](../devtrail-api), using `WebApplicationFactory<Program>` to run integration tests, and possible future unit tests, against the API in-process.

## How it works
- References `devtrail-api` directly and spins it up in-memory per test via `WebApplicationFactory<Program>`.
- Current coverage:
  - `HealthEndpointTests` — `GET /health` returns success.
  - `WeatherForecastEndpointTests` — `GET /weatherforecast` returns success.

## Prerequisites
- .NET 10 SDK

## Common commands
```bash
dotnet test                            # run from the repo root
dotnet test devtrail-api.Tests         # run just this project
```

## Troubleshooting
- Open the repo via `DevTrail.slnx` so Rider's Tests tool window picks up this project alongside `devtrail-api`.
