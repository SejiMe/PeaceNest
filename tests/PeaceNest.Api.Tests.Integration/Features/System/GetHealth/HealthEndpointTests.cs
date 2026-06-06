using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using PeaceNest.Api.Features.System.GetHealth;

namespace PeaceNest.Api.Tests.Integration.Features.System.GetHealth;

public sealed class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
        });
    }

    [Fact]
    public async Task GetHealth_ReturnsHealthyPayload()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/health");
        var payload = await response.Content.ReadFromJsonAsync<Response>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(Response.HealthyStatus, payload.Status);
        Assert.Equal(Response.ServiceName, payload.Service);
        Assert.True(payload.CheckedAtUtc <= DateTimeOffset.UtcNow);
    }
}
