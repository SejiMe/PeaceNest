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
using CreateMilestoneRequest = PeaceNest.Api.Features.FamilyMilestones.CreateMilestone.Request;
using CreateMilestoneResponse = PeaceNest.Api.Features.FamilyMilestones.CreateMilestone.Response;
using CreateMilestoneStepRequest = PeaceNest.Api.Features.FamilyMilestones.CreateMilestone.CreateMilestoneStepRequest;
using GetMilestoneResponse = PeaceNest.Api.Features.FamilyMilestones.GetMilestone.Response;
using ListMilestonesResponse = PeaceNest.Api.Features.FamilyMilestones.ListMilestones.Response;
using UpdateMilestoneStepCompletionRequest = PeaceNest.Api.Features.FamilyMilestones.UpdateMilestoneStepCompletion.Request;
using UpdateMilestoneStepCompletionResponse = PeaceNest.Api.Features.FamilyMilestones.UpdateMilestoneStepCompletion.Response;
using UpdateMilestoneRequest = PeaceNest.Api.Features.FamilyMilestones.UpdateMilestone.Request;
using UpdateMilestoneResponse = PeaceNest.Api.Features.FamilyMilestones.UpdateMilestone.Response;
using UpdateMilestoneStepRequest = PeaceNest.Api.Features.FamilyMilestones.UpdateMilestone.UpdateMilestoneStepRequest;

namespace PeaceNest.Api.Tests.Integration.Features.FamilyMilestones;

public sealed class FamilyMilestonesEndpointTests
{
    [Fact]
    public async Task CreateMilestone_WritesFamilyPlanMilestoneDetailsAndGoalSteps()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var client = CreateAuthenticatedClient(
            factory,
            "aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa",
            "parent@example.test");
        var family = await CreateFamilyAsync(client, "Milestone Nest");
        var request = NewCreateRequest(
            "Sunday family dinner",
            [
                new("Choose recipe", "Pick one meal together.", 2),
                new("Set table", null, 1)
            ]);

        using var response = await client.PostAsJsonAsync($"/families/{family.Id}/milestones", request);
        var payload = await response.Content.ReadFromJsonAsync<CreateMilestoneResponse>();

        Assert.True(
            response.StatusCode == HttpStatusCode.Created,
            await response.Content.ReadAsStringAsync());
        Assert.NotNull(payload);
        Assert.Equal(family.Id, payload.Milestone.FamilyId);
        Assert.Equal("Sunday family dinner", payload.Milestone.Title);
        Assert.Equal(PlanStatus.Active, payload.Milestone.Status);
        Assert.Equal("habit", payload.Milestone.MilestoneType);
        Assert.True(payload.Milestone.IncludeInRecap);
        Assert.Equal(["Set table", "Choose recipe"], payload.Milestone.Steps.Select(step => step.Title));

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var plan = await dbContext.FamilyPlans
            .Include(item => item.MilestoneDetails)
            .Include(item => item.GoalSteps)
            .SingleAsync();

