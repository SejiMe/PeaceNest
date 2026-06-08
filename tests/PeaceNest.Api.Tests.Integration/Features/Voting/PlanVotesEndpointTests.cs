using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Tests.Integration.Support;
using CastPlanVoteRequest = PeaceNest.Api.Features.Voting.CastPlanVote.Request;
using CastPlanVoteResponse = PeaceNest.Api.Features.Voting.CastPlanVote.Response;
using CreateFamilyRequest = PeaceNest.Api.Features.Families.CreateFamily.Request;
using CreateFamilyResponse = PeaceNest.Api.Features.Families.CreateFamily.Response;
using CreateWantOrNeedRequest = PeaceNest.Api.Features.WantsAndNeeds.CreateWantOrNeed.Request;
using CreateWantOrNeedResponse = PeaceNest.Api.Features.WantsAndNeeds.CreateWantOrNeed.Response;
using ListPlanVotesResponse = PeaceNest.Api.Features.Voting.ListPlanVotes.Response;

namespace PeaceNest.Api.Tests.Integration.Features.Voting;

public sealed class PlanVotesEndpointTests
{
    [Fact]
    public async Task CastPlanVote_WritesVoteForFamilyPlan()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var client = CreateAuthenticatedClient(
            factory,
            "aaaaaaaa-5555-5555-5555-aaaaaaaaaaaa",
            "parent@example.test");
        var family = await CreateFamilyAsync(client, "Voting Nest");
        var plan = await CreateWantOrNeedAsync(client, family.Id, "School supplies");

