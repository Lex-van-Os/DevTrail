---
status: "accepted"
date: 2026-08-28
decision-makers: [Lex van Os]
---

# Use the .NET isolated worker model for Azure Functions

## Context and Problem Statement

`devtrail-sync` is the nightly GitHub sync worker, running on Azure Functions v4 and targeting
.NET 10. Azure Functions offers two .NET execution models: the **in-process** model (function
code runs inside the Functions host process) and the **isolated worker** model (function code
runs in a separate process the host talks to over RPC). The model has to be chosen at scaffold
time — it shapes the project layout, startup code, and dependency set — so it is settled here.

## Decision Drivers

* Long-term support — the worker should not be built on a model with a known end date.
* .NET version freedom — the project targets .NET 10 and wants to stay current.
* Control over startup, DI, and middleware.
* Fit with the chosen hosting plan (Azure Functions Flex Consumption).

## Considered Options

* In-process model
* Isolated worker model

## Decision Outcome

Chosen option: **isolated worker model**.

* Microsoft is retiring the in-process model: support for it ends after **10 November 2026**, and
  it does not support .NET 8+ / .NET 10. Choosing it would mean building on a dead end.
* The isolated model decouples the worker from the host's runtime, so the project can target
  .NET 10 (and future versions) independently of the host.
* It gives a normal `HostApplicationBuilder` startup with full control of DI, configuration, and
  middleware — see [`devtrail-sync/Program.cs`](../../../devtrail-sync/Program.cs).
* It is the model Flex Consumption is designed around.

### Consequences

* Good — forward-supported, .NET-version-independent, explicit host/DI/middleware setup.
* Good — this is effectively the only viable choice for a new .NET Functions project, so the
  decision is stable and unlikely to be revisited.
* Bad — a separate worker process means marginally higher cold-start latency and a slightly
  larger dependency set (`Microsoft.Azure.Functions.Worker.*` and per-trigger worker-extension
  packages).
* Trigger bindings use the worker SDK's types and attributes rather than the in-process ones.

### Confirmation

`devtrail-sync/devtrail-sync.csproj` references `Microsoft.Azure.Functions.Worker` /
`Microsoft.Azure.Functions.Worker.Sdk` (not `Microsoft.NET.Sdk.Functions`), and `Program.cs`
builds its own host. A reviewer checks this at PR time.

## More Information

Summarised for contributors in [`devtrail-sync/README.md`](../../../devtrail-sync/README.md)
("How it works"). Revisit only if Microsoft changes the support model — not expected.
