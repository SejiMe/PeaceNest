using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Features.Auth.GetMe;
using PeaceNest.Api.Tests.Integration.Support;

namespace PeaceNest.Api.Tests.Integration.Features.Auth;

public sealed class GetMeEndpointTests
{
    [Fact]
    public async Task GetMe_MirrorsGoogleUserOnFirstAuthenticatedRequest()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestJwtTokenFactory.CreateSupabaseAccessToken(
                subject: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
                email: "parent@example.test"));

        using var response = await client.GetAsync("/auth/me");
        var payload = await response.Content.ReadFromJsonAsync<Response>();

        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            await response.Content.ReadAsStringAsync());
        Assert.NotNull(payload);
        Assert.Equal(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), payload.User.SupabaseUserId);
        Assert.Equal("parent@example.test", payload.User.Email);
        Assert.Equal("parent", payload.User.DisplayName);
        Assert.Empty(payload.FamilyMemberships);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var mirroredUser = await dbContext.Users.SingleAsync();

        Assert.Equal(payload.User.Id, mirroredUser.Id);
        Assert.NotNull(mirroredUser.LastSeenAt);
    }

    [Fact]
    public async Task GetMe_RejectsNonGoogleProvider()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestJwtTokenFactory.CreateSupabaseAccessToken(provider: "email"));

        using var response = await client.GetAsync("/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            401,
            ErrorCodes.AuthenticationRequired);
    }
}
