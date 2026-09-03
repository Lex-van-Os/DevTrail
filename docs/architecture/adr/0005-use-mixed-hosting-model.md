---
status: "accepted"
date: 2026-09-02
decision-makers: [Lex van Os]
---

# Use a Mixed Hosting Model for Compute

## Context and Problem Statement

DevTrail has two distinct kinds of compute: a nightly data-sync job (reads GitHub, writes to cached storage) and the user-facing services that serve the dashboard (the API and the web front-end). These have very different behavior and criticality profiles. This ADR settles whether all three run on the same hosting model, or whether the sync job is deliberately hosted differently from the two user-facing services.

## Decision Drivers

* Criticality — the sync job is not user-facing and isn't critical to serve instantly; the API and web app are, since a visitor loading the dashboard expects a prompt response at any time.
* Separation of concerns — the sync job's own failure modes (a GitHub rate limit, a transient network error) shouldn't be able to affect the API's availability. Running them as genuinely separate deployable units keeps a bad sync run from ever touching the always-on, constantly-deployed API/web processes.
* Usage pattern — the sync job only needs to run once a day, for a short duration, sitting idle the rest of the time. The API and web app need to be ready to respond at any moment while the app is live.
* Cost — paying only for compute while it actually runs fits a once-a-day job; paying for always-warm compute fits a user-facing service that can't tolerate a cold start on a visitor's first request.
* Learning value — this is a personal project explicitly meant to build and demonstrate breadth. Deliberately working with more than one Azure compute model, and the responsibilities that come with managing each, is itself a goal of the project, not just an implementation detail.

## Considered Options

* Mixed hosting — Azure Functions (Timer Trigger) for the sync job, Azure Container Apps for the API and web app
* Single hosting model — everything on Azure Container Apps (the sync job as a scheduled Container Apps Job, or a timer-woken revision)
* Single hosting model — everything on Azure Functions (API and web app as HTTP-triggered Functions)

## Decision Outcome

Chosen option: **Mixed hosting — Azure Functions for the sync job, Container Apps for the API and web app**, because the sync job and the user-facing services have genuinely different criticality, usage patterns, and failure-isolation needs, and no single hosting model serves both well without a real compromise on one side.

### Consequences

* Good, because the sync job's cost tracks its actual usage — a few minutes once a day — rather than being billed for constant availability it doesn't need.
* Good, because a bad sync run can't take down or slow the API/web app: separate processes, separate deployments, separate failure domains, not just a coding convention.
* Good, because it's a deliberate opportunity to work with two different Azure compute models end-to-end (isolated-worker Azure Functions, and containerized Azure Container Apps) rather than defaulting to one for everything.
* Bad, because it's two hosting models to understand, configure, and monitor instead of one — more moving parts across Terraform, CI, and the mental model of "where does X run."
* Neutral, because the two sides must still share the same cached-storage layer as their integration point — the sync job writes what the API reads — so this decision doesn't remove the need for a well-defined data contract between them.

### Confirmation

`devtrail-sync` is scaffolded as an Azure Functions isolated-worker project with a Timer Trigger (see [ADR-0001](0001-use-dotnet-isolated-worker-model-for-azure-functions.md)); `devtrail-api` and `devtrail-web` are both Dockerized for Container Apps. A reviewer checks that no later change collapses these back onto a single hosting model without a new ADR superseding this one.

## Pros and Cons of the Options

### Mixed hosting (Functions for sync, Container Apps for API/web)

* Good, because cost and behavior each match the workload that actually needs them.
* Good, because failure isolation between the sync job and the user-facing services is structural, not just a coding convention.
* Neutral, because it requires learning and maintaining two hosting models instead of one.

### Single hosting — everything on Container Apps

* Good, because it's one platform, one deployment model, one place to look for logs and metrics.
* Bad, because a scheduled job on Container Apps is a less natural fit for "runs for a couple of minutes once a day" than Functions' native Timer Trigger, and the API/web app's always-on revisions would still bill regardless of the sync job's own idle time — no cost benefit gained by consolidating.

### Single hosting — everything on Azure Functions

* Good, because it's one platform, one deployment model.
* Bad, because HTTP-triggered Functions on a consumption-style plan can cold-start, a poor fit for a public dashboard where every visitor's first request should be fast. Avoiding that means paying for an always-warm plan for the API too, undoing the exact cost benefit Functions offers for the sync job.

## More Information

See [`docs/roadmap.md`](../../roadmap.md), which already states this decision as part of the project's tech stack: *"The sync worker deliberately stays on Azure Functions rather than moving to a container job alongside the API and frontend... This mixed hosting model is a deliberate choice, not an oversight."* Revisit if the sync job ever needs to run more frequently than daily, or needs to react to real-time events rather than a fixed schedule — at that point the cost/idle-time tradeoff that favors Functions today may no longer hold.
