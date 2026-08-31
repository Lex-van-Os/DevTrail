# DevTrail Infra

## What it is
`devtrail-infra` is DevTrail's Terraform configuration. Per the project's [roadmap](../docs/roadmap.md), it's intended to provision the MVP's Azure resources.

## How it works
- `azurerm` provider, with remote state stored in Azure Storage rather than HCP Terraform — see [ADR-0004](../docs/architecture/adr/0004-use-azure-storage-for-terraform-remote-state.md).
- The state-holding resource group, storage account, and container (`rg-devtrail-tfstate` / `stdevtrailtfstate` / `tfstate`, North Europe) were created manually via the Azure CLI, not by this Terraform config — Terraform can't manage the backend it depends on to run.
- `main.tf` currently only holds provider and backend configuration. `variables.tf`, `outputs.tf`, and `providers.tf` are placeholders for when real resources are added.

## Architecture
See the root [README](../README.md#architecture) and [`docs/roadmap.md`](../docs/roadmap.md) for the wider system's architecture; formal ADRs are indexed in [`docs/architecture/architecture-decisions.md`](../docs/architecture/architecture-decisions.md), with the records under [`docs/architecture/adr/`](../docs/architecture/adr/). [ADR-0004](../docs/architecture/adr/0004-use-azure-storage-for-terraform-remote-state.md) covers this project's remote state choice specifically.

## Prerequisites
- [Terraform](https://developer.hashicorp.com/terraform/install) (developed against v1.16.0)
- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli), authenticated via `az login` — the `azurerm` provider picks up Azure CLI auth automatically for local runs

## Install
```bash
terraform init
```

## Common commands
```bash
terraform plan     # preview changes
terraform apply     # apply changes
terraform fmt       # format .tf files
```

## Troubleshooting
- `terraform init` failing with a state-migration error (e.g. "Migrating from HCP Terraform...") means stale local `.terraform/` metadata from a previous backend configuration — delete `.terraform/` and re-run `terraform init`.
