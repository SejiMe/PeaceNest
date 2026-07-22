using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Common.Localization;
using PeaceNest.Api.Common.RateLimiting;

namespace PeaceNest.Api.Features.WantsAndNeeds.CreateWantOrNeed;

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
        Post("/families/{familyId:guid}/wants-needs");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(builder => builder.RequireRateLimiting(RateLimitPolicyNames.Write));
        Description(builder => builder.WithTags("Wants & Needs"));
        Summary(summary =>
        {
            summary.Summary = "Create a want or need.";
            summary.Description = "Creates a Wants & Needs family plan with lightweight manual progress.";
            summary.Responses[201] = "The want or need was created.";
            summary.Responses[400] = "The request was invalid.";
            summary.Responses[403] = "The authenticated family member cannot create family plans.";
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
            "You do not have permission to create Wants & Needs for this family workspace.",
            ct);

        var title = request.Title.Trim();
        var description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        var currency = request.EstimatedCostAmount is null
            ? null
            : string.IsNullOrWhiteSpace(request.EstimatedCostCurrency)
                ? await _dbContext.Families
                    .Where(family => family.Id == familyId)
                    .Select(family => family.PreferredCurrency)
                    .SingleAsync(ct)
                : FamilyCurrencies.Normalize(request.EstimatedCostCurrency);
        var priorityScore = WantOrNeedPriorityScore.Calculate(
            request.Kind,
            request.UrgencyLevel,
            request.ImportanceLevel,
            request.EmotionalValueLevel,
            request.EstimatedCostAmount);

        var plan = new FamilyPlan
        {
            Id = Guid.NewGuid(),
            FamilyId = familyId,
            CreatedByUserId = user.Id,
            PlanType = PlanType.WantNeed,
            Title = title,
            Description = description,
            Status = PlanStatus.Active,
            PriorityRank = request.PriorityRank,
            PriorityScore = priorityScore,
            ProgressPercent = request.ProgressPercent,
            TargetDate = request.TargetDate,
            WantNeedDetails = new WantNeedDetails
            {
                Kind = request.Kind,
                EstimatedCostAmount = request.EstimatedCostAmount,
                EstimatedCostCurrency = currency,
                UrgencyLevel = request.UrgencyLevel,
                ImportanceLevel = request.ImportanceLevel,
                EmotionalValueLevel = request.EmotionalValueLevel,
                DesiredByDate = request.DesiredByDate
            }
        };

        _dbContext.FamilyPlans.Add(plan);
        await _dbContext.SaveChangesAsync(ct);

        await Send.CreatedAtAsync(
            nameof(GetWantOrNeed.Endpoint),
            new { familyId, wantOrNeedId = plan.Id },
            new Response(WantOrNeedResponseProjection.FromPlan(plan)),
            cancellation: ct);
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

        if (request.ProgressPercent is < 0 or > 100)
        {
            failures.Add(new ValidationFailure("progressPercent", "Progress must be between 0 and 100."));
        }

        if (request.EstimatedCostAmount is < 0)
        {
            failures.Add(new ValidationFailure("estimatedCostAmount", "Estimated cost cannot be negative."));
        }

        if (!string.IsNullOrWhiteSpace(request.EstimatedCostCurrency) &&
            !FamilyCurrencies.IsSupported(request.EstimatedCostCurrency))
        {
            failures.Add(new ValidationFailure("estimatedCostCurrency", "Select PHP, SGD, or USD for the estimate."));
        }

        if (!Enum.IsDefined(request.UrgencyLevel))
        {
            failures.Add(new ValidationFailure("urgencyLevel", "Urgency level is required."));
        }

        if (!Enum.IsDefined(request.ImportanceLevel))
        {
            failures.Add(new ValidationFailure("importanceLevel", "Importance level is required."));
        }

        if (!Enum.IsDefined(request.EmotionalValueLevel))
        {
            failures.Add(new ValidationFailure("emotionalValueLevel", "Emotional value level is required."));
        }

        if (failures.Count > 0)
        {
            throw new ValidationAppException("Want or need request is invalid.", failures);
        }
    }

}
