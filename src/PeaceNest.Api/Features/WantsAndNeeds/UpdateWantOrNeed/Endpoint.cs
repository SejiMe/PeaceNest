using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Common.Localization;
using PeaceNest.Api.Common.RateLimiting;

namespace PeaceNest.Api.Features.WantsAndNeeds.UpdateWantOrNeed;

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
        Put("/families/{familyId:guid}/wants-needs/{wantOrNeedId:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(builder => builder.RequireRateLimiting(RateLimitPolicyNames.Write));
        Description(builder => builder.WithTags("Wants & Needs"));
        Summary(summary =>
        {
            summary.Summary = "Edit an active want or need.";
            summary.Responses[200] = "The want or need was updated.";
            summary.Responses[403] = "The family member cannot edit family plans.";
            summary.Responses[409] = "The plan changed after it was opened.";
            summary.Responses[422] = "Completed and archived plans are read-only.";
        });
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        ValidateRequest(request);

        var familyId = Route<Guid>("familyId");
        var planId = Route<Guid>("wantOrNeedId");
        var authenticatedUser = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticatedUser, ct);
        await _familyMembershipAuthorizer.RequireActiveMembershipAsync(
            familyId,
            user.Id,
            FamilyRolePermissions.CanUpdateFamilyPlans,
            "You do not have permission to edit Wants & Needs for this family workspace.",
            ct);

        var plan = await _dbContext.FamilyPlans
            .Include(candidate => candidate.WantNeedDetails)
            .SingleOrDefaultAsync(candidate =>
                candidate.Id == planId &&
                candidate.FamilyId == familyId &&
                candidate.PlanType == PlanType.WantNeed,
                ct);

        if (plan is null)
        {
            throw new NotFoundAppException("Want or need was not found.");
        }

        if (plan.Status != PlanStatus.Active)
        {
            throw new DomainRuleAppException("Completed and archived Wants & Needs are read-only.");
        }

        var versionProperty = _dbContext.Entry(plan).Property(candidate => candidate.Version);
        versionProperty.OriginalValue = request.Version;
        versionProperty.IsModified = true;

        plan.Title = request.Title.Trim();
        plan.Description = NormalizeText(request.Description);
        plan.PriorityRank = request.PriorityRank;
        plan.TargetDate = request.TargetDate;

        var details = plan.WantNeedDetails!;
        details.Kind = request.Kind;
        details.EstimatedCostAmount = request.EstimatedCostAmount;
        details.EstimatedCostCurrency = request.EstimatedCostAmount is null
            ? null
            : FamilyCurrencies.Normalize(request.EstimatedCostCurrency!);
        details.UrgencyLevel = request.UrgencyLevel;
        details.ImportanceLevel = request.ImportanceLevel;
        details.EmotionalValueLevel = request.EmotionalValueLevel;
        details.DesiredByDate = request.DesiredByDate;
        plan.PriorityScore = WantOrNeedPriorityScore.Calculate(
            details.Kind,
            details.UrgencyLevel,
            details.ImportanceLevel,
            details.EmotionalValueLevel,
            details.EstimatedCostAmount);

        try
        {
            await _dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictAppException("This family plan changed after you opened it. Refresh and try again.");
        }

        await Send.OkAsync(new Response(WantOrNeedResponseProjection.FromPlan(plan)), ct);
    }

    private static void ValidateRequest(Request request)
    {
        var failures = new List<ValidationFailure>();

        if (!Enum.IsDefined(request.Kind))
        {
            failures.Add(new ValidationFailure("kind", "Need or want is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            failures.Add(new ValidationFailure("title", "A title is required."));
        }
        else if (request.Title.Trim().Length > 180)
        {
            failures.Add(new ValidationFailure("title", "Title must be 180 characters or fewer."));
        }

        if (request.Description?.Trim().Length > 2000)
        {
            failures.Add(new ValidationFailure("description", "Description must be 2000 characters or fewer."));
        }

        if (request.PriorityRank is < 1)
        {
            failures.Add(new ValidationFailure("priorityRank", "Priority rank must be 1 or greater."));
        }

        if (request.EstimatedCostAmount is < 0)
        {
            failures.Add(new ValidationFailure("estimatedCostAmount", "Estimated cost cannot be negative."));
        }

        if (request.EstimatedCostAmount is not null && !FamilyCurrencies.IsSupported(request.EstimatedCostCurrency))
        {
            failures.Add(new ValidationFailure("estimatedCostCurrency", "Select PHP, SGD, or USD for the estimate."));
        }

        if (!Enum.IsDefined(request.UrgencyLevel) ||
            !Enum.IsDefined(request.ImportanceLevel) ||
            !Enum.IsDefined(request.EmotionalValueLevel))
        {
            failures.Add(new ValidationFailure("priority", "Urgency, importance, and emotional value are required."));
        }

        if (request.Version < 1)
        {
            failures.Add(new ValidationFailure("version", "A valid plan version is required."));
        }

        if (failures.Count > 0)
        {
            throw new ValidationAppException("Want or need request is invalid.", failures);
        }
    }

    private static string? NormalizeText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
