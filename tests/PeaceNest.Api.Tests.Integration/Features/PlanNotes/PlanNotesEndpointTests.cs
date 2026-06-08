using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Tests.Integration.Support;
using AddPlanNoteRequest = PeaceNest.Api.Features.PlanNotes.AddPlanNote.Request;
using AddPlanNoteResponse = PeaceNest.Api.Features.PlanNotes.AddPlanNote.Response;
using CreateFamilyRequest = PeaceNest.Api.Features.Families.CreateFamily.Request;
using CreateFamilyResponse = PeaceNest.Api.Features.Families.CreateFamily.Response;
using CreateWantOrNeedRequest = PeaceNest.Api.Features.WantsAndNeeds.CreateWantOrNeed.Request;
using CreateWantOrNeedResponse = PeaceNest.Api.Features.WantsAndNeeds.CreateWantOrNeed.Response;
using ListPlanNotesResponse = PeaceNest.Api.Features.PlanNotes.ListPlanNotes.Response;

namespace PeaceNest.Api.Tests.Integration.Features.PlanNotes;

public sealed class PlanNotesEndpointTests
{
    [Fact]
    public async Task AddPlanNote_WritesPlanLevelNote()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var client = CreateAuthenticatedClient(
            factory,
            "aaaaaaaa-3333-3333-3333-aaaaaaaaaaaa",
            "parent@example.test");
        var family = await CreateFamilyAsync(client, "Notes Nest");
        var plan = await CreateWantOrNeedAsync(client, family.Id, "School supplies");

        using var response = await client.PostAsJsonAsync(
            $"/families/{family.Id}/plans/{plan.WantOrNeed.Id}/notes",
            new AddPlanNoteRequest("Let's compare prices this weekend."));
        var payload = await response.Content.ReadFromJsonAsync<AddPlanNoteResponse>();

        Assert.True(
            response.StatusCode == HttpStatusCode.Created,
            await response.Content.ReadAsStringAsync());
        Assert.NotNull(payload);
        Assert.Equal(plan.WantOrNeed.Id, payload.Note.PlanId);
        Assert.Equal("Let's compare prices this weekend.", payload.Note.Body);
        Assert.Equal("parent", payload.Note.AuthorDisplayName);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var note = await dbContext.Comments.SingleAsync();

