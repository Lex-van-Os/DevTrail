# Roadmap

DevTrail is a self-updating portfolio dashboard: a public site that reads directly from a set of GitHub repositories and shows them off — description, languages, activity — as tangible, live proof of a skill set for job interviews. The goal is a live link plus a concrete interview story about the design choices behind it, not just another line on a CV.

This roadmap is phased so v1 is small enough to actually finish, with v2 building on top of it rather than trying to build everything at once. Each bullet below is meant to be issue-sized.

## Tech stack

- **API** — `devtrail-api`: ASP.NET Core Minimal API, Dockerized, deployed to Azure Container Apps.
- **Sync worker** — new `devtrail-sync-func-dev` component, Azure Functions (.NET isolated worker), Timer Trigger, nightly. Reads a GitHub PAT from Key Vault, calls GitHub via Octokit.NET (REST), writes to cached storage.
- **Frontend** — `devtrail-web`: Next.js (App Router), Dockerized, deployed to Azure Container Apps.
- **GitHub integration** — Octokit.NET (REST) for all v1 data; GraphQL against GitHub's API for `contributionsCollection` only, in v2 (REST has no equivalent for that data).
- **Cached storage** — Azure Table Storage or Cosmos DB free tier (exact pick deferred to an ADR).
- **Secrets** — Azure Key Vault (GitHub PAT; later, Entra ID app credentials).
- **IaC** — Terraform, `azurerm` provider: resource group, Container Apps environment + Azure Container Registry, 2 Container Apps (API, Web), Function App + its required storage account, Key Vault, cache storage, budget alert, Application Insights.
- **CI/CD** — GitHub Actions: build/push Docker images to ACR, build/deploy the Function app, `terraform plan` on PR / `terraform apply` on merge to `main`.
- **Auth (v2 only)** — Microsoft Entra ID, OAuth2, restricted to a single admin account.
- **Observability** — Application Insights wired to all three compute components.

The sync worker deliberately stays on Azure Functions rather than moving to a container job alongside the API and frontend: cost-efficient idle behavior fits a once-a-night job, while the user-facing services benefit from always-warm containers. This mixed hosting model is a deliberate choice, not an oversight — worth its own ADR.

## Phase 0 — Foundations & tooling

- Scaffold `devtrail-api` as an ASP.NET Core Minimal API project with a health-check endpoint.
- Scaffold the sync worker as an Azure Functions isolated-worker project with a stub Timer Trigger.
- Scaffold `devtrail-web` as a Next.js (App Router) project.
- Local dev: Docker Compose covering the API + Azurite (Table Storage emulator) + the Next.js dev server.
- Terraform skeleton: provider config + remote state backend (Azure Storage account for tfstate), no application resources yet.
- GitHub Actions CI skeleton: build+test the API, build+test the Next.js app, `terraform fmt`/`validate` on every PR.
- First real ADRs in [architecture/architecture-decisions.md](architecture/architecture-decisions.md): mixed hosting model (Functions for sync, Container Apps for API/Web), REST-first with GraphQL only where needed, Docker as the deployment unit for API/Web.

## Phase 1 — v1 MVP: live portfolio dashboard

- Sync: Function reads the GitHub PAT from Key Vault, pulls description/language breakdown/last-activity for `code-challenges` and `Steward` via Octokit.NET.
- Sync: for `code-challenges`, count solved challenges per language via the GitHub Contents API (subfolder counts under `TypeScript/`, `Python/`).
- Sync: write results to cached storage on the nightly timer.
- API: read-only endpoints in `devtrail-api` serving the cached per-repo data.
- Frontend: `devtrail-web` dashboard page — one card/row per repo showing description, language breakdown, last activity, and per-language challenge counts. No login.
- Terraform: provision the real MVP resources (Container Apps environment + ACR + 2 Container Apps, Function App + storage, Key Vault + PAT secret, cache storage, budget alert, Application Insights).
- CI/CD: GitHub Actions builds+pushes Docker images, deploys the Function app, runs `terraform apply` on merge to `main`.
- Wrap-up: README gets an architecture diagram and a live link; Phase 0/1 ADRs finalized.

## Phase 2a — Contribution calendar (GraphQL)

- Sync: add a GraphQL query against GitHub's GraphQL API for `contributionsCollection`.
- Extend the cached data model and add a new API endpoint for calendar data.
- Frontend: contribution heatmap component in `devtrail-web`.

## Phase 2b — Admin login + custom descriptions

- Register an Entra ID app; wire OAuth2 login restricted to a single admin account.
- Admin-only route in `devtrail-web` (protected) to edit a repo's description.
- RBAC-gated write endpoint on `devtrail-api`; storage gains an optional override field.
- Public dashboard prefers the override over the README-derived description when present.

## Backlog — not scheduled

- **Automatic PR/issue contribution counter**: parked until there's actual activity contributing to external open-source repos to count. Revisit then; not part of v1/v2.
- Exact cache storage choice (Table Storage vs. Cosmos DB) and detailed RBAC/DTO design: deferred to a full architecture document.
