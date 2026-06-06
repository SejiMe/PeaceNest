using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Features.Auth.GetProtectedSmoke;
using PeaceNest.Api.Tests.Integration.Support;

namespace PeaceNest.Api.Tests.Integration.Common.Auth;

public sealed class ProtectedEndpointTests : IClassFixture<TestingApiFactory>
{
    private readonly TestingApiFactory _factory;

    public ProtectedEndpointTests(TestingApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ProtectedEndpoint_RejectsMissingBearerToken()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/auth/protected-smoke");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            401,
            ErrorCodes.AuthenticationRequired);
    }

    [Fact]
    public async Task ProtectedEndpoint_RejectsInvalidBearerToken()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-valid-token");

        using var response = await client.GetAsync("/auth/protected-smoke");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            401,
            ErrorCodes.AuthenticationRequired);
    }

    [Fact]
    public async Task ProtectedEndpoint_AcceptsValidSupabaseBearerToken()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestJwtTokenFactory.CreateSupabaseAccessToken());

        using var response = await client.GetAsync("/auth/protected-smoke");
        var payload = await response.Content.ReadFromJsonAsync<Response>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("123e4567-e89b-12d3-a456-426614174000", payload.Subject);
        Assert.Equal("parent@example.test", payload.Email);
        Assert.Equal("authenticated", payload.Role);
    }
}
