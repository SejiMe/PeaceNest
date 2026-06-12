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
using CreateWantOrNeedRequest = PeaceNest.Api.Features.WantsAndNeeds.CreateWantOrNeed.Request;
using CreateWantOrNeedResponse = PeaceNest.Api.Features.WantsAndNeeds.CreateWantOrNeed.Response;
using GetWantOrNeedResponse = PeaceNest.Api.Features.WantsAndNeeds.GetWantOrNeed.Response;
using ListWantsAndNeedsResponse = PeaceNest.Api.Features.WantsAndNeeds.ListWantsAndNeeds.Response;
using UpdatePlanProgressRequest = PeaceNest.Api.Features.FamilyPlans.UpdatePlanProgress.Request;
using UpdatePlanProgressResponse = PeaceNest.Api.Features.FamilyPlans.UpdatePlanProgress.Response;
using CompletePlanResponse = PeaceNest.Api.Features.FamilyPlans.CompletePlan.Response;
using ArchivePlanResponse = PeaceNest.Api.Features.FamilyPlans.ArchivePlan.Response;

namespace PeaceNest.Api.Tests.Integration.Features.WantsAndNeeds;

public sealed class WantsAndNeedsEndpointTests
{
    [Fact]
    public async Task CreateWantOrNeed_WritesFamilyPlanAndWantNeedDetails()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var client = CreateAuthenticatedClient(
            factory,
            "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
            "parent@example.test");
        var family = await CreateFamilyAsync(client, "Planning Nest");
        var request = NewCreateRequest(
            title: "School supplies",
            kind: WantNeedKind.Need,
            progressPercent: 25);

        using var response = await client.PostAsJsonAsync($"/families/{family.Id}/wants-needs", request);
        var payload = await response.Content.ReadFromJsonAsync<CreateWantOrNeedResponse>();

        Assert.True(
            response.StatusCode == HttpStatusCode.Created,
            await response.Content.ReadAsStringAsync());
        Assert.NotNull(payload);
        Assert.Equal(family.Id, payload.WantOrNeed.FamilyId);
        Assert.Equal(WantNeedKind.Need, payload.WantOrNeed.Kind);
        Assert.Equal("School supplies", payload.WantOrNeed.Title);
        Assert.Equal(25, payload.WantOrNeed.ProgressPercent);
        Assert.Equal(ScoreLevel.High, payload.WantOrNeed.UrgencyLevel);
        Assert.Equal("USD", payload.WantOrNeed.EstimatedCostCurrency);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var plan = await dbContext.FamilyPlans
            .Include(item => item.WantNeedDetails)
            .SingleAsync();

