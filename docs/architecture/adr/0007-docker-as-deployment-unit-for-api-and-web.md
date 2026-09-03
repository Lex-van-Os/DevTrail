---
status: "accepted"
date: 2026-09-03
decision-makers: [Lex van Os]
---

# Use Docker Images as the Deployment Unit for the API and Web

## Context and Problem Statement

DevTrail has two user-facing services: `devtrail-api` (ASP.NET Core Minimal API) and
`devtrail-web` (Next.js 16, App Router). [ADR-0005](0005-use-mixed-hosting-model.md) settled
that both run on Azure Container Apps. This ADR settles what the *deployment artifact* is for
those two services — a Docker/OCI image built in CI and pushed to a registry, versus a
platform-managed source or framework deploy such as Azure Static Web Apps for the web tier or
source-based deployment to the platform.

Azure Static Web Apps was evaluated specifically for `devtrail-web`, because a public
dashboard whose data refreshes nightly has no hard per-request server-rendering requirement
and could in principle be a static site.

## Decision Drivers

* One deployment model across both user-facing services — one CI pattern and one mental model
  of "how a service ships".
* Fit with already-decided infrastructure — a Container Apps environment and a container
  registry are required for the API regardless ([ADR-0005](0005-use-mixed-hosting-model.md)),
  so a web image is marginal additional infrastructure.
* Framework-feature freedom — a Docker image runs the full Next.js standalone server, so any
  App Router capability (server components, middleware, server actions) stays available for v2
  without a hosting migration.
* Avoiding preview dependencies — Static Web Apps' hybrid Next.js support is still in preview,
  with material limits: a 250 MB app cap, no linked backend to Azure Container Apps, and no
  ISR image caching.
* Infrastructure defined in one place — an image plus a Container App revision is fully
  expressible in Terraform ([ADR-0004](0004-use-azure-storage-for-terraform-remote-state.md));
  Static Web Apps deploys content through its own token-based pipeline, outside Terraform.
* Cost and cold-start at portfolio traffic — scale-to-zero Container Apps are near-free but
  cold-start on the first request after idle; this is a manageable tradeoff, not a reason to
  split the model.
* Learning value — this is a personal project meant to build breadth; containerising both
  services and shipping them through a registry, Container Apps, and Terraform is itself a
  goal.

## Considered Options

* Docker image as the deployment unit for both API and web — multi-stage build in CI, push to
  Azure Container Registry, Container Apps runs a pinned image tag.
* Azure Static Web Apps for `devtrail-web` (static export or hybrid), Docker on Container Apps
  for `devtrail-api` only.
* Source-based deployment to Container Apps / App Service — no explicit image, the platform
  builds from source.

## Decision Outcome

Chosen option: **Docker image as the deployment unit for both `devtrail-api` and
`devtrail-web`**, because it gives the two user-facing services one build-and-ship pattern,
reuses infrastructure the API already needs, keeps the full Next.js server available for later
features, and avoids depending on a preview-stage hosting feature.

A multi-stage Dockerfile per service produces the image; CI pushes it to Azure Container
Registry; each Container App runs a pinned image tag. `devtrail-web` uses Next.js
`output: "standalone"` to keep the image small.

### Consequences

* Good, because there is one pattern for both user-facing services — multi-stage Dockerfile →
  Azure Container Registry → Container App revision — wired the same way in Terraform and in
  GitHub Actions.
* Good, because the web tier keeps the full Next.js server, so v2 features (admin auth in
  middleware, server actions for description edits) need no hosting change.
* Good, because the hosting target is GA and well understood — no preview-stage platform
  dependency.
* Good, because the image and its Container App revision are fully expressed in Terraform, so
  infrastructure and its state stay in one place.
* Bad, because a scale-to-zero web container cold-starts on the first request after idle;
  mitigated by accepting it for v1 or by setting a minimum of one replica at a small recurring
  cost. A static CDN host would serve that first request instantly.
* Bad, because DevTrail maintains its own Dockerfiles and base-image upkeep for both services
  rather than delegating the web build to a managed service; partly offset by
  [ADR-0003](0003-do-not-adopt-docker-hardened-images.md) keeping images on official bases.
* Neutral, because Static Web Apps' built-in Entra ID auth and free per-PR preview
  environments are given up; the v2 admin login is implemented in-app, and preview
  environments, if wanted, are built on Container Apps revisions.

### Confirmation

Each of `devtrail-api` and `devtrail-web` has a Dockerfile and is deployed as an image from
Azure Container Registry to its Container App; no `azurerm_static_web_app` resource exists and
no `Azure/static-web-apps-deploy` step appears in CI. A reviewer checks that any new
user-facing service follows the same image-based pattern, or supersedes this ADR.

## Pros and Cons of the Options

### Docker image as the deployment unit for both services

* Good, because one deployment model, one CI pattern, one place — "the image" — to reason
  about what ships.
* Good, because it reuses the Container Apps environment and registry the API needs anyway, so
  the web image is marginal extra infrastructure.
* Good, because the full Next.js server stays available, so later features don't force a
  hosting migration.
* Neutral, because the project owns the Dockerfiles and base-image upkeep for both services.
* Bad, because a scale-to-zero container cold-starts where a static CDN host would not.

### Azure Static Web Apps for the web tier

* Good, because $0 on the Free tier, CDN-instant first paint, free per-PR preview
  environments, and built-in Entra ID auth that fits the v2 admin route.
* Bad, because it splits the user-facing services across two hosting and deployment models,
  reopening [ADR-0005](0005-use-mixed-hosting-model.md) for the web tier and adding a second
  pipeline that deploys content outside Terraform.
* Bad, because avoiding the preview hybrid mode means static export — changing `output` and
  moving data fetching client-side or to a rebuild-on-sync step — while the hybrid mode that
  keeps App Router server features is still preview, with a 250 MB cap and no linked backend
  to the Container Apps API.

### Source-based deployment to Container Apps / App Service

* Good, because there is no Dockerfile to author or maintain; the platform builds from source.
* Bad, because the build runs in the platform's environment rather than one the repo controls
  and can reproduce identically in CI and locally; harder to pin, and in tension with
  [ADR-0003](0003-do-not-adopt-docker-hardened-images.md)'s base-image control.
* Bad, because it still does not unify with the API, which is already image-based.

## More Information

See [`docs/roadmap.md`](../../roadmap.md) (*"Docker as the deployment unit for API/Web"*) and
[ADR-0005](0005-use-mixed-hosting-model.md), which this ADR builds on by fixing the artifact
type for the two Container Apps services. Base-image choices are governed by
[ADR-0003](0003-do-not-adopt-docker-hardened-images.md). Azure Static Web Apps Next.js support
and its hybrid-mode preview limits:
<https://learn.microsoft.com/en-us/azure/static-web-apps/nextjs> and
<https://learn.microsoft.com/en-us/azure/static-web-apps/deploy-nextjs-hybrid>. Revisit if
`devtrail-web` becomes genuinely static with no prospect of server-side features, or if
cold-start on Container Apps proves unacceptable and a warm replica's cost is not justified —
at which point a static host for the web tier could be reconsidered under a new ADR.
