using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Common.RateLimiting;

namespace PeaceNest.Api.Features.Voting.CastPlanVote;

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
        Put("/families/{familyId:guid}/plans/{planId:guid}/vote");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(builder => builder.RequireRateLimiting(RateLimitPolicyNames.Write));
        Description(builder => builder.WithTags("Voting"));
        Summary(summary =>
        {
            summary.Summary = "Cast or update a plan vote.";
            summary.Description = "Stores the authenticated family member's participation signal for a family plan.";
            summary.Responses[200] = "The plan vote was stored.";
            summary.Responses[400] = "The vote request was invalid.";
            summary.Responses[403] = "The authenticated family member cannot vote on this family plan.";
            summary.Responses[404] = "The family plan was not found.";
        });
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        ValidateRequest(request);

        var familyId = Route<Guid>("familyId");
        var planId = Route<Guid>("planId");
        var authenticatedUser = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticatedUser, ct);
        await _familyMembershipAuthorizer.RequireActiveMembershipAsync(
            familyId,
            user.Id,
            FamilyRolePermissions.CanCastPlanVotes,
            "You do not have permission to vote on this family plan.",
            ct);

        var planExists = await _dbContext.FamilyPlans
            .AsNoTracking()
            .AnyAsync(plan => plan.Id == planId && plan.FamilyId == familyId, ct);

        if (!planExists)
        {
            throw new NotFoundAppException("Family plan was not found.");
        }

        var vote = await _dbContext.PlanVotes
            .Include(planVote => planVote.User)
            .SingleOrDefaultAsync(
                planVote => planVote.PlanId == planId && planVote.UserId == user.Id,
                ct);

        if (vote is null)
        {
            vote = new PlanVote
            {
                Id = Guid.NewGuid(),
                PlanId = planId,
                UserId = user.Id,
                User = user
            };

            _dbContext.PlanVotes.Add(vote);
        }

        vote.VoteValue = request.VoteValue;
        vote.PriorityPoints = request.PriorityPoints;
        vote.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();

        await _dbContext.SaveChangesAsync(ct);

        vote.User = user;

        await Send.OkAsync(new Response(PlanVoteResponseProjection.FromVote(vote)), ct);
    }

    private static void ValidateRequest(Request request)
    {
        var failures = new List<ValidationFailure>();

        if (!Enum.IsDefined(request.VoteValue))
        {
            failures.Add(new ValidationFailure("voteValue", "Vote value is not supported."));
        }

        if (request.PriorityPoints is < 0 or > 5)
        {
            failures.Add(new ValidationFailure("priorityPoints", "Priority points must be between 0 and 5."));
        }

        if (request.Note?.Trim().Length > 1000)
        {
            failures.Add(new ValidationFailure("note", "Vote note must be 1000 characters or fewer."));
        }

        if (failures.Count > 0)
        {
            throw new ValidationAppException("Plan vote request is invalid.", failures);
        }
    }
}