        Assert.Equal(payload.WantOrNeed.Id, plan.Id);
        Assert.Equal(PlanType.WantNeed, plan.PlanType);
        Assert.Equal(PlanStatus.Active, plan.Status);
        Assert.NotNull(plan.WantNeedDetails);
        Assert.Equal(WantNeedKind.Need, plan.WantNeedDetails.Kind);
    }

    [Fact]
    public async Task ListWantsAndNeeds_ReturnsOnlyFamilyScopedWantNeedPlans()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var firstClient = CreateAuthenticatedClient(
            factory,
            "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
            "first@example.test");
        using var secondClient = CreateAuthenticatedClient(
            factory,
            "cccccccc-cccc-cccc-cccc-cccccccccccc",
            "second@example.test");
        var firstFamily = await CreateFamilyAsync(firstClient, "First Nest");
        var secondFamily = await CreateFamilyAsync(secondClient, "Second Nest");
        await CreateWantOrNeedAsync(firstClient, firstFamily.Id, NewCreateRequest("Emergency fund", WantNeedKind.Need));
        await CreateWantOrNeedAsync(secondClient, secondFamily.Id, NewCreateRequest("Family trip", WantNeedKind.Want));

        using var response = await firstClient.GetAsync($"/families/{firstFamily.Id}/wants-needs");
        var payload = await response.Content.ReadFromJsonAsync<ListWantsAndNeedsResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        var item = Assert.Single(payload.WantsAndNeeds);
        Assert.Equal("Emergency fund", item.Title);
        Assert.Equal(firstFamily.Id, item.FamilyId);
    }

    [Fact]
    public async Task GetWantOrNeed_ReturnsCreatedItemForFamilyMember()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var client = CreateAuthenticatedClient(
            factory,
            "dddddddd-dddd-dddd-dddd-dddddddddddd",
            "parent@example.test");
        var family = await CreateFamilyAsync(client, "Detail Nest");
        var created = await CreateWantOrNeedAsync(
            client,
            family.Id,
            NewCreateRequest("New refrigerator", WantNeedKind.Need));

        using var response = await client.GetAsync($"/families/{family.Id}/wants-needs/{created.WantOrNeed.Id}");
        var payload = await response.Content.ReadFromJsonAsync<GetWantOrNeedResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(created.WantOrNeed.Id, payload.WantOrNeed.Id);
        Assert.Equal("New refrigerator", payload.WantOrNeed.Title);
    }

    [Fact]
    public async Task CreateWantOrNeed_RejectsInvalidManualProgress()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var client = CreateAuthenticatedClient(
            factory,
            "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
            "parent@example.test");
        var family = await CreateFamilyAsync(client, "Validation Nest");

        using var response = await client.PostAsJsonAsync(
            $"/families/{family.Id}/wants-needs",
            NewCreateRequest("Too far", WantNeedKind.Want, progressPercent: 101));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            400,
            ErrorCodes.ValidationFailed);
    }

    [Fact]
    public async Task UpdatePlanProgress_UpdatesManualProgressForWantOrNeed()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var client = CreateAuthenticatedClient(
            factory,
            "eeeeeeee-1111-1111-1111-eeeeeeeeeeee",
            "parent@example.test");
        var family = await CreateFamilyAsync(client, "Progress Nest");
        var created = await CreateWantOrNeedAsync(
            client,
            family.Id,
            NewCreateRequest("Family vacation", WantNeedKind.Want));

        using var response = await client.PutAsJsonAsync(
            $"/families/{family.Id}/plans/{created.WantOrNeed.Id}/progress",
            new UpdatePlanProgressRequest(65));
        var payload = await response.Content.ReadFromJsonAsync<UpdatePlanProgressResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(created.WantOrNeed.Id, payload.Plan.PlanId);
        Assert.Equal(65, payload.Plan.ProgressPercent);
        Assert.Equal(PlanStatus.Active, payload.Plan.Status);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var plan = await dbContext.FamilyPlans.SingleAsync(plan => plan.Id == created.WantOrNeed.Id);

        Assert.Equal(65, plan.ProgressPercent);
    }

    [Fact]
    public async Task CompletePlan_MarksWantOrNeedCompleted()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var client = CreateAuthenticatedClient(
            factory,
            "eeeeeeee-2222-2222-2222-eeeeeeeeeeee",
            "parent@example.test");
        var family = await CreateFamilyAsync(client, "Completion Nest");
        var created = await CreateWantOrNeedAsync(
            client,
            family.Id,
            NewCreateRequest("Tuition", WantNeedKind.Need, progressPercent: 40));

        using var response = await client.PutAsync($"/families/{family.Id}/plans/{created.WantOrNeed.Id}/complete", null);
        var payload = await response.Content.ReadFromJsonAsync<CompletePlanResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(PlanStatus.Completed, payload.Plan.Status);
        Assert.Equal(100, payload.Plan.ProgressPercent);
        Assert.NotNull(payload.Plan.CompletedAt);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var plan = await dbContext.FamilyPlans.SingleAsync(plan => plan.Id == created.WantOrNeed.Id);

        Assert.Equal(PlanStatus.Completed, plan.Status);
        Assert.Equal(100, plan.ProgressPercent);
        Assert.NotNull(plan.CompletedAt);
    }

    [Fact]
    public async Task ArchivePlan_RejectsViewerRole()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(
            factory,
            "eeeeeeee-3333-3333-3333-eeeeeeeeeeee",
            "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Archive Permission Nest");
        var created = await CreateWantOrNeedAsync(
            ownerClient,
            family.Id,
            NewCreateRequest("Garden tools", WantNeedKind.Want));
        var viewerUser = await AddMemberAsync(
            factory,
            family.Id,
            "eeeeeeee-4444-4444-4444-eeeeeeeeeeee",
            "viewer@example.test",
            FamilyMemberRole.Viewer);
        using var viewerClient = CreateAuthenticatedClient(
            factory,
            viewerUser.SupabaseUserId.ToString(),
            viewerUser.Email);

        using var response = await viewerClient.PutAsync($"/families/{family.Id}/plans/{created.WantOrNeed.Id}/archive", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            403,
            ErrorCodes.AuthorizationDenied);
    }

    [Fact]
    public async Task ArchivePlan_MarksWantOrNeedArchived()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var client = CreateAuthenticatedClient(
            factory,
            "eeeeeeee-5555-5555-5555-eeeeeeeeeeee",
            "parent@example.test");
        var family = await CreateFamilyAsync(client, "Archive Nest");
        var created = await CreateWantOrNeedAsync(
            client,
            family.Id,
            NewCreateRequest("Someday trip", WantNeedKind.Want));

        using var response = await client.PutAsync($"/families/{family.Id}/plans/{created.WantOrNeed.Id}/archive", null);
        var payload = await response.Content.ReadFromJsonAsync<ArchivePlanResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(PlanStatus.Archived, payload.Plan.Status);
        Assert.NotNull(payload.Plan.ArchivedAt);
    }

    [Fact]
    public async Task CreateWantOrNeed_RejectsViewerRole()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(
            factory,
            "ffffffff-1111-1111-1111-ffffffffffff",
            "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Permission Nest");
        var viewerUser = await AddMemberAsync(
            factory,
            family.Id,
            "ffffffff-2222-2222-2222-ffffffffffff",
            "viewer@example.test",
            FamilyMemberRole.Viewer);
        using var viewerClient = CreateAuthenticatedClient(
            factory,
            viewerUser.SupabaseUserId.ToString(),
            viewerUser.Email);

        using var response = await viewerClient.PostAsJsonAsync(
            $"/families/{family.Id}/wants-needs",
            NewCreateRequest("Hidden wish", WantNeedKind.Want));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            403,
            ErrorCodes.AuthorizationDenied);
    }

    [Fact]
    public async Task ListWantsAndNeeds_RejectsOutsider()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(
            factory,
            "11111111-1111-1111-1111-111111111111",
            "owner@example.test");
        using var outsiderClient = CreateAuthenticatedClient(
            factory,
            "22222222-2222-2222-2222-222222222222",
            "outsider@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Private Nest");

        using var response = await outsiderClient.GetAsync($"/families/{family.Id}/wants-needs");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            403,
            ErrorCodes.AuthorizationDenied);
    }

    private static CreateWantOrNeedRequest NewCreateRequest(
        string title,
        WantNeedKind kind,
        int progressPercent = 0) =>
        new(
            kind,
            title,
            "A calm planning item.",
            PriorityRank: null,
            progressPercent,
            EstimatedCostAmount: 75m,
            EstimatedCostCurrency: "usd",
            ScoreLevel.High,
            ScoreLevel.Medium,
            ScoreLevel.Medium,
            DesiredByDate: null,
            TargetDate: null);

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
        CreateWantOrNeedRequest request)
    {
        using var response = await client.PostAsJsonAsync($"/families/{familyId}/wants-needs", request);
        var payload = await response.Content.ReadFromJsonAsync<CreateWantOrNeedResponse>();

        Assert.True(
            response.StatusCode == HttpStatusCode.Created,
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
