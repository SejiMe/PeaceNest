using System.Net;
using System.Net.Http.Json;
using PeaceNest.Api.Features.System.GetHealth;
using PeaceNest.Api.Tests.Integration.Support;

namespace PeaceNest.Api.Tests.Integration.Features.System.GetHealth;

public sealed class HealthEndpointTests : IClassFixture<TestingApiFactory>
{
    private readonly TestingApiFactory _factory;

    public HealthEndpointTests(TestingApiFactory factory)
    {
        _factory = factory;
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
