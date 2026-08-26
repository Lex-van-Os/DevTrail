using Microsoft.AspNetCore.Mvc.Testing;

namespace devtrail_api.Tests;

public class WeatherForecastEndpointTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetWeatherForecast_ReturnsSuccess()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/weatherforecast");

        Assert.True(response.IsSuccessStatusCode);
    }
}

public class HealthEndpointTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetHealth_ReturnsSuccess()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("health");

        Assert.True(response.IsSuccessStatusCode);
    }
}