using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Common.RateLimiting;

namespace PeaceNest.Api.Features.FamilyMilestones.UpdateMilestone;

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
        Put("/families/{familyId:guid}/milestones/{milestoneId:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(builder => builder.RequireRateLimiting(RateLimitPolicyNames.Write));
        Description(builder => builder.WithTags("Family Milestones"));
        Summary(summary =>
        {
            summary.Summary = "Edit an active family milestone.";
            summary.Responses[200] = "The family milestone was updated.";
            summary.Responses[409] = "The milestone changed after it was opened.";
            summary.Responses[422] = "Completed and archived milestones are read-only.";
        });
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        ValidateRequest(request);

        var familyId = Route<Guid>("familyId");
        var milestoneId = Route<Guid>("milestoneId");
        var authenticatedUser = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticatedUser, ct);
        await _familyMembershipAuthorizer.RequireActiveMembershipAsync(
            familyId,
            user.Id,
            FamilyRolePermissions.CanUpdateFamilyPlans,
            "You do not have permission to edit Family Milestones for this family workspace.",
            ct);

        var milestone = await _dbContext.FamilyPlans
            .Include(plan => plan.MilestoneDetails)
            .Include(plan => plan.GoalSteps)
            .SingleOrDefaultAsync(plan =>
                plan.Id == milestoneId &&
                plan.FamilyId == familyId &&
                plan.PlanType == PlanType.Milestone,
                ct);

        if (milestone is null)
        {
            throw new NotFoundAppException("Family milestone was not found.");
        }

        if (milestone.Status != PlanStatus.Active)
        {
            throw new DomainRuleAppException("Completed and archived Family Milestones are read-only.");
        }

        var existingSteps = milestone.GoalSteps.ToDictionary(step => step.Id);
        var requestedExistingIds = request.Steps
            .Where(step => step.Id.HasValue)
            .Select(step => step.Id!.Value)
            .ToHashSet();
        var unknownIds = requestedExistingIds.Where(id => !existingSteps.ContainsKey(id)).ToArray();
        if (unknownIds.Length > 0)
        {
            throw new ValidationAppException(
                "Family milestone request is invalid.",
                [new ValidationFailure("steps", "One or more checklist steps do not belong to this milestone.")]);
        }

        var versionProperty = _dbContext.Entry(milestone).Property(plan => plan.Version);
        versionProperty.OriginalValue = request.Version;
        versionProperty.IsModified = true;
        milestone.Title = request.Title.Trim();
        milestone.Description = NormalizeText(request.Description);
        milestone.PriorityRank = request.PriorityRank;
        milestone.TargetDate = request.TargetDate;

        var details = milestone.MilestoneDetails!;
        details.MilestoneType = NormalizeText(request.MilestoneType);
        details.CelebrationNotes = NormalizeText(request.CelebrationNotes);
        details.ReflectionPrompt = NormalizeText(request.ReflectionPrompt);
        details.IncludeInRecap = request.IncludeInRecap;

        foreach (var stepRequest in request.Steps)
        {
            if (stepRequest.Id is Guid existingId)
            {
                var step = existingSteps[existingId];
                step.Title = stepRequest.Title.Trim();
                step.Description = NormalizeText(stepRequest.Description);
                step.SortOrder = stepRequest.SortOrder;
                continue;
            }

            milestone.GoalSteps.Add(new GoalStep
            {
                Title = stepRequest.Title.Trim(),
                Description = NormalizeText(stepRequest.Description),
                SortOrder = stepRequest.SortOrder
            });
        }

        foreach (var removedStep in existingSteps.Values.Where(step => !requestedExistingIds.Contains(step.Id)))
        {
            _dbContext.GoalSteps.Remove(removedStep);
        }

        var visibleSteps = milestone.GoalSteps
            .Where(step => !_dbContext.Entry(step).State.Equals(EntityState.Deleted))
            .ToArray();
        if (visibleSteps.Length > 0)
        {
            milestone.ProgressPercent = (int)Math.Round(
                (decimal)visibleSteps.Count(step => step.IsCompleted) / visibleSteps.Length * 100m);
        }

        try
        {
            await _dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictAppException("This family milestone changed after you opened it. Refresh and try again.");
        }

        await Send.OkAsync(new Response(MilestoneResponseProjection.FromPlan(milestone)), ct);
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

        if (request.MilestoneType?.Trim().Length > 120 ||
            request.CelebrationNotes?.Trim().Length > 1000 ||
            request.ReflectionPrompt?.Trim().Length > 1000)
        {
            failures.Add(new ValidationFailure("milestoneDetails", "One or more milestone details are too long."));
        }

        var steps = request.Steps ?? [];
        if (steps.Count > 25)
        {
            failures.Add(new ValidationFailure("steps", "A milestone can have 25 checklist steps or fewer."));
        }

        if (steps.Where(step => step.Id.HasValue).GroupBy(step => step.Id).Any(group => group.Count() > 1))
        {
            failures.Add(new ValidationFailure("steps", "Each existing checklist step can appear only once."));
        }

        for (var index = 0; index < steps.Count; index++)
        {
            var step = steps.ElementAt(index);
            if (string.IsNullOrWhiteSpace(step.Title) || step.Title.Trim().Length > 180)
            {
                failures.Add(new ValidationFailure($"steps[{index}].title", "Step title is required and must be 180 characters or fewer."));
            }

            if (step.Description?.Trim().Length > 1000)
            {
                failures.Add(new ValidationFailure($"steps[{index}].description", "Step description must be 1000 characters or fewer."));
            }

            if (step.SortOrder < 1)
            {
                failures.Add(new ValidationFailure($"steps[{index}].sortOrder", "Step sort order must be 1 or greater."));
            }
        }

        if (request.Version < 1)
        {
            failures.Add(new ValidationFailure("version", "A valid milestone version is required."));
        }

        if (failures.Count > 0)
        {
            throw new ValidationAppException("Family milestone request is invalid.", failures);
        }
    }

    private static string? NormalizeText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
