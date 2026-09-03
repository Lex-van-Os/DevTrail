# DevTrail Sync

## What it is
`devtrail-sync` is an Azure Functions (.NET isolated worker) project. Per the project's [roadmap](../docs/roadmap.md), it's intended to run the nightly GitHub sync job, but no sync logic exists yet — see [How it works](#how-it-works) for what's actually implemented today.

## How it works
- Isolated worker model (not in-process), targeting **.NET 10**, on Azure Functions v4.
- Currently contains a single stub function:
  - `DevTrailSync` — a Timer Trigger (`0 0 0 * * *`, daily at midnight) that logs a message. No sync logic yet.
- Hosting target: Azure Functions **Flex Consumption** plan.
- Application Insights/OpenTelemetry is wired in [`Program.cs`](Program.cs), active only when `APPLICATIONINSIGHTS_CONNECTION_STRING` is set.

## Architecture
See the root [README](../README.md#architecture) and [`docs/roadmap.md`](../docs/roadmap.md) for the wider system's architecture; formal ADRs are indexed in [`docs/architecture/architecture-decisions.md`](../docs/architecture/architecture-decisions.md), with the records under [`docs/architecture/adr/`](../docs/architecture/adr/). The isolated worker model ([ADR-0001](../docs/architecture/adr/0001-use-dotnet-isolated-worker-model-for-azure-functions.md)) and REST-first GitHub API consumption ([ADR-0006](../docs/architecture/adr/0006-rest-first-github-api-consumption.md)) are the ones that touch this project; the Azurite-based local dev setup is a development/tooling decision (see this README's own Prerequisites), not an ADR.

## Prerequisites
- .NET 10 SDK
- [Azure Functions Core Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local) v4
- [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite) running locally — the Timer trigger's listener needs a working `AzureWebJobsStorage` connection to start, even before any sync logic is added

## Install
```bash
dotnet restore
```

## Common commands
```bash
dotnet run --project devtrail-sync     # run locally via Azure Functions Core Tools
```

To manually invoke the function locally instead of waiting for its schedule:
```bash
curl -i -X POST http://localhost:7071/admin/functions/DevTrailSync \
  -H "Content-Type: application/json" \
  -d "{}"
```

## Troubleshooting
- Open the repo via `DevTrail.slnx` (not this folder alone) so Rider picks up the project alongside `devtrail-api`.
- Listener fails to start with a connection-refused error on port `10000`: Azurite isn't running — start it with `azurite --silent --location ~/.azurite` in a separate terminal first.
