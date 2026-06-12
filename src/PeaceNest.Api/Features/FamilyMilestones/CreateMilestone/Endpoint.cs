using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Common.RateLimiting;

namespace PeaceNest.Api.Features.FamilyMilestones.CreateMilestone;

public sealed class Endpoint : Endpoint<Request, Response>
{
    private readonly CurrentUserService _currentUserService;
    private readonly FamilyMembershipAuthorizer _familyMembershipAuthorizer;
    private readonly PeaceNestDbContext _dbContext;

    public Endpoint(
        CurrentUserService currentUserService,
        FamilyMembershipAuthorizer familyMembershipAuthorizer,
        PeaceNestDbContext dbContext)
    {
        _currentUserService = currentUserService;
        _familyMembershipAuthorizer = familyMembershipAuthorizer;
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Post("/families/{familyId:guid}/milestones");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(builder => builder.RequireRateLimiting(RateLimitPolicyNames.Write));
        Description(builder => builder.WithTags("Family Milestones"));
        Summary(summary =>
        {
            summary.Summary = "Create a family milestone.";
            summary.Description = "Creates a milestone family plan with shared checklist progress markers.";
            summary.Responses[201] = "The family milestone was created.";
            summary.Responses[400] = "The request was invalid.";
            summary.Responses[403] = "The authenticated family member cannot create family milestones.";
        });
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        ValidateRequest(request);

        var familyId = Route<Guid>("familyId");
        var authenticatedUser = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticatedUser, ct);
        await _familyMembershipAuthorizer.RequireActiveMembershipAsync(
            familyId,
            user.Id,
            FamilyRolePermissions.CanCreateFamilyPlans,
            "You do not have permission to create Family Milestones for this family workspace.",
            ct);

        var plan = new FamilyPlan
        {
            Id = Guid.NewGuid(),
            FamilyId = familyId,
            CreatedByUserId = user.Id,
            PlanType = PlanType.Milestone,
            Title = request.Title.Trim(),
            Description = NormalizeText(request.Description),
            Status = PlanStatus.Active,
            PriorityRank = request.PriorityRank,
            PriorityScore = 0m,
            ProgressPercent = request.ProgressPercent,
            TargetDate = request.TargetDate,
            MilestoneDetails = new MilestoneDetails
            {
                MilestoneType = NormalizeText(request.MilestoneType),
                CelebrationNotes = NormalizeText(request.CelebrationNotes),
                ReflectionPrompt = NormalizeText(request.ReflectionPrompt),
                IncludeInRecap = request.IncludeInRecap
            }
        };

        foreach (var step in NormalizeSteps(request.Steps))
        {
            plan.GoalSteps.Add(new GoalStep
            {
                Title = step.Title,
                Description = step.Description,
                SortOrder = step.SortOrder,
                IsCompleted = false
            });
        }

        _dbContext.FamilyPlans.Add(plan);
        await _dbContext.SaveChangesAsync(ct);

        await Send.CreatedAtAsync(
            nameof(GetMilestone.Endpoint),
            new { familyId, milestoneId = plan.Id },
            new Response(MilestoneResponseProjection.FromPlan(plan)),
            cancellation: ct);
    }

    private static void ValidateRequest(Request request)
    {
        var failures = new List<ValidationFailure>();

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            failures.Add(new ValidationFailure("title", "A milestone title is required."));
        }
        else if (request.Title.Trim().Length > 180)
        {
            failures.Add(new ValidationFailure("title", "Milestone title must be 180 characters or fewer."));
        }

        if (request.Description?.Trim().Length > 2000)
        {
            failures.Add(new ValidationFailure("description", "Description must be 2000 characters or fewer."));
        }

        if (request.PriorityRank is < 1)
        {
            failures.Add(new ValidationFailure("priorityRank", "Priority rank must be 1 or greater."));
        }

        if (request.ProgressPercent is < 0 or > 100)
        {
            failures.Add(new ValidationFailure("progressPercent", "Progress must be between 0 and 100."));
        }

        if (request.MilestoneType?.Trim().Length > 120)
        {
            failures.Add(new ValidationFailure("milestoneType", "Milestone type must be 120 characters or fewer."));
        }

        if (request.CelebrationNotes?.Trim().Length > 1000)
        {
            failures.Add(new ValidationFailure("celebrationNotes", "Celebration notes must be 1000 characters or fewer."));
        }

        if (request.ReflectionPrompt?.Trim().Length > 1000)
        {
            failures.Add(new ValidationFailure("reflectionPrompt", "Reflection prompt must be 1000 characters or fewer."));
        }

        var steps = request.Steps ?? [];

        if (steps.Count > 25)
        {
            failures.Add(new ValidationFailure("steps", "A milestone can start with 25 checklist steps or fewer."));
        }

        for (var index = 0; index < steps.Count; index++)
        {
            var step = steps.ElementAt(index);
            var prefix = $"steps[{index}]";

            if (string.IsNullOrWhiteSpace(step.Title))
            {
                failures.Add(new ValidationFailure($"{prefix}.title", "Step title is required."));
            }
            else if (step.Title.Trim().Length > 180)
            {
                failures.Add(new ValidationFailure($"{prefix}.title", "Step title must be 180 characters or fewer."));
            }

            if (step.Description?.Trim().Length > 1000)
            {
                failures.Add(new ValidationFailure($"{prefix}.description", "Step description must be 1000 characters or fewer."));
            }

            if (step.SortOrder is < 1)
            {
                failures.Add(new ValidationFailure($"{prefix}.sortOrder", "Step sort order must be 1 or greater."));
            }
        }

        if (failures.Count > 0)
        {
            throw new ValidationAppException("Family milestone request is invalid.", failures);
        }
    }

    private static IReadOnlyCollection<NormalizedMilestoneStep> NormalizeSteps(
        IReadOnlyCollection<CreateMilestoneStepRequest> steps) =>
        (steps ?? [])
            .Select((step, index) => new NormalizedMilestoneStep(
                step.Title.Trim(),
                NormalizeText(step.Description),
                step.SortOrder ?? index + 1))
            .OrderBy(step => step.SortOrder)
            .ThenBy(step => step.Title)
            .ToArray();

    private static string? NormalizeText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed record NormalizedMilestoneStep(
        string Title,
        string? Description,
        int SortOrder);
}
