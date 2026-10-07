# Configure the Azure provider
terraform {
  required_version = ">= 1.1.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 4.0.4"
    }
  }
  backend "azurerm" {
    resource_group_name  = "rg-devtrail-tfstate"
    storage_account_name = "stdevtrailtfstate"
    container_name       = "tfstate"
    key                  = "devtrail.tfstate"
  }
}

provider "azurerm" {
  features {}
}

data "azurerm_subscription" "current-subscription" {}

resource "azurerm_resource_group" "devtrail-sync-func-dev_group" {
  name     = "devtrail-sync-func-dev_group"
  location = "North Europe"
}

resource "azurerm_storage_account" "devtrail-sync-func-storage-dev" {
  name                     = "devtrailsyncfuncdev"
  resource_group_name      = azurerm_resource_group.devtrail-sync-func-dev_group.name
  location                 = azurerm_resource_group.devtrail-sync-func-dev_group.location
  account_tier             = "Standard"
  account_replication_type = "LRS"

  tags = {
    environment = "development"
  }
}

resource "azurerm_storage_table" "repositories" {
  name               = "Repositories"
  storage_account_id = azurerm_storage_account.devtrail-sync-func-storage-dev.id
}

resource "azurerm_storage_table" "solvedChallenges" {
  name               = "SolvedChallenges"
  storage_account_id = azurerm_storage_account.devtrail-sync-func-storage-dev.id
}

# resource "azurerm_container_registry" "devtrail-container-registry-dev" {
#   name                = "devtrailregistrydev"
#   resource_group_name = azurerm_resource_group.devtrail-sync-func-dev_group.name
#   location            = azurerm_resource_group.devtrail-sync-func-dev_group.location
#   sku                 = "Basic"
#   admin_enabled       = false
# }

# resource "azurerm_container_app_environment" "devtrail-container-environment-dev" {
#   name                       = "devtrail-container-environment-dev"
#   location                   = azurerm_resource_group.devtrail-sync-func-dev_group.location
#   resource_group_name        = azurerm_resource_group.devtrail-sync-func-dev_group.name
# }

# resource "azurerm_container_app" "devtrail-web-dev" {
#   name                         = "devtrail-web-dev"
#   container_app_environment_id = azurerm_container_app_environment.devtrail-container-environment-dev.id
#   resource_group_name          = azurerm_resource_group.devtrail-sync-func-dev_group.name
#   revision_mode                = "Single"

#   template {
#     container {
#       name   = "devtrail-web-dev"
#       image  = "mcr.microsoft.com/k8se/quickstart:latest"
#       cpu    = 0.25
#       memory = "0.5Gi"
#     }
#   }
# }

# resource "azurerm_container_app" "devtrail-api-dev" {
#   name                         = "devtrail-api-dev"
#   container_app_environment_id = azurerm_container_app_environment.devtrail-container-environment-dev.id
#   resource_group_name          = azurerm_resource_group.devtrail-sync-func-dev_group.name
#   revision_mode                = "Single"

#   template {
#     container {
#       name   = "devtrail-api-dev"
#       image  = "mcr.microsoft.com/k8se/quickstart:latest"
#       cpu    = 0.25
#       memory = "0.5Gi"
#     }
#   }
# }

# data "azurerm_client_config" "devtrail-config" {}

# resource "azurerm_key_vault" "devtrail-key_vault" {
#   name                        = "examplekeyvault"
#   location                    = azurerm_resource_group.devtrail-sync-func-dev_group.location
#   resource_group_name         = azurerm_resource_group.devtrail-sync-func-dev_group.name
#   rbac_authorization_enabled  = false
#   enabled_for_disk_encryption = true
#   tenant_id                   = data.azurerm_client_config.devtrail-config.tenant_id
#   soft_delete_retention_days  = 7
#   purge_protection_enabled    = false

#   sku_name = "standard"

#   access_policy {
#     tenant_id = data.azurerm_client_config.devtrail-config.tenant_id
#     object_id = data.azurerm_client_config.devtrail-config.object_id

#     key_permissions = [
#       "Get",
#     ]

#     secret_permissions = [
#       "Get",
#     ]

#     storage_permissions = [
#       "Get",
#     ]
#   }
# }

# resource "azurerm_application_insights" "devtrail-appinsights" {
#   name                = "devtrail-appinsights"
#   location            = azurerm_resource_group.devtrail-sync-func-dev_group.location
#   resource_group_name = azurerm_resource_group.devtrail-sync-func-dev_group.name
#   application_type    = "web"
# }

# output "instrumentation_key" {
#   value = azurerm_application_insights.devtrail-appinsights.instrumentation_key
# }

# output "app_id" {
#   value = azurerm_application_insights.devtrail-appinsights.app_id
# }

# resource "azurerm_monitor_action_group" "devtrail-monitor_group" {
#   name                = "devtrail-monitor_group"
#   resource_group_name = azurerm_resource_group.devtrail-sync-func-dev_group.name
#   short_name          = "devtrail"
# }

# resource "azurerm_consumption_budget_subscription" "devtrail-budget-plan" {
#   name            = "devtrail-budget-plans"
#   subscription_id = data.azurerm_subscription.current-subscription.id

#   amount     = 10
#   time_grain = "Monthly"

#   time_period {
#     start_date = "2026-08-01T00:00:00Z"
#     end_date   = "2030-07-31T00:00:00Z"
#   }

#   notification {
#     enabled        = true
#     threshold      = 100.0
#     operator       = "GreaterThan"
#     threshold_type = "Actual"

#     contact_emails = [
#       var.budget_email,
#     ]
#   }

#   notification {
#     enabled        = true
#     threshold      = 10.0
#     operator       = "GreaterThan"
#     threshold_type = "Forecasted"

#     contact_emails = [
#       var.budget_email,
#     ]
#   }

#   notification {
#     enabled        = true
#     threshold      = 80.0
#     operator       = "GreaterThan"
#     threshold_type = "Forecasted"

#     contact_emails = [
#       var.budget_email,
#     ]
#   }
# }