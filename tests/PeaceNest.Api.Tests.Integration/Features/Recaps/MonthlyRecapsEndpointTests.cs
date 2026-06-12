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
using GenerateMonthlyRecapResponse = PeaceNest.Api.Features.Recaps.GenerateMonthlyRecap.Response;
using GetMonthlyRecapResponse = PeaceNest.Api.Features.Recaps.GetMonthlyRecap.Response;

namespace PeaceNest.Api.Tests.Integration.Features.Recaps;

public sealed class MonthlyRecapsEndpointTests
{
    [Fact]
    public async Task GenerateMonthlyRecap_CreatesDeterministicMonthlyRecap()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var client = CreateAuthenticatedClient(
            factory,
            "aaaaaaaa-aaaa-7777-7777-aaaaaaaaaaaa",
            "owner@example.test");
        var family = await CreateFamilyAsync(client, "Recap Nest");
        var owner = await FindUserByEmailAsync(factory, "owner@example.test");
        var period = CurrentPeriod();
        var completedPlan = await AddPlanAsync(
            factory,
            family.Id,
            owner.Id,
            "Emergency kit",
            PlanType.WantNeed,
            PlanStatus.Completed,
            completedAt: period.StartAtUtc.AddDays(3));
        await AddPlanAsync(
            factory,
            family.Id,
            owner.Id,
            "Sunday family dinner",
            PlanType.Milestone,
            PlanStatus.Completed,
            completedAt: period.StartAtUtc.AddDays(6));
        await AddPlanAsync(
            factory,
            family.Id,
            owner.Id,
            "House repair",
            PlanType.WantNeed,
            PlanStatus.Active,
            targetDate: period.Start);
        await AddCommentAsync(factory, completedPlan.Id, owner.Id, "This felt good to finish.");
        await AddVoteAsync(factory, completedPlan.Id, owner.Id, VoteValue.Support);

