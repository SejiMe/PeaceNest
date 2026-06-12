using System.Text.Json;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.RateLimiting;

namespace PeaceNest.Api.Features.Recaps.GenerateMonthlyRecap;

public sealed class Endpoint : EndpointWithoutRequest<Response>
{
    private readonly CurrentUserService _currentUserService;
    private readonly FamilyMembershipAuthorizer _familyMembershipAuthorizer;
    private readonly PeaceNestDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public Endpoint(
        CurrentUserService currentUserService,
        FamilyMembershipAuthorizer familyMembershipAuthorizer,
        PeaceNestDbContext dbContext,
        TimeProvider timeProvider)
    {
        _currentUserService = currentUserService;
        _familyMembershipAuthorizer = familyMembershipAuthorizer;
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public override void Configure()
    {
        Post("/families/{familyId:guid}/recaps/monthly/{year:int}/{month:int}/generate");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(builder => builder.RequireRateLimiting(RateLimitPolicyNames.RecapGeneration));
        Description(builder => builder.WithTags("Recaps"));
        Summary(summary =>
        {
            summary.Summary = "Generate a monthly recap.";
            summary.Description = "Creates or refreshes a deterministic monthly recap for an authorized family member.";
            summary.Responses[200] = "The monthly recap was generated.";
            summary.Responses[400] = "The recap period was invalid.";
            summary.Responses[403] = "The authenticated user is not a member of this family workspace.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var familyId = Route<Guid>("familyId");
        var period = MonthlyRecapPeriod.FromRoute(Route<int>("year"), Route<int>("month"));
        var authenticatedUser = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticatedUser, ct);
        await _familyMembershipAuthorizer.RequireActiveMembershipAsync(
            familyId,
            user.Id,
            FamilyRolePermissions.CanViewFamily,
            "You do not have permission to generate recaps for this family workspace.",
            ct);

        var plans = await _dbContext.FamilyPlans
            .AsNoTracking()
            .Where(plan => plan.FamilyId == familyId)
            .ToListAsync(ct);
        var planIds = plans.Select(plan => plan.Id).ToHashSet();
        var notesAdded = await _dbContext.Comments
            .AsNoTracking()
            .CountAsync(comment => planIds.Contains(comment.PlanId) &&
                comment.CreatedAt >= period.StartAtUtc &&
                comment.CreatedAt < period.ExclusiveEndAtUtc,
                ct);
        var votesCast = await _dbContext.PlanVotes
            .AsNoTracking()
            .CountAsync(vote => planIds.Contains(vote.PlanId) &&
                vote.CreatedAt >= period.StartAtUtc &&
                vote.CreatedAt < period.ExclusiveEndAtUtc,
                ct);

        var completedPlans = plans
            .Where(plan => plan.Status == PlanStatus.Completed &&
                plan.CompletedAt is not null &&
                IsInPeriod(plan.CompletedAt.Value, period))
            .OrderBy(plan => plan.CompletedAt)
            .ThenBy(plan => plan.Title)
            .ToArray();
        var delayedPlans = plans
            .Where(plan => plan.Status == PlanStatus.Active &&
                plan.TargetDate is not null &&
                plan.TargetDate <= period.End)
            .OrderBy(plan => plan.TargetDate)
            .ThenBy(plan => plan.Title)
            .ToArray();
        var newPlans = plans.Count(plan => IsInPeriod(plan.CreatedAt, period));
        var stats = new MonthlyRecapStatsResponse(
            plans.Count,
            plans.Count(plan => plan.Status == PlanStatus.Active),
            newPlans,
            completedPlans.Length,
            completedPlans.Count(plan => plan.PlanType == PlanType.Milestone),
            delayedPlans.Length,
            notesAdded,
            votesCast);

        var now = _timeProvider.GetUtcNow();
        var recap = await _dbContext.Recaps
            .Include(candidate => candidate.Items)
            .SingleOrDefaultAsync(
                candidate => candidate.FamilyId == familyId &&
                    candidate.PeriodType == RecapPeriodType.Monthly &&
                    candidate.PeriodStart == period.Start,
                ct);

        if (recap is null)
        {
            recap = new Recap
            {
                FamilyId = familyId,
                PeriodType = RecapPeriodType.Monthly,
                PeriodStart = period.Start,
                PeriodEnd = period.End
            };

            _dbContext.Recaps.Add(recap);
        }
        else
        {
            _dbContext.RecapItems.RemoveRange(recap.Items);
            recap.Items.Clear();
        }

        recap.Title = $"{period.MonthName} family recap";
        recap.Summary = BuildSummary(period, stats);
        recap.Stats = JsonSerializer.SerializeToDocument(stats);
        recap.GeneratedByUserId = user.Id;
        recap.PublishedAt = now;

        foreach (var item in BuildItems(completedPlans, delayedPlans, now))
        {
            recap.Items.Add(item);
        }

        await _dbContext.SaveChangesAsync(ct);

        await Send.OkAsync(new Response(MonthlyRecapResponseProjection.FromRecap(recap)), ct);
    }

    private static bool IsInPeriod(DateTimeOffset value, MonthlyRecapPeriod period) =>
        value >= period.StartAtUtc && value < period.ExclusiveEndAtUtc;

    private static string BuildSummary(MonthlyRecapPeriod period, MonthlyRecapStatsResponse stats) =>
        $"{period.MonthName}: {stats.CompletedPlans} plans completed, {stats.NotesAdded} notes added, and {stats.VotesCast} votes shared.";

    private static IReadOnlyCollection<RecapItem> BuildItems(
        IReadOnlyCollection<FamilyPlan> completedPlans,
        IReadOnlyCollection<FamilyPlan> delayedPlans,
        DateTimeOffset now)
    {
        var items = new List<RecapItem>();

        foreach (var plan in completedPlans.Take(10))
        {
            items.Add(new RecapItem
            {
                PlanId = plan.Id,
                ItemType = "completed_plan",
                Title = plan.Title,
                Description = "Completed this month.",
                SortOrder = items.Count + 1,
                CreatedAt = now
            });
        }

        foreach (var plan in delayedPlans.Take(10))
        {
            items.Add(new RecapItem
            {
                PlanId = plan.Id,
                ItemType = "still_growing",
                Title = plan.Title,
                Description = "Still growing beyond this month.",
                SortOrder = items.Count + 1,
                CreatedAt = now
            });
        }

        return items;
    }
}
