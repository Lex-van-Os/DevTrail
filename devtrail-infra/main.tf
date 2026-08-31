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