        Assert.Equal(payload.Milestone.Id, plan.Id);
        Assert.Equal(PlanType.Milestone, plan.PlanType);
        Assert.NotNull(plan.MilestoneDetails);
        Assert.Equal(2, plan.GoalSteps.Count);
        Assert.All(plan.GoalSteps, step => Assert.False(step.IsCompleted));
    }

    [Fact]
    public async Task ListMilestones_ReturnsOnlyFamilyScopedMilestonePlans()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var firstClient = CreateAuthenticatedClient(
            factory,
            "bbbbbbbb-1111-1111-1111-bbbbbbbbbbbb",
            "first@example.test");
        using var secondClient = CreateAuthenticatedClient(
            factory,
            "bbbbbbbb-2222-2222-2222-bbbbbbbbbbbb",
            "second@example.test");
        var firstFamily = await CreateFamilyAsync(firstClient, "First Nest");
        var secondFamily = await CreateFamilyAsync(secondClient, "Second Nest");
        await CreateMilestoneAsync(firstClient, firstFamily.Id, NewCreateRequest("Family reunion"));
        await CreateMilestoneAsync(secondClient, secondFamily.Id, NewCreateRequest("Private reflection"));

        using var response = await firstClient.GetAsync($"/families/{firstFamily.Id}/milestones");
        var payload = await response.Content.ReadFromJsonAsync<ListMilestonesResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        var milestone = Assert.Single(payload.Milestones);
        Assert.Equal("Family reunion", milestone.Title);
        Assert.Equal(firstFamily.Id, milestone.FamilyId);
    }

    [Fact]
    public async Task GetMilestone_ReturnsCreatedMilestoneWithChecklistSteps()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var client = CreateAuthenticatedClient(
            factory,
            "cccccccc-1111-1111-1111-cccccccccccc",
            "parent@example.test");
        var family = await CreateFamilyAsync(client, "Detail Nest");
        var created = await CreateMilestoneAsync(
            client,
            family.Id,
            NewCreateRequest("Visit grandparents", [new("Plan weekend", null, null)]));

        using var response = await client.GetAsync($"/families/{family.Id}/milestones/{created.Milestone.Id}");
        var payload = await response.Content.ReadFromJsonAsync<GetMilestoneResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(created.Milestone.Id, payload.Milestone.Id);
        Assert.Equal("Visit grandparents", payload.Milestone.Title);
        var step = Assert.Single(payload.Milestone.Steps);
        Assert.Equal("Plan weekend", step.Title);
        Assert.Null(step.CompletedByUserId);
    }

    [Fact]
    public async Task CreateMilestone_RejectsInvalidProgress()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var client = CreateAuthenticatedClient(
            factory,
            "dddddddd-1111-1111-1111-dddddddddddd",
            "parent@example.test");
        var family = await CreateFamilyAsync(client, "Validation Nest");

        using var response = await client.PostAsJsonAsync(
            $"/families/{family.Id}/milestones",
            NewCreateRequest("Too far", progressPercent: 101));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            400,
            ErrorCodes.ValidationFailed);
    }

    [Fact]
    public async Task UpdateMilestoneStepCompletion_AllowsChildMemberAndRecalculatesProgress()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(
            factory,
            "dddddddd-2222-2222-2222-dddddddddddd",
            "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Checklist Nest");
        var created = await CreateMilestoneAsync(
            ownerClient,
            family.Id,
            NewCreateRequest(
                "Visit grandparents",
                [
                    new("Pick date", null, 1),
                    new("Call grandparents", null, 2)
                ]));
        var childUser = await AddMemberAsync(
            factory,
            family.Id,
            "dddddddd-3333-3333-3333-dddddddddddd",
            "child@example.test",
            FamilyMemberRole.ChildMember);
        using var childClient = CreateAuthenticatedClient(
            factory,
            childUser.SupabaseUserId.ToString(),
            childUser.Email);
        var step = created.Milestone.Steps.First();

        using var response = await childClient.PutAsJsonAsync(
            $"/families/{family.Id}/milestones/{created.Milestone.Id}/steps/{step.Id}/completion",
            new UpdateMilestoneStepCompletionRequest(true));
        var payload = await response.Content.ReadFromJsonAsync<UpdateMilestoneStepCompletionResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(50, payload.Milestone.ProgressPercent);
        var updatedStep = payload.Milestone.Steps.Single(candidate => candidate.Id == step.Id);
        Assert.True(updatedStep.IsCompleted);
        Assert.Equal(childUser.Id, updatedStep.CompletedByUserId);
        Assert.NotNull(updatedStep.CompletedAt);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var plan = await dbContext.FamilyPlans
            .Include(candidate => candidate.GoalSteps)
            .SingleAsync(candidate => candidate.Id == created.Milestone.Id);

        Assert.Equal(50, plan.ProgressPercent);
        Assert.Equal(childUser.Id, plan.GoalSteps.Single(candidate => candidate.Id == step.Id).CompletedByUserId);
    }

    [Fact]
    public async Task UpdateMilestoneStepCompletion_RejectsViewerRole()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(
            factory,
            "dddddddd-4444-4444-4444-dddddddddddd",
            "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Checklist Permission Nest");
        var created = await CreateMilestoneAsync(
            ownerClient,
            family.Id,
            NewCreateRequest("Sunday dinner", [new("Set table", null, 1)]));
        var viewerUser = await AddMemberAsync(
            factory,
            family.Id,
            "dddddddd-5555-5555-5555-dddddddddddd",
            "viewer@example.test",
            FamilyMemberRole.Viewer);
        using var viewerClient = CreateAuthenticatedClient(
            factory,
            viewerUser.SupabaseUserId.ToString(),
            viewerUser.Email);
        var step = created.Milestone.Steps.Single();

        using var response = await viewerClient.PutAsJsonAsync(
            $"/families/{family.Id}/milestones/{created.Milestone.Id}/steps/{step.Id}/completion",
            new UpdateMilestoneStepCompletionRequest(true));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            403,
            ErrorCodes.AuthorizationDenied);
    }

    [Fact]
    public async Task CreateMilestone_RejectsViewerRole()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(
            factory,
            "eeeeeeee-1111-1111-1111-eeeeeeeeeeee",
            "owner@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Permission Nest");
        var viewerUser = await AddMemberAsync(
            factory,
            family.Id,
            "eeeeeeee-2222-2222-2222-eeeeeeeeeeee",
            "viewer@example.test",
            FamilyMemberRole.Viewer);
        using var viewerClient = CreateAuthenticatedClient(
            factory,
            viewerUser.SupabaseUserId.ToString(),
            viewerUser.Email);

        using var response = await viewerClient.PostAsJsonAsync(
            $"/families/{family.Id}/milestones",
            NewCreateRequest("Viewer milestone"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            403,
            ErrorCodes.AuthorizationDenied);
    }

    [Fact]
    public async Task ListMilestones_RejectsOutsider()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(
            factory,
            "ffffffff-1111-1111-1111-ffffffffffff",
            "owner@example.test");
        using var outsiderClient = CreateAuthenticatedClient(
            factory,
            "ffffffff-2222-2222-2222-ffffffffffff",
            "outsider@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Private Nest");

        using var response = await outsiderClient.GetAsync($"/families/{family.Id}/milestones");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            403,
            ErrorCodes.AuthorizationDenied);
    }

    [Fact]
    public async Task UpdateMilestone_ReconcilesChecklistWithoutLosingCompletionHistory()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var client = CreateAuthenticatedClient(
            factory,
            "abababab-1111-1111-1111-abababababab",
            "parent@example.test");
        var family = await CreateFamilyAsync(client, "Growing Nest");
        var created = await CreateMilestoneAsync(
            client,
            family.Id,
            NewCreateRequest("Family dinner", [new("Choose day", null, 1), new("Pick recipe", null, 2)]));
        var completedStep = created.Milestone.Steps.ElementAt(0);
        var removedStep = created.Milestone.Steps.ElementAt(1);

        using var completionResponse = await client.PutAsJsonAsync(
            $"/families/{family.Id}/milestones/{created.Milestone.Id}/steps/{completedStep.Id}/completion",
            new UpdateMilestoneStepCompletionRequest(true));
        var afterCompletion = await completionResponse.Content.ReadFromJsonAsync<UpdateMilestoneStepCompletionResponse>();
        Assert.NotNull(afterCompletion);

        var request = new UpdateMilestoneRequest(
            "Sunday family dinner",
            "Updated milestone.",
            PriorityRank: 1,
            TargetDate: new DateOnly(2026, 12, 31),
            MilestoneType: "habit",
            CelebrationNotes: null,
            ReflectionPrompt: null,
            IncludeInRecap: true,
            [
                new UpdateMilestoneStepRequest(completedStep.Id, "Choose every Sunday", null, 1),
                new UpdateMilestoneStepRequest(null, "Invite everyone", null, 2)
            ],
            afterCompletion.Milestone.Version);

        using var updateResponse = await client.PutAsJsonAsync(
            $"/families/{family.Id}/milestones/{created.Milestone.Id}",
            request);
        var updated = await updateResponse.Content.ReadFromJsonAsync<UpdateMilestoneResponse>();

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal("Sunday family dinner", updated.Milestone.Title);
        Assert.Equal(2, updated.Milestone.Steps.Count);
        var preserved = updated.Milestone.Steps.Single(step => step.Id == completedStep.Id);
        Assert.True(preserved.IsCompleted);
        Assert.Equal("Choose every Sunday", preserved.Title);
        Assert.DoesNotContain(updated.Milestone.Steps, step => step.Id == removedStep.Id);
        Assert.Contains(updated.Milestone.Steps, step => step.Id != completedStep.Id && step.Id != Guid.Empty);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var softDeleted = await dbContext.GoalSteps
            .IgnoreQueryFilters()
            .SingleAsync(step => step.Id == removedStep.Id);
        Assert.NotNull(softDeleted.DeletedAt);
    }

    private static CreateMilestoneRequest NewCreateRequest(
        string title,
        IReadOnlyCollection<CreateMilestoneStepRequest>? steps = null,
        int progressPercent = 0) =>
        new(
            title,
            "A meaningful family milestone.",
            PriorityRank: null,
            progressPercent,
            TargetDate: null,
            MilestoneType: "habit",
            CelebrationNotes: "Celebrate gently together.",
            ReflectionPrompt: "What made this feel meaningful?",
            IncludeInRecap: true,
            steps ?? []);

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

    private static async Task<CreateMilestoneResponse> CreateMilestoneAsync(
        HttpClient client,
        Guid familyId,
        CreateMilestoneRequest request)
    {
        using var response = await client.PostAsJsonAsync($"/families/{familyId}/milestones", request);
        var payload = await response.Content.ReadFromJsonAsync<CreateMilestoneResponse>();

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
