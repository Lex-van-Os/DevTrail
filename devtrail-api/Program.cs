using Azure.Data.Tables;

using devtrail_api.Mappers;

using devtrail_core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddSingleton(_ =>
    new TableServiceClient(builder.Configuration.GetConnectionString("TableStorage")));
builder.Services.AddScoped<ITableStorageService, TableStorageService>();
builder.Services.AddScoped<IRepositoryStatisticsMapper, RepositoryStatisticsMapper>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.MapGet("/health", async (TableServiceClient tableServiceClient) =>
    {
        var table = tableServiceClient.GetTableClient("healthcheck");
        await table.CreateIfNotExistsAsync();

        var entity = new TableEntity("health", Guid.NewGuid().ToString())
        {
            ["checkedAt"] = DateTimeOffset.UtcNow
        };
        await table.AddEntityAsync(entity);
        await table.GetEntityAsync<TableEntity>(entity.PartitionKey, entity.RowKey);

        return Results.Ok();
    })
    .WithName("GetHealth");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public partial class Program { }