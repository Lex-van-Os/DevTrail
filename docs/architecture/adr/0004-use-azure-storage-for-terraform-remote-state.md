---
status: "accepted"
date: 2026-08-31
decision-makers: [Lex van Os]
---

# Use Azure Storage for Terraform Remote State

## Context and Problem Statement

Terraform needs a remote backend to store its state file, so it isn't kept only on one local machine, isn't lost, and supports locking for safe concurrent use. Two realistic options exist for `devtrail-infra`: **HCP Terraform** (formerly Terraform Cloud), HashiCorp's own hosted state/backend service, or the **`azurerm` backend**, backed by an Azure Storage account. This ADR settles which one DevTrail's remote state uses.

## Decision Drivers

* Project scale — DevTrail is a personal portfolio/hobby project (see [ADR-0003](0003-do-not-adopt-docker-hardened-images.md)), not a team project; HCP Terraform's collaboration features (remote plan/apply, policy-as-code, team access controls) solve problems this project doesn't have.
* Consolidation — every other piece of DevTrail already lives on Azure (Container Apps, Functions, Key Vault, Storage). Adding HCP Terraform introduces a second external account/service, and a second place secrets and access need managing, for no functional gain here.
* Cost — Azure Storage for a single small state file costs fractions of a cent per month, comfortably inside the project's free-tier budget; HCP Terraform's free tier would also work, but at the cost of the consolidation driver above.

## Considered Options

* Azure Storage (`azurerm` backend)
* HCP Terraform (Terraform Cloud)

## Decision Outcome

Chosen option: **Azure Storage (`azurerm` backend)**, because it keeps state inside the same cloud and account boundary as everything else DevTrail provisions, needs no additional external account, and HCP Terraform's actual differentiators (team collaboration, remote execution, policy-as-code) don't apply to a single-maintainer hobby project.

### Consequences

* Good, because there's one fewer external service/account in the project's supply chain — no separate HCP Terraform organization, no separate token to manage or rotate.
* Good, because state lives alongside the rest of DevTrail's Azure footprint, in a resource group (`rg-devtrail-tfstate`) that is deliberately *not* itself Terraform-managed, avoiding the bootstrapping problem of a backend managing its own storage.
* Bad, because the `azurerm` backend has no built-in remote plan/apply, run history UI, or policy-as-code — if DevTrail ever grows a second contributor or needs enforced review gates on `apply`, HCP Terraform's collaboration model would need revisiting.
* Neutral, because migrating between backend types later is possible but not free — confirmed firsthand during setup: switching away from an initial, accidentally-scaffolded HCP Terraform `cloud {}` block required clearing local Terraform metadata and reinitializing, since Terraform has no automated migration path between backend types.

### Confirmation

`devtrail-infra/main.tf`'s `terraform {}` block contains a `backend "azurerm" {}` block, not a `cloud {}` block. A reviewer checks this at PR time; `terraform init` succeeding against the `stdevtrailtfstate` storage account is the practical confirmation.

## Pros and Cons of the Options

### Azure Storage (`azurerm` backend)

* Good, because it's already within the project's existing Azure subscription and budget.
* Good, because it supports state locking and consistency checks natively, via blob leases — the same guarantees HCP Terraform provides.
* Neutral, because it requires manually bootstrapping a resource group, storage account, and container once, outside of Terraform itself.
* Bad, because it has no UI for run history, remote execution, or policy-as-code.

### HCP Terraform (Terraform Cloud)

* Good, because it provides remote plan/apply execution, a run history UI, and policy-as-code (Sentinel/OPA) out of the box.
* Good, because its free tier is sufficient for a solo project.
* Bad, because it's a second external account/service to manage, separate from the rest of DevTrail's Azure-only footprint.
* Bad, because its collaboration/governance features address a team-scale problem this project doesn't have.

## More Information

Terraform backend types: <https://developer.hashicorp.com/terraform/language/backend>. The `azurerm` backend specifically: <https://developer.hashicorp.com/terraform/language/backend/azurerm>. Revisit if DevTrail gains additional contributors or needs enforced apply-time review gates.
