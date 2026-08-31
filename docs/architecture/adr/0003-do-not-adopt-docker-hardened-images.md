---
status: "accepted"
date: 2026-08-29
decision-makers: [Lex van Os]
---

# Do not adopt Docker Hardened Images

## Context and Problem Statement

`devtrail-api` (ASP.NET Core Minimal API) and `devtrail-web` (Next.js) are containerised as the
deployment unit for Azure Container Apps, pulled from Azure Container Registry. Docker's image
guidance presents a choice between a **Docker Hardened Image (DHI)** — a minimal, near-distroless,
non-root image with a CVE-remediation SLA and supply-chain attestations (SBOM, VEX, provenance,
signatures), distributed under a paid subscription — and a **Docker Official Image (DOI)**, the
free curated base images on Docker Hub and vendor registries. This ADR settles which of the two
DevTrail's images are built on.

## Decision Drivers

* Project scale and environment — DevTrail is a personal portfolio / hobby project, not a
  production system holding user data. The exposure of a public, read-only dashboard is low, and
  the one real secret (the GitHub PAT) lives in `devtrail-sync` + Key Vault, not in either
  container image.
* Cost — DHI requires a paid subscription (Docker Business tier / add-on); the project is
  deliberately built around free tiers with a budget alert.
* Developer ergonomics — DHI's shell-less, non-root images constrain interactive debugging
  (needs `docker debug` / ephemeral containers) and low-numbered port binding. Reasonable for a
  hardened production fleet, unnecessary friction here.
* DHI hardens only the base OS layer, not the application dependency tree (npm / NuGet), which is
  where a small app's realistic vulnerability risk sits.

## Considered Options

* Docker Official Images
* Docker Hardened Images

## Decision Outcome

Chosen option: **Docker Official Images**.

* DHI's benefits — a CVE SLA, attestations, a minimal attack surface — address risks that are
  minor for a non-production hobby dashboard, while its costs are immediate and concrete: a paid
  subscription, and constrained debugging and port usage.
* Hardening that does **not** depend on DHI stays available and is the intended direction:
  minimal / distroless-style official bases (e.g. `-alpine`, Microsoft's `-noble-chiseled`),
  running as a non-root user, pinned base tags, and image scanning in CI. That is a separate
  implementation concern, not part of this decision.

### Consequences

* Good — no recurring cost; full shell and standard tooling in the images keep local debugging
  and iteration simple; no subscription or namespace-mirroring dependency added to the supply
  chain.
* Bad — DOI base images carry transitive OS-package CVEs with no remediation SLA; mitigated by
  choosing minimal bases, rebuilding regularly to pick up upstream patches, and scanning images
  in CI.
* Neutral — if DevTrail later gains authenticated write endpoints handling user data, or a
  compliance obligation, this decision should be revisited.

### Confirmation

No Dockerfile in the repository references a Docker Hardened Images repository (a subscribed
`docker.io/<org>/dhi-*` or mirrored `dhi` namespace); base images resolve to Docker Official
Images or vendor official images (`mcr.microsoft.com/dotnet/*`, `node:*`). A reviewer checks this
at PR time.

## More Information

Docker Hardened Images: <https://docs.docker.com/dhi/>. Docker Official Images:
<https://docs.docker.com/docker-hub/repos/manage/trusted-content/official-images/>. This decision
sits alongside the planned "Docker as the deployment unit for the API and web" ADR from
[`docs/roadmap.md`](../../roadmap.md). Revisit if the threat model changes (user data,
authenticated writes) or a compliance requirement appears.