        using var response = await client.PutAsJsonAsync(
            $"/families/{family.Id}/plans/{plan.WantOrNeed.Id}/vote",
            new CastPlanVoteRequest(VoteValue.Support, 4, "This should be a now plan."));
        var payload = await response.Content.ReadFromJsonAsync<CastPlanVoteResponse>();

        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            await response.Content.ReadAsStringAsync());
        Assert.NotNull(payload);
        Assert.Equal(plan.WantOrNeed.Id, payload.Vote.PlanId);
        Assert.Equal(VoteValue.Support, payload.Vote.VoteValue);
        Assert.Equal(4, payload.Vote.PriorityPoints);
        Assert.Equal("This should be a now plan.", payload.Vote.Note);
        Assert.Equal("parent", payload.Vote.UserDisplayName);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var vote = await dbContext.PlanVotes.SingleAsync();

        Assert.Equal(payload.Vote.Id, vote.Id);
        Assert.Equal(4, vote.PriorityPoints);
    }

    [Fact]
    public async Task CastPlanVote_UpdatesExistingVoteForUser()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var client = CreateAuthenticatedClient(
            factory,
            "bbbbbbbb-5555-5555-5555-bbbbbbbbbbbb",
            "parent@example.test");
        var family = await CreateFamilyAsync(client, "Update Vote Nest");
        var plan = await CreateWantOrNeedAsync(client, family.Id, "Family repair");
        await CastVoteAsync(client, family.Id, plan.WantOrNeed.Id, VoteValue.Support, 5, "First thought.");

        var updated = await CastVoteAsync(
            client,
            family.Id,
            plan.WantOrNeed.Id,
            VoteValue.NotNow,
            1,
            "Can wait.");

        Assert.Equal(VoteValue.NotNow, updated.Vote.VoteValue);
        Assert.Equal(1, updated.Vote.PriorityPoints);
        Assert.Equal("Can wait.", updated.Vote.Note);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var vote = await dbContext.PlanVotes.SingleAsync();

        Assert.Equal(updated.Vote.Id, vote.Id);
        Assert.Equal(VoteValue.NotNow, vote.VoteValue);
    }

    [Fact]
    public async Task ListPlanVotes_ReturnsFamilyPlanVoteSummary()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(
            factory,
            "cccccccc-5555-5555-5555-cccccccccccc",
            "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Summary Nest");
        var plan = await CreateWantOrNeedAsync(ownerClient, family.Id, "Family trip");
        var adultUser = await AddMemberAsync(
            factory,
            family.Id,
            "cccccccc-6666-6666-6666-cccccccccccc",
            "adult@example.test",
            FamilyMemberRole.AdultMember);
        var childUser = await AddMemberAsync(
            factory,
            family.Id,
            "cccccccc-7777-7777-7777-cccccccccccc",
            "child@example.test",
            FamilyMemberRole.ChildMember);
        using var adultClient = CreateAuthenticatedClient(
            factory,
            adultUser.SupabaseUserId.ToString(),
            adultUser.Email);
        using var childClient = CreateAuthenticatedClient(
            factory,
            childUser.SupabaseUserId.ToString(),
            childUser.Email);
        await CastVoteAsync(ownerClient, family.Id, plan.WantOrNeed.Id, VoteValue.Support, 4, null);
        await CastVoteAsync(adultClient, family.Id, plan.WantOrNeed.Id, VoteValue.Neutral, 2, null);
        await CastVoteAsync(childClient, family.Id, plan.WantOrNeed.Id, VoteValue.NotNow, 1, null);

        using var response = await ownerClient.GetAsync($"/families/{family.Id}/plans/{plan.WantOrNeed.Id}/votes");
        var payload = await response.Content.ReadFromJsonAsync<ListPlanVotesResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(plan.WantOrNeed.Id, payload.VoteSummary.PlanId);
        Assert.Equal(3, payload.VoteSummary.TotalVotes);
        Assert.Equal(1, payload.VoteSummary.SupportCount);
        Assert.Equal(1, payload.VoteSummary.NeutralCount);
        Assert.Equal(1, payload.VoteSummary.NotNowCount);
        Assert.Equal(7, payload.VoteSummary.TotalPriorityPoints);
        Assert.Equal(3, payload.VoteSummary.Votes.Count);
    }

    [Fact]
    public async Task CastPlanVote_RejectsInvalidPriorityPoints()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var client = CreateAuthenticatedClient(
            factory,
            "dddddddd-5555-5555-5555-dddddddddddd",
            "parent@example.test");
        var family = await CreateFamilyAsync(client, "Validation Nest");
        var plan = await CreateWantOrNeedAsync(client, family.Id, "Emergency kit");

        using var response = await client.PutAsJsonAsync(
            $"/families/{family.Id}/plans/{plan.WantOrNeed.Id}/vote",
            new CastPlanVoteRequest(VoteValue.Support, 6, null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            400,
            ErrorCodes.ValidationFailed);
    }

    [Fact]
    public async Task CastPlanVote_RejectsViewerRole()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(
            factory,
            "eeeeeeee-5555-5555-5555-eeeeeeeeeeee",
            "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Viewer Nest");
        var plan = await CreateWantOrNeedAsync(ownerClient, family.Id, "Family supplies");
        var viewerUser = await AddMemberAsync(
            factory,
            family.Id,
            "eeeeeeee-6666-6666-6666-eeeeeeeeeeee",
            "viewer@example.test",
            FamilyMemberRole.Viewer);
        using var viewerClient = CreateAuthenticatedClient(
            factory,
            viewerUser.SupabaseUserId.ToString(),
            viewerUser.Email);

        using var response = await viewerClient.PutAsJsonAsync(
            $"/families/{family.Id}/plans/{plan.WantOrNeed.Id}/vote",
            new CastPlanVoteRequest(VoteValue.Support, 3, null));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            403,
            ErrorCodes.AuthorizationDenied);
    }

    [Fact]
    public async Task ListPlanVotes_RejectsOutsider()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(
            factory,
            "ffffffff-5555-5555-5555-ffffffffffff",
            "owner@example.test");
        using var outsiderClient = CreateAuthenticatedClient(
            factory,
            "ffffffff-6666-6666-6666-ffffffffffff",
            "outsider@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Private Vote Nest");
        var plan = await CreateWantOrNeedAsync(ownerClient, family.Id, "Private plan");

        using var response = await outsiderClient.GetAsync($"/families/{family.Id}/plans/{plan.WantOrNeed.Id}/votes");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            403,
            ErrorCodes.AuthorizationDenied);
    }

    private static async Task<CreateFamilyResponse> CreateFamilyAsync(HttpClient client, string name)
    {
        using var response = await client.PostAsJsonAsync("/families", new CreateFamilyRequest(name, null));
        var payload = await response.Content.ReadFromJsonAsync<CreateFamilyResponse>();

        Assert.True(
            response.StatusCode == HttpStatusCode.Created,
            await response.Content.ReadAsStringAsync());
        Assert.NotNull(payload);
        return payload;
    }

    private static async Task<CreateWantOrNeedResponse> CreateWantOrNeedAsync(
        HttpClient client,
        Guid familyId,
        string title)
    {
        using var response = await client.PostAsJsonAsync(
            $"/families/{familyId}/wants-needs",
            new CreateWantOrNeedRequest(
                WantNeedKind.Need,
                title,
                "A calm planning item.",
                PriorityRank: null,
                ProgressPercent: 0,
                EstimatedCostAmount: null,
                EstimatedCostCurrency: null,
                ScoreLevel.Medium,
                ScoreLevel.Medium,
                ScoreLevel.Medium,
                DesiredByDate: null,
                TargetDate: null));
        var payload = await response.Content.ReadFromJsonAsync<CreateWantOrNeedResponse>();

        Assert.True(
            response.StatusCode == HttpStatusCode.Created,
            await response.Content.ReadAsStringAsync());
        Assert.NotNull(payload);
        return payload;
    }

    private static async Task<CastPlanVoteResponse> CastVoteAsync(
        HttpClient client,
        Guid familyId,
        Guid planId,
        VoteValue voteValue,
        int priorityPoints,
        string? note)
    {
        using var response = await client.PutAsJsonAsync(
            $"/families/{familyId}/plans/{planId}/vote",
            new CastPlanVoteRequest(voteValue, priorityPoints, note));
        var payload = await response.Content.ReadFromJsonAsync<CastPlanVoteResponse>();

        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            await response.Content.ReadAsStringAsync());
        Assert.NotNull(payload);
        return payload;
    }

    private static async Task<User> AddMemberAsync(
        TestingApiFactory factory,
        Guid familyId,
        string supabaseUserId,
        string email,
        FamilyMemberRole role)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var user = new User
        {
            Id = Guid.NewGuid(),
            SupabaseUserId = Guid.Parse(supabaseUserId),
            Email = email,
            DisplayName = email.Split('@')[0]
        };

        dbContext.Users.Add(user);
        dbContext.FamilyMembers.Add(new FamilyMember
        {
            Id = Guid.NewGuid(),
            FamilyId = familyId,
            UserId = user.Id,
            Role = role,
            Status = FamilyMemberStatus.Active,
            JoinedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        return user;
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
