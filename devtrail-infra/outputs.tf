output "instrumentation_key" {
  value     = azurerm_application_insights.devtrail-appinsights.instrumentation_key
  sensitive = true
}

output "app_id" {
  value = azurerm_application_insights.devtrail-appinsights.app_id
}