        Assert.Equal(payload.Note.Id, note.Id);
        Assert.Null(note.ParentCommentId);
    }

    [Fact]
    public async Task ListPlanNotes_ReturnsOnlyNotesForRequestedFamilyPlan()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var firstClient = CreateAuthenticatedClient(
            factory,
            "bbbbbbbb-3333-3333-3333-bbbbbbbbbbbb",
            "first@example.test");
        using var secondClient = CreateAuthenticatedClient(
            factory,
            "bbbbbbbb-4444-4444-4444-bbbbbbbbbbbb",
            "second@example.test");
        var firstFamily = await CreateFamilyAsync(firstClient, "First Nest");
        var secondFamily = await CreateFamilyAsync(secondClient, "Second Nest");
        var firstPlan = await CreateWantOrNeedAsync(firstClient, firstFamily.Id, "Emergency kit");
        var secondPlan = await CreateWantOrNeedAsync(secondClient, secondFamily.Id, "Private wish");
        await AddNoteAsync(firstClient, firstFamily.Id, firstPlan.WantOrNeed.Id, "Shared with my family.");
        await AddNoteAsync(secondClient, secondFamily.Id, secondPlan.WantOrNeed.Id, "Private family note.");

        using var response = await firstClient.GetAsync($"/families/{firstFamily.Id}/plans/{firstPlan.WantOrNeed.Id}/notes");
        var payload = await response.Content.ReadFromJsonAsync<ListPlanNotesResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        var note = Assert.Single(payload.Notes);
        Assert.Equal("Shared with my family.", note.Body);
    }

    [Fact]
    public async Task AddPlanNote_RejectsBlankBody()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var client = CreateAuthenticatedClient(
            factory,
            "cccccccc-3333-3333-3333-cccccccccccc",
            "parent@example.test");
        var family = await CreateFamilyAsync(client, "Validation Nest");
        var plan = await CreateWantOrNeedAsync(client, family.Id, "Family trip");

        using var response = await client.PostAsJsonAsync(
            $"/families/{family.Id}/plans/{plan.WantOrNeed.Id}/notes",
            new AddPlanNoteRequest(" "));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            400,
            ErrorCodes.ValidationFailed);
    }

    [Fact]
    public async Task AddPlanNote_RejectsViewerRole()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(
            factory,
            "dddddddd-3333-3333-3333-dddddddddddd",
            "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Permission Nest");
        var plan = await CreateWantOrNeedAsync(ownerClient, family.Id, "Family repair");
        var viewerUser = await AddMemberAsync(
            factory,
            family.Id,
            "dddddddd-4444-4444-4444-dddddddddddd",
            "viewer@example.test",
            FamilyMemberRole.Viewer);
        using var viewerClient = CreateAuthenticatedClient(
            factory,
            viewerUser.SupabaseUserId.ToString(),
            viewerUser.Email);

        using var response = await viewerClient.PostAsJsonAsync(
            $"/families/{family.Id}/plans/{plan.WantOrNeed.Id}/notes",
            new AddPlanNoteRequest("I can see but not add."));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            403,
            ErrorCodes.AuthorizationDenied);
    }

    [Fact]
    public async Task ListPlanNotes_RejectsOutsider()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(
            factory,
            "eeeeeeee-3333-3333-3333-eeeeeeeeeeee",
            "owner@example.test");
        using var outsiderClient = CreateAuthenticatedClient(
            factory,
            "eeeeeeee-4444-4444-4444-eeeeeeeeeeee",
            "outsider@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Private Nest");
        var plan = await CreateWantOrNeedAsync(ownerClient, family.Id, "Private plan");

        using var response = await outsiderClient.GetAsync($"/families/{family.Id}/plans/{plan.WantOrNeed.Id}/notes");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            403,
            ErrorCodes.AuthorizationDenied);
    }

    [Fact]
    public async Task DeletePlanNote_SoftDeletesAuthorNote()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var client = CreateAuthenticatedClient(
            factory,
            "ffffffff-3333-3333-3333-ffffffffffff",
            "parent@example.test");
        var family = await CreateFamilyAsync(client, "Delete Nest");
        var plan = await CreateWantOrNeedAsync(client, family.Id, "Family supplies");
        var note = await AddNoteAsync(client, family.Id, plan.WantOrNeed.Id, "Remove this later.");

        using var response = await client.DeleteAsync(
            $"/families/{family.Id}/plans/{plan.WantOrNeed.Id}/notes/{note.Note.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var deletedNote = await dbContext.Comments
            .IgnoreQueryFilters()
            .SingleAsync(comment => comment.Id == note.Note.Id);

        Assert.NotNull(deletedNote.DeletedAt);
    }

    [Fact]
    public async Task DeletePlanNote_AllowsOwnerToDeleteAnotherMembersNote()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(
            factory,
            "99999999-3333-3333-3333-999999999999",
            "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Moderation Nest");
        var plan = await CreateWantOrNeedAsync(ownerClient, family.Id, "Family plan");
        var adultUser = await AddMemberAsync(
            factory,
            family.Id,
            "99999999-4444-4444-4444-999999999999",
            "adult@example.test",
            FamilyMemberRole.AdultMember);
        using var adultClient = CreateAuthenticatedClient(
            factory,
            adultUser.SupabaseUserId.ToString(),
            adultUser.Email);
        var note = await AddNoteAsync(adultClient, family.Id, plan.WantOrNeed.Id, "Owner may moderate this.");

        using var response = await ownerClient.DeleteAsync(
            $"/families/{family.Id}/plans/{plan.WantOrNeed.Id}/notes/{note.Note.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
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

    private static async Task<AddPlanNoteResponse> AddNoteAsync(
        HttpClient client,
        Guid familyId,
        Guid planId,
        string body)
    {
        using var response = await client.PostAsJsonAsync(
            $"/families/{familyId}/plans/{planId}/notes",
            new AddPlanNoteRequest(body));
        var payload = await response.Content.ReadFromJsonAsync<AddPlanNoteResponse>();

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
