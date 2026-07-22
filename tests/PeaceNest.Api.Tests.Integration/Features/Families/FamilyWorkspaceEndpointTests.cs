using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Tests.Integration.Support;
using CreateFamilyRequest = PeaceNest.Api.Features.Families.CreateFamily.Request;
using CreateFamilyResponse = PeaceNest.Api.Features.Families.CreateFamily.Response;
using ListFamiliesResponse = PeaceNest.Api.Features.Families.ListFamilies.Response;

namespace PeaceNest.Api.Tests.Integration.Features.Families;

public sealed class FamilyWorkspaceEndpointTests
{
    [Fact]
    public async Task CreateFamily_RequiresCompletedProfile()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestJwtTokenFactory.CreateSupabaseAccessToken(
                subject: "10101010-1010-1010-1010-101010101010",
                email: "new@example.test",
                includeCompletedProfile: false));

        using var response = await client.PostAsJsonAsync(
            "/families",
            new CreateFamilyRequest("New Nest", null, "PHP"));

        Assert.Equal((HttpStatusCode)422, response.StatusCode);
        await ProblemDetailsAssert.HasProblemDetailsAsync(response, 422, ErrorCodes.DomainRuleViolated);
    }

    [Fact]
    public async Task CreateFamily_CreatesWorkspaceAndOwnerMembership()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var client = CreateAuthenticatedClient(
            factory,
            "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
            "owner@example.test");

        using var response = await client.PostAsJsonAsync(
            "/families",
            new CreateFamilyRequest("The Santos Nest", "Family planning with peace."));
        var payload = await response.Content.ReadFromJsonAsync<CreateFamilyResponse>();

        Assert.True(
            response.StatusCode == HttpStatusCode.Created,
            await response.Content.ReadAsStringAsync());
        Assert.NotNull(payload);
        Assert.Equal("The Santos Nest", payload.Name);
        Assert.Equal(FamilyMemberRole.Owner, payload.CurrentUserRole);
        Assert.Equal(1, payload.MemberCount);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var family = await dbContext.Families.SingleAsync();
        var member = await dbContext.FamilyMembers.SingleAsync();

        Assert.Equal(payload.Id, family.Id);
        Assert.Equal(family.Id, member.FamilyId);
        Assert.Equal(FamilyMemberRole.Owner, member.Role);
        Assert.Equal(FamilyMemberStatus.Active, member.Status);
    }

    [Fact]
    public async Task ListFamilies_ReturnsOnlyAuthenticatedUsersFamilyMemberships()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var firstClient = CreateAuthenticatedClient(
            factory,
            "cccccccc-cccc-cccc-cccc-cccccccccccc",
            "first@example.test");
        using var secondClient = CreateAuthenticatedClient(
            factory,
            "dddddddd-dddd-dddd-dddd-dddddddddddd",
            "second@example.test");

        await firstClient.PostAsJsonAsync("/families", new CreateFamilyRequest("First Nest", null));
        await secondClient.PostAsJsonAsync("/families", new CreateFamilyRequest("Second Nest", null));

        using var response = await firstClient.GetAsync("/families");
        var payload = await response.Content.ReadFromJsonAsync<ListFamiliesResponse>();

        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            await response.Content.ReadAsStringAsync());
        Assert.NotNull(payload);
        var family = Assert.Single(payload.Families);
        Assert.Equal("First Nest", family.Name);
        Assert.Equal(FamilyMemberRole.Owner, family.CurrentUserRole);
    }

    [Fact]
    public async Task GetFamily_ReturnsForbiddenWhenUserIsNotMember()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(
            factory,
            "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
            "owner@example.test");
        using var outsiderClient = CreateAuthenticatedClient(
            factory,
            "ffffffff-ffff-ffff-ffff-ffffffffffff",
            "outsider@example.test");

        using var createResponse = await ownerClient.PostAsJsonAsync(
            "/families",
            new CreateFamilyRequest("Private Nest", null));
        var createdFamily = await createResponse.Content.ReadFromJsonAsync<CreateFamilyResponse>();

        using var response = await outsiderClient.GetAsync($"/families/{createdFamily!.Id}");

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden,
            await response.Content.ReadAsStringAsync());
        await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            403,
            ErrorCodes.AuthorizationDenied);
    }

    [Fact]
    public async Task CreateFamily_RejectsBlankName()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var client = CreateAuthenticatedClient(
            factory,
            "11111111-1111-1111-1111-111111111111",
            "owner@example.test");

        using var response = await client.PostAsJsonAsync("/families", new CreateFamilyRequest(" ", null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            400,
            ErrorCodes.ValidationFailed);
    }

    private static HttpClient CreateAuthenticatedClient(
        TestingApiFactory factory,
        string subject,
        string email)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestJwtTokenFactory.CreateSupabaseAccessToken(subject: subject, email: email));

        return client;
    }
}
