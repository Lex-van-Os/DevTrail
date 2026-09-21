---
status: "accepted"
date: 2026-09-08
decision-makers: [Lex van Os]
---

# Use Azure Table Storage for Cached Portfolio Data

## Context and Problem Statement

The nightly sync (`devtrail-sync`) reads a small, hand-maintained set of repositories from
GitHub and must persist the result so `devtrail-api` can serve it without a live GitHub call.
Two record types are written (see [`devtrail-sync/Models`](../../../devtrail-sync/Models)):

* `RepositorySnapshot` — description, name, owner, a list of languages, last commit date.
* `SolvedChallengesSnapshot` — a per-language solved-challenge count for `code-challenges`.

Volume is kilobytes and grows only when the tracked-repository list is edited by hand. The
access pattern is: nightly upsert of the full set, and a read of everything to render one
dashboard. There is no filtering or sorting on non-key fields, no secondary-index lookup, and
no join.

[`docs/roadmap.md`](../../roadmap.md) lists the store as *"Azure Table Storage or Cosmos DB
free tier"* and defers the choice to this ADR.

## Decision Drivers

* Fit to the access pattern — point reads, small partition scans, and nightly upserts; none
  of Cosmos DB's richer querying, indexing, or distribution features are exercised.
* Existing code — `devtrail-api` already talks to Table Storage through `Azure.Data.Tables`
  / `TableServiceClient` (the `/health` endpoint).
* Local development — Azurite already emulates Table Storage for the app and its tests
  ([ADR-0002](0002-do-not-adopt-dotnet-aspire.md)); Azurite does not emulate Cosmos DB, which
  has its own heavier emulator.
* Cost — Table Storage is effectively free at this scale with no allocation to claim; Cosmos
  DB's free tier is one per subscription and would be spent here for no benefit.
* Terraform footprint — a storage account plus a table is a small resource; a Cosmos DB
  account is heavier, slower to provision, and has more to configure.
* Reversibility — both are reachable through the same `Azure.Data.Tables` surface, so a later
  move to the Cosmos DB Table API is a connection-string and Terraform change, provided reads
  always filter by partition key.

## Considered Options

* Azure Table Storage.
* Azure Cosmos DB (Table API) on the provisioned free tier.
* Azure Cosmos DB (Table API) serverless.

## Decision Outcome

Chosen option: **Azure Table Storage**, because the data and access pattern are a direct fit
for what Table Storage does, it is already the store the API uses and Azurite already
emulates, and it needs no free-tier allocation and only a small Terraform resource. Cosmos
DB's advantages address needs this project does not have.

### Key design

Entities are partitioned by repository (one partition per tracked repo):

* `PartitionKey` = `{owner}__{name}` (`/`, `\`, `#`, `?` are illegal in keys, so `__` is the
  separator).
* `RowKey` = `metadata` for the `RepositorySnapshot` row.
* `RowKey` = `challenge__{language}` for each `SolvedChallengesSnapshot` row.

The nightly write for one repository is then a single-partition transactional batch
(`SubmitTransactionAsync`): the `metadata` row and all `challenge__*` rows are upserted
together, and rows for languages that no longer exist are deleted in the same batch. Reads
always filter by `PartitionKey`; a full-table scan (to enumerate all repos) is treated as a
temporary convenience given the tiny row count, not part of the design.

### Mapping layer

Table Storage properties are scalars, so `RepositorySnapshot.Languages` (a `List<string>`) is
stored as a JSON string property. A mapping layer in both `devtrail-sync` and `devtrail-api`
converts between the stored entity form (flat, JSON-encoded list) and the
`RepositorySnapshot` / `SolvedChallengesSnapshot` domain models the service layers work with.
The storage representation and the domain representation are deliberately not the same type.

### Consequences

* Good, because the store matches the workload exactly — nothing is provisioned or paid for
  that the access pattern does not use.
* Good, because `devtrail-api` keeps one storage client and one local-dev emulator (Azurite)
  across the health check and the cached data.
* Good, because the per-repository partition makes each nightly write atomic, so the API
  never serves a repo's fresh metadata against stale challenge counts.
* Bad, because Table Storage indexes only `PartitionKey` + `RowKey`; any future need to
  filter or sort cached data on another field means a table scan or a redesign.
* Bad, because the `List<string>` of languages cannot be stored natively and must be
  JSON-encoded and decoded in the mapping layer on every read and write.
* Neutral, because the cached data and the existing `healthcheck` table share a storage
  account provisioned for application data, kept separate from the Function host's own
  `AzureWebJobsStorage` account per Microsoft's guidance — a second small Terraform resource,
  no material cost.

### Confirmation

`devtrail-sync` and `devtrail-api` both use `Azure.Data.Tables` against a Table Storage
endpoint; `devtrail-infra` contains no `azurerm_cosmosdb_account`. Stored entities use
`PartitionKey` = `{owner}__{name}` with `metadata` / `challenge__{language}` row keys, writes
per repository go through a single-partition transaction, and a mapping layer separates stored
entities from domain models. A reviewer checks that reads filter by `PartitionKey` and that
no query depends on a scan over a non-key field.

## Pros and Cons of the Options

### Azure Table Storage

* Good, because it fits the point-read / small-scan / nightly-upsert pattern with no unused
  capability.
* Good, because it reuses the client, connection style, and Azurite emulator already in the
  codebase.
* Good, because its Terraform resource (storage account + table) is small and quick to
  provision.
* Neutral, because the languages list must be JSON-encoded, since Table Storage has no array
  type.
* Bad, because it has a single index — non-key queries are scans.

### Azure Cosmos DB (Table API), provisioned free tier

* Good, because the free tier (1,000 RU/s + 25 GB) would cover this workload at no cost, and
  automatic indexing would remove the non-key-query limitation.
* Bad, because the free tier is one per subscription — a scarce allocation spent on a
  workload that needs none of what it provides.
* Bad, because Azurite does not emulate Cosmos DB, so local development and tests would need
  the separate Cosmos DB emulator or a real account.
* Bad, because the Terraform resource is a full Cosmos DB account — heavier, slower, and more
  to configure than a storage account.

### Azure Cosmos DB (Table API), serverless

* Good, because it costs only per request consumed — pennies at this volume — and spends no
  free-tier allocation.
* Bad, because it still adds the heavier Cosmos DB account to Terraform and still has no
  Azurite support, for capabilities the project does not use.

## More Information

See [`docs/roadmap.md`](../../roadmap.md) (tech stack and the backlog note deferring this
pick). This decision is distinct from [ADR-0004](0004-use-azure-storage-for-terraform-remote-state.md),
which covers Azure Storage for Terraform *remote state*; this ADR covers Azure Storage for
*application cache data*. Local emulation via Azurite is established by
[ADR-0002](0002-do-not-adopt-dotnet-aspire.md).

Pricing references:
<https://azure.microsoft.com/en-us/pricing/details/storage/tables/> and
<https://learn.microsoft.com/en-us/azure/cosmos-db/free-tier>.

Revisit if the tracked-repository list grows large enough that reading the whole set stops
being cheap, if the API needs server-side filtering or sorting on non-key fields, or if a
feature needs multi-region writes or a latency SLA — at which point the Cosmos DB Table API
(reached through the same SDK) should be re-evaluated under a new ADR.
