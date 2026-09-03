# DevTrail Web

## What it is
`devtrail-web` is DevTrail's front-end: a Next.js (App Router) app. Per the project's [roadmap](../docs/roadmap.md), it's intended to render the public portfolio dashboard.

## How it works
- Built with the Next.js **App Router**, on Next.js 16, React 19, and TypeScript.
- Styled with Tailwind CSS.
- Currently contains two pages:
  - `/` — the default `create-next-app` starter page, with a link to `/welcome`.
  - `/welcome` — a placeholder page.
- Every PR runs `pnpm lint` and `pnpm build` via [GitHub Actions](../.github/workflows/ci.yml).

## Architecture
See the root [README](../README.md#architecture) and [`docs/roadmap.md`](../docs/roadmap.md) for the wider system's architecture; formal ADRs are indexed in [`docs/architecture/architecture-decisions.md`](../docs/architecture/architecture-decisions.md), with the records under [`docs/architecture/adr/`](../docs/architecture/adr/). This project is planned to be deployed as a Docker image to Azure Container Apps ([ADR-0007](../docs/architecture/adr/0007-docker-as-deployment-unit-for-api-and-web.md)), built on Docker Official / vendor images rather than Docker Hardened Images ([ADR-0003](../docs/architecture/adr/0003-do-not-adopt-docker-hardened-images.md)).

## Prerequisites
- Node.js
- pnpm (pinned to `pnpm@11.24.0` via `packageManager` in `package.json`)

## Install
```bash
pnpm install
```

## Common commands
```bash
pnpm dev      # run the dev server (http://localhost:3000)
pnpm build    # production build
pnpm start    # run the production build
pnpm lint     # run ESLint
docker compose up devtrail-web devtrail-api azurite   # run in Docker instead (from repo root)
```

## Troubleshooting
- This Next.js version has breaking changes from what most AI coding assistants were trained on — check `node_modules/next/dist/docs/` before making routing or API changes (see `AGENTS.md`).
