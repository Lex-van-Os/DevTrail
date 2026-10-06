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

resource "azurerm_resource_group" "devtrail-sync-func-dev_group" {
  name     = "devtrail-sync-func-dev_group"
  location = "North Europe"
}

resource "azurerm_storage_account" "devtrailsyncfuncdev" {
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
  storage_account_id = azurerm_storage_account.devtrailsyncfuncdev.id
}

resource "azurerm_storage_table" "solvedChallenges" {
  name               = "SolvedChallenges"
  storage_account_id = azurerm_storage_account.devtrailsyncfuncdev.id
}

resource "azurerm_container_app_environment" "example" {
  name                       = "my-environment"
  location                   = azurerm_resource_group.example.location
  resource_group_name        = azurerm_resource_group.example.name
  logs_destination           = "log-analytics"
  log_analytics_workspace_id = azurerm_log_analytics_workspace.example.id
}

// TODO:
// 1. Create Azure resource group for all Azure resources for DevTrail (separate resource group from the Azure Terraform state implementation)
// 2. Define Azure Storage Account resource
// 3. Define Azure Table storage resource for cache data 
// 4. Define Azure Container App Environment resource
// 5. Define Azure Container App Registry resource
// 6. Define Azure Container App resource
// 7. Define Azure Key Vault resource
// 8. Define Azure Application Insights resource
// 9. Define Azure Consumption Budget Description resource