---
status: "accepted"
date: 2026-10-13
decision-makers: [Lex van Os]
---

# Use System-Assigned Managed Identities for Access Between Azure Resources

## Context and Problem Statement

Provisioning the MVP in Azure ([issue #13](https://github.com/Lex-van-Os/DevTrail/issues/13))
means DevTrail's compute has to reach other Azure resources:

* The Function App (`devtrail-sync`) fetches its deployment package from blob storage, keeps
  Functions host state (timer-trigger locks and schedule status), writes cached data to Table
  Storage, and reads the GitHub PAT from Key Vault.
* The API Container App (`devtrail-api`) reads cached data from Table Storage.
* Both Container Apps pull their images from Azure Container Registry.

Today every one of these paths is either unconfigured or authenticated with a shared secret:
storage account keys, a `TableStorage` connection string read in `Program.cs` of both
`devtrail-sync` and `devtrail-api`, and ACR admin credentials. A connection string bundles two
things — *where* the resource is and *proof of access* — and anyone holding it has full access
to the account. Should access between DevTrail's Azure resources keep relying on such secrets,
or should each compute resource authenticate as its own Azure identity?

## Decision Drivers

* No secrets to manage or leak — keys and connection strings in app settings have to be
  supplied, kept out of source control and logs, and rotated; an identity has nothing to hand
  out.
* Access managed in a higher layer than configuration files — who may access what is
  expressed as role assignments on Azure resources, defined in Terraform, instead of being
  implied by whoever holds a key.
* A complete switch rather than a partial one — if one path keeps a connection string, that
  secret still has to be managed, and most of the benefit is lost.
* Clean `terraform destroy` / `apply` cycles (issue #13) — identities and their grants should
  be created and removed with the resources they belong to.
* Local development keeps working against Azurite, which has no identities
  ([ADR-0002](0002-do-not-adopt-dotnet-aspire.md), [ADR-0008](0008-use-azure-table-storage-for-cached-data.md)).

## Considered Options

* System-assigned managed identities for every path.
* User-assigned managed identities for every path.
* Partial switch — identities for platform paths (deployment package, host storage, ACR),
  connection string for application code reaching Table Storage.
* Keys and connection strings for every path, delivered through app settings.

## Decision Outcome

Chosen option: **System-assigned managed identities for every path**, because it is the only
option that removes shared secrets from every access path between DevTrail's Azure resources,
and system-assigned identities share the lifecycle of the app they belong to, which fits the
`destroy` / `apply` cycle at this project's scale without an extra resource to manage.

### How it applies

* **Infrastructure (`devtrail-infra`, issue #13)** — the Function App and both Container Apps
  get a system-assigned identity. Terraform grants each identity only the roles it needs:
  blob access on the deployment storage account (Function App), table access on the cache
  storage account (Function App and API), image pull on ACR (Container Apps), and secret read
  on Key Vault (Function App). Terraform sets the storage endpoint as a non-secret environment
  variable on each app. No account key or connection string is passed to any app.
* **Application code (separate issue)** — `Program.cs` in `devtrail-sync` and `devtrail-api`
  chooses how to build the `TableServiceClient` from which setting is present:
  * a `TableStorage` connection string → connect with it (local development, Azurite);
  * an endpoint setting (working name `StorageEndpointUrl`) → connect with the endpoint and an
    Azure credential from `Azure.Identity` (Azure);
  * neither → fail at startup with a clear configuration error;
  * both → fail at startup with a conflict error.
* **Storage layout** — the Function App's deployment package and host state live in their own
  storage account, separate from the cache-data account. This keeps build artefacts apart from
  the data the API serves, consistent with the separation noted in
  [ADR-0008](0008-use-azure-table-storage-for-cached-data.md).

### Consequences

* Good, because no app receives a storage key, connection string, or registry password, so
  none can leak from app settings, logs, or a copied configuration file.
* Good, because each identity's access is explicit and narrow, and reviewable in Terraform as
  role assignments.
* Good, because system-assigned identities are created and deleted with their app, so
  `terraform destroy` leaves no orphaned identities behind.
* Bad, because the application code needs two ways to build its storage client (connection
  string locally, identity in Azure) and a startup check to choose between them.
* Bad, because role assignments can take a few minutes to take effect, and a system-assigned
  identity's grants can only be created after its app exists — a freshly applied app may fail
  to reach storage briefly.
* Neutral, because storage account keys still exist and are still stored in Terraform state as
  attributes of `azurerm_storage_account`; this decision stops handing them to apps, it does
  not remove them from state.
* Neutral, because the GitHub PAT remains a secret — GitHub does not accept Azure identities.
  The identity only changes how the Function App reads it from Key Vault; how the PAT reaches
  Key Vault is decided separately.
* Neutral, because this ties access management to Azure: on another cloud, identities and
  role assignments would be rewritten using that cloud's equivalent.

### Confirmation

A reviewer checks that:

* `devtrail-infra` passes no storage account key, connection string, or ACR admin credential
  to any app, and ACR has `admin_enabled = false`.
* Every Azure access path listed above is backed by a role assignment (or Key Vault grant) to
  the calling app's system-assigned identity.
* The Function App's deployment storage uses `storage_authentication_type =
  "SystemAssignedIdentity"`.
* `Program.cs` in `devtrail-sync` and `devtrail-api` fails at startup when neither or both of
  the connection-string and endpoint settings are present.

## Pros and Cons of the Options

### System-assigned managed identities for every path

* Good, because no shared secret is used between DevTrail's Azure resources.
* Good, because the identity's lifecycle matches its app — nothing extra to create or clean up.
* Neutral, because each app has its own identity, so shared access must be granted to each one
  separately (three apps in this project).
* Bad, because grants can only be created after the app exists, which can briefly delay access
  after a fresh `apply`.

### User-assigned managed identities for every path

* Good, because the identity and its role assignments can exist before the app starts,
  avoiding the post-`apply` delay.
* Good, because one identity can be shared by several resources.
* Bad, because each identity is a separate resource with its own lifecycle to manage — more
  than this project's scale needs.

### Partial switch — identities for platform paths only

* Good, because the application code stays unchanged.
* Bad, because the `TableStorage` connection string — a full-access key to the cache account —
  still has to be supplied to the Function App and the API, and so still ends up in app
  settings and Terraform state.

### Keys and connection strings for every path

* Good, because it needs no identity or role-assignment configuration and matches local
  development.
* Bad, because every app holds a full-access secret that has to be supplied, protected, and
  rotated.
* Bad, because access is implied by possession of a key rather than expressed per resource.

## More Information

Related: [ADR-0005](0005-use-mixed-hosting-model.md) (Functions for sync, Container Apps for
API and web — the three compute resources that get identities),
[ADR-0007](0007-docker-as-deployment-unit-for-api-and-web.md) (images pulled from ACR), and
[ADR-0008](0008-use-azure-table-storage-for-cached-data.md) (the cache store and its account
separation).

Revisit if several resources need to share one identity, or if the delay before a fresh
system-assigned identity's grants take effect becomes a practical problem in deployments —
at that point user-assigned identities should be re-evaluated under a new ADR.