        using var response = await client.PostAsync(
            $"/families/{family.Id}/recaps/monthly/{period.Year}/{period.Month}/generate",
            content: null);
        var payload = await response.Content.ReadFromJsonAsync<GenerateMonthlyRecapResponse>();

        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            await response.Content.ReadAsStringAsync());
        Assert.NotNull(payload);
        Assert.Equal(family.Id, payload.Recap.FamilyId);
        Assert.Equal(period.Start, payload.Recap.PeriodStart);
        Assert.Equal(period.End, payload.Recap.PeriodEnd);
        Assert.Equal(RecapPeriodType.Monthly, payload.Recap.PeriodType);
        Assert.Equal(2, payload.Recap.Stats.CompletedPlans);
        Assert.Equal(1, payload.Recap.Stats.CompletedMilestones);
        Assert.Equal(1, payload.Recap.Stats.DelayedPlans);
        Assert.Equal(1, payload.Recap.Stats.NotesAdded);
        Assert.Equal(1, payload.Recap.Stats.VotesCast);
        Assert.Contains(period.MonthName, payload.Recap.Title);
        Assert.Contains("2 plans completed", payload.Recap.Summary);
        Assert.Contains(payload.Recap.Items, item => item.ItemType == "completed_plan" && item.Title == "Emergency kit");
        Assert.Contains(payload.Recap.Items, item => item.ItemType == "still_growing" && item.Title == "House repair");
    }

    [Fact]
    public async Task GetMonthlyRecap_ReturnsGeneratedRecap()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var client = CreateAuthenticatedClient(
            factory,
            "bbbbbbbb-bbbb-7777-7777-bbbbbbbbbbbb",
            "owner@example.test");
        var family = await CreateFamilyAsync(client, "Read Recap Nest");
        var owner = await FindUserByEmailAsync(factory, "owner@example.test");
        var period = CurrentPeriod();
        await AddPlanAsync(
            factory,
            family.Id,
            owner.Id,
            "Graduation prep",
            PlanType.Milestone,
            PlanStatus.Completed,
            completedAt: period.StartAtUtc.AddDays(4));
        await client.PostAsync(
            $"/families/{family.Id}/recaps/monthly/{period.Year}/{period.Month}/generate",
            content: null);

        using var response = await client.GetAsync($"/families/{family.Id}/recaps/monthly/{period.Year}/{period.Month}");
        var payload = await response.Content.ReadFromJsonAsync<GetMonthlyRecapResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(period.Start, payload.Recap.PeriodStart);
        Assert.Equal(1, payload.Recap.Stats.CompletedPlans);
        Assert.Single(payload.Recap.Items);
    }

    [Fact]
    public async Task GenerateMonthlyRecap_RefreshesExistingRecapForSamePeriod()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var client = CreateAuthenticatedClient(
            factory,
            "cccccccc-cccc-7777-7777-cccccccccccc",
            "owner@example.test");
        var family = await CreateFamilyAsync(client, "Refresh Recap Nest");
        var owner = await FindUserByEmailAsync(factory, "owner@example.test");
        var period = CurrentPeriod();
        var firstPlan = await AddPlanAsync(
            factory,
            family.Id,
            owner.Id,
            "First win",
            PlanType.WantNeed,
            PlanStatus.Completed,
            completedAt: period.StartAtUtc.AddDays(1));
        var first = await GenerateAsync(client, family.Id, period);
        await AddPlanAsync(
            factory,
            family.Id,
            owner.Id,
            "Second win",
            PlanType.WantNeed,
            PlanStatus.Completed,
            completedAt: period.StartAtUtc.AddDays(2));

        var second = await GenerateAsync(client, family.Id, period);

        Assert.Equal(first.Recap.Id, second.Recap.Id);
        Assert.Equal(2, second.Recap.Stats.CompletedPlans);
        Assert.Contains(second.Recap.Items, item => item.PlanId == firstPlan.Id);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();

        Assert.Equal(1, await dbContext.Recaps.CountAsync());
        Assert.Equal(2, await dbContext.RecapItems.CountAsync());
    }

    [Fact]
    public async Task GenerateMonthlyRecap_RejectsInvalidMonth()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var client = CreateAuthenticatedClient(
            factory,
            "dddddddd-dddd-7777-7777-dddddddddddd",
            "owner@example.test");
        var family = await CreateFamilyAsync(client, "Invalid Recap Nest");

        using var response = await client.PostAsync($"/families/{family.Id}/recaps/monthly/2026/13/generate", content: null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            400,
            ErrorCodes.ValidationFailed);
    }

    [Fact]
    public async Task GenerateMonthlyRecap_RejectsOutsider()
    {
        using var factory = TestingApiFactory.WithIsolatedDatabase();
        using var ownerClient = CreateAuthenticatedClient(
            factory,
            "eeeeeeee-eeee-7777-7777-eeeeeeeeeeee",
            "owner@example.test");
        using var outsiderClient = CreateAuthenticatedClient(
            factory,
            "ffffffff-ffff-7777-7777-ffffffffffff",
            "outsider@example.test");
        var family = await CreateFamilyAsync(ownerClient, "Private Recap Nest");
        var period = CurrentPeriod();

        using var response = await outsiderClient.PostAsync(
            $"/families/{family.Id}/recaps/monthly/{period.Year}/{period.Month}/generate",
            content: null);

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

    private static async Task<GenerateMonthlyRecapResponse> GenerateAsync(
        HttpClient client,
        Guid familyId,
        TestRecapPeriod period)
    {
        using var response = await client.PostAsync(
            $"/families/{familyId}/recaps/monthly/{period.Year}/{period.Month}/generate",
            content: null);
        var payload = await response.Content.ReadFromJsonAsync<GenerateMonthlyRecapResponse>();

        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            await response.Content.ReadAsStringAsync());
        Assert.NotNull(payload);
        return payload;
    }

    private static async Task<User> FindUserByEmailAsync(TestingApiFactory factory, string email)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        return await dbContext.Users.SingleAsync(user => user.Email == email);
    }

    private static async Task<FamilyPlan> AddPlanAsync(
        TestingApiFactory factory,
        Guid familyId,
        Guid createdByUserId,
        string title,
        PlanType planType,
        PlanStatus status,
        DateTimeOffset? completedAt = null,
        DateOnly? targetDate = null)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var plan = new FamilyPlan
        {
            Id = Guid.NewGuid(),
            FamilyId = familyId,
            CreatedByUserId = createdByUserId,
            PlanType = planType,
            Title = title,
            Status = status,
            ProgressPercent = status == PlanStatus.Completed ? 100 : 25,
            PriorityScore = 1,
            CompletedAt = completedAt,
            TargetDate = targetDate
        };

        dbContext.FamilyPlans.Add(plan);
        await dbContext.SaveChangesAsync();

        return plan;
    }

    private static async Task AddCommentAsync(
        TestingApiFactory factory,
        Guid planId,
        Guid authorUserId,
        string body)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        dbContext.Comments.Add(new Comment
        {
            PlanId = planId,
            AuthorUserId = authorUserId,
            Body = body
        });
        await dbContext.SaveChangesAsync();
    }

    private static async Task AddVoteAsync(
        TestingApiFactory factory,
        Guid planId,
        Guid userId,
        VoteValue voteValue)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        dbContext.PlanVotes.Add(new PlanVote
        {
            PlanId = planId,
            UserId = userId,
            VoteValue = voteValue,
            PriorityPoints = 3
        });
        await dbContext.SaveChangesAsync();
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

    private static TestRecapPeriod CurrentPeriod()
    {
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        var start = new DateOnly(today.Year, today.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        return new TestRecapPeriod(today.Year, today.Month, start, end);
    }

    private sealed record TestRecapPeriod(int Year, int Month, DateOnly Start, DateOnly End)
    {
        public DateTimeOffset StartAtUtc =>
            new(Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero);

        public string MonthName =>
            Start.ToDateTime(TimeOnly.MinValue).ToString("MMMM yyyy", global::System.Globalization.CultureInfo.InvariantCulture);
    }
}
