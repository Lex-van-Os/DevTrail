using Azure.Data.Tables;
using Azure.Monitor.OpenTelemetry.Exporter;

using devtrail_sync.Mappers;
using devtrail_sync.Services;

using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Octokit;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Services.AddSingleton(_ =>
    new TableServiceClient(builder.Configuration.GetConnectionString("TableStorage")));

builder.Services.AddSingleton(_ => new GitHubClient(new ProductHeaderValue("DevTrailSync"))
{
    Credentials = new Credentials(builder.Configuration["Github:PAT"])
});
builder.Services.AddScoped<IGitHubSyncService, GitHubSyncService>();
builder.Services.AddScoped<ITableStorageService, TableStorageService>();
builder.Services.AddScoped<IRepositoryMapper, RepositoryMapper>();
builder.Services.AddScoped<ISolvedChallengeMapper, SolvedChallengeMapper>();

builder.Build().Run();