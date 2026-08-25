# DevTrail
A self-updating portfolio dashboard that reads your GitHub repositories and shows them off through a live connection with your GitHub account.

## What it is
DevTrail is a public dashboard that displays your GitHub projects — description, primary language(s), last commit activity, and (for repos like `code-challenges`) a breakdown of solved challenges per language — by making use of the GitHub API and a Next.js front-end. DevTrail exists to serve as an interactive display of a built up developer profile.

## How it works
1. **Nightly sync** — a timer-triggered Azure Function reads a GitHub Personal Access Token from Key Vault and pulls repo data via Octokit.NET. It's the only component that talks to GitHub, which keeps usage safely within the free rate limits.
2. **Cached storage** — the synced results (description, language breakdown, activity, per-language counts) are written to Azure Table Storage/Cosmos DB, so pageviews never trigger a live GitHub call.
3. **API layer** — `devtrail-api`, an ASP.NET Core Minimal API, serves the cached data to the front-end.
4. **Front-end** — `devtrail-web` renders the dashboard for visitors, publicly accessible with no login required.
5. Everything is provisioned with Terraform and deployed through GitHub Actions to Azure, with Application Insights for monitoring.

## Architecture
Key architectural decisions for DevTrail's stack are recorded in [docs/architecture-decisions.md](docs/architecture-decisions.md).

## Prerequisites
WIP

## Install
WIP

## Common commands
WIP

## Troubleshooting
WIP