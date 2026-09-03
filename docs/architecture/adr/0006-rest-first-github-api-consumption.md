---
status: "accepted"
date: 2026-09-03
decision-makers: [Lex van Os]
---

# Use REST for GitHub API Consumption, GraphQL Only for contributionsCollection

## Context and Problem Statement

`devtrail-sync` is the only component that talks to GitHub: a nightly job that reads a
handful of repositories (`code-challenges`, `Steward` for v1) and pulls each one's
description, language breakdown, last-activity timestamp, and per-language solved-challenge
counts, then writes the results to cached storage. GitHub exposes two APIs for this — a REST
API and a GraphQL API — and one planned v2 feature, the contribution calendar
(`contributionsCollection`), exists **only** in GraphQL with no REST equivalent. This ADR
settles which API the sync worker consumes, and whether GraphQL is taken on for anything
beyond the one case that forces it.

This decision concerns how `devtrail-sync` *reads from GitHub*. It does not affect
`devtrail-api`'s own contract with `devtrail-web`, which is a REST Minimal API and is
settled separately.

## Decision Drivers

* Tooling maturity — the .NET client story is lopsided: Octokit.NET (REST) is stable,
  official, and widely used; Octokit.GraphQL.NET is still pre-1.0 (0.4.0-beta,
  prerelease-only on NuGet).
* Simplicity and surface area — the project favours the minimum moving parts; every added
  client, auth wiring, and error model is a cost.
* Workload shape — a nightly job over ~2 repositories uses a negligible fraction of either
  API's rate budget, so raw efficiency (fewer round trips, fewer rate-limit units) is not a
  deciding factor here.
* Conditional caching — REST supports ETag / `If-None-Match` → `304 Not Modified`, and 304s
  do not count against the rate limit; a nightly poll where most repos are unchanged
  benefits from this. GraphQL is POST-only with no HTTP caching.
* Forced dependency — `contributionsCollection` has no REST equivalent, so the contribution
  calendar (roadmap Phase 2a) cannot be built without at least one GraphQL call.
* Stack consistency — everything else in DevTrail speaks REST; keeping GitHub consumption on
  REST wherever possible means one mental model for almost the whole integration.

## Considered Options

* REST-first — Octokit.NET for all repo data; a single hand-written GraphQL query over
  `HttpClient` for `contributionsCollection` only, behind a small interface.
* REST-first, but adopt Octokit.GraphQL.NET for the `contributionsCollection` call.
* GraphQL for everything — one client and one query model for all GitHub reads.

## Decision Outcome

Chosen option: **REST-first — Octokit.NET for all repo data, and for
`contributionsCollection` a single hand-written GraphQL query over `HttpClient` behind a
small interface (e.g. `IContributionCalendarSource`)** — because it keeps the whole
integration on the mature, stable client except for the one field that genuinely requires
GraphQL, and it adds that one field without taking on a pre-1.0 SDK.

The GraphQL surface is deliberately kept to a single query. If DevTrail later needs several
GraphQL-only fields, adopting a real GraphQL client (Octokit.GraphQL.NET, or a code-gen
client such as StrawberryShake) should be reconsidered under a new ADR.

### Consequences

* Good, because all v1 data and most of v2 runs on Octokit.NET — stable, typed, handles
  pagination, auth, rate-limit headers, and conditional requests with no beta dependency.
* Good, because the nightly sync can issue conditional REST requests and get free `304`s for
  unchanged repositories.
* Good, because the GraphQL footprint is one query and one `HttpClient` call — small enough
  to isolate, test, and reason about, and to swap later without disturbing the rest of the
  sync.
* Bad, because from Phase 2a onward the sync worker carries two HTTP clients, two auth
  wirings (the same Key Vault PAT, passed to each), and two error models — GraphQL returns
  HTTP 200 with an `errors` array rather than HTTP status codes.
* Neutral, because the `contributionsCollection` query is capped at a one-year range, so a
  multi-year calendar means one query per year — an implementation detail, not an
  architectural constraint.

### Confirmation

`devtrail-sync` references `Octokit` (REST) for repo data; any GraphQL call is a direct
`HttpClient` POST to `https://api.github.com/graphql` behind a dedicated interface, and
`Octokit.GraphQL` does **not** appear in `devtrail-sync.csproj`. A reviewer checks this at
PR time; introducing a GraphQL SDK, or moving v1 repo reads to GraphQL, requires a new ADR.

## Pros and Cons of the Options

### REST-first, hand-written GraphQL query for contributionsCollection only

* Good, because the mature client covers everything it can, and the forced GraphQL case is
  added with zero new package dependencies.
* Good, because conditional caching keeps repeated nightly syncs cheap on the REST side.
* Neutral, because two API styles still coexist in the worker from Phase 2a on, just with
  the GraphQL side kept as small as possible.

### REST-first, adopt Octokit.GraphQL.NET for the contributionsCollection call

* Good, because a strongly-typed LINQ-style query is more ergonomic than a raw query string.
* Bad, because it pulls a pre-1.0, prerelease-only library into the build to serve exactly
  one query — disproportionate, and exposed to the library's API churn.

### GraphQL for everything

* Good, because one client and one query model for all GitHub reads.
* Bad, because it means rewriting the v1 repo pulls as GraphQL, losing REST's conditional
  caching, and either adopting the beta SDK or hand-maintaining every query — all to unify a
  handful of calls that Octokit.NET already handles cleanly.

## More Information

See [`docs/roadmap.md`](../../roadmap.md), which already frames this as *"Octokit.NET (REST)
for all v1 data; GraphQL against GitHub's API for `contributionsCollection` only, in v2
(REST has no equivalent for that data)."* Fine-grained PATs support the GraphQL API for the
`User.contributionsCollection` field (the standing fine-grained gap is personal
Projects / ProjectV2, which DevTrail does not use), so the same Key Vault PAT works for both
clients. Revisit if DevTrail comes to need multiple GraphQL-only fields, at which point a
dedicated GraphQL client should be re-evaluated.

* GitHub REST rate limits:
  <https://docs.github.com/en/rest/using-the-rest-api/rate-limits-for-the-rest-api>
* GitHub GraphQL rate and point limits:
  <https://docs.github.com/en/graphql/overview/rate-limits-and-query-limits-for-the-graphql-api>
