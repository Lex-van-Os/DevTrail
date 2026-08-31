# Architecture decisions

This is the index of DevTrail's architecture decisions. Each significant decision is recorded as a
single immutable file under [`adr/`](adr/), using the [MADR 4.0](https://adr.github.io/madr/)
format.

## What gets an ADR here

| Tier | Definition | Where it goes |
| --- | --- | --- |
| **ADR** | Architecturally significant: affects structure, cross-cutting qualities, dependencies, interfaces, or hosting/deployment topology, and is costly to reverse. | A numbered file in [`adr/`](adr/). |
| **Development / tooling decision** | Affects how we build, test, or run the project *locally* — not the deployed system. | The relevant `README.md` (e.g. "Prerequisites"), or folded into a related ADR as a consequence. |
| **Not recorded** | Reversible, obvious, needs no rationale. | Nowhere. |

## How to add one

1. Copy [`adr/0000-madr-template.md`](adr/0000-madr-template.md) to
   `adr/NNNN-short-title.md` (next free number).
2. Fill it in. Set `status: proposed` while it's under discussion, `accepted` once decided.
3. Never edit an accepted ADR's decision. If it changes, write a new ADR and set the old one's
   status to `superseded by ADR-NNNN`.
4. Add a row to the table below.

## Accepted / proposed

| # | Title | Status | Date |
| --- | --- | --- | --- |
| [0001](adr/0001-use-dotnet-isolated-worker-model-for-azure-functions.md) | Use the .NET isolated worker model for Azure Functions | accepted | 2026-08-28 |
| [0002](adr/0002-do-not-adopt-dotnet-aspire.md) | Do not adopt .NET Aspire | accepted | 2026-08-28 |
| [0003](adr/0003-do-not-adopt-docker-hardened-images.md) | Do not adopt Docker Hardened Images | accepted | 2026-08-29 |
| [0004](adr/0004-use-azure-storage-for-terraform-remote-state.md) | Use Azure Storage for Terraform Remote State | accepted | 2026-08-31 |

## Planned (from [`docs/roadmap.md`](../roadmap.md), not yet written)

- Mixed hosting model — Azure Functions for the sync worker, Container Apps for the API and web.
- REST-first via Octokit.NET, GraphQL only where REST has no equivalent (`contributionsCollection`).
- Docker as the deployment unit for the API and web.
- Cached storage pick — Azure Table Storage vs. Cosmos DB free tier.
