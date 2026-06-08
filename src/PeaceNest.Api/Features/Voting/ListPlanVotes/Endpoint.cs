using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;

namespace PeaceNest.Api.Features.Voting.ListPlanVotes;

public sealed class Endpoint : EndpointWithoutRequest<Response>
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
        Get("/families/{familyId:guid}/plans/{planId:guid}/votes");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(builder => builder.WithTags("Voting"));
        Summary(summary =>
        {
            summary.Summary = "List plan votes.";
            summary.Description = "Returns vote participation and a lightweight summary for a family plan.";
            summary.Responses[200] = "The plan votes were returned.";
            summary.Responses[403] = "The authenticated user is not a member of this family workspace.";
            summary.Responses[404] = "The family plan was not found.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var familyId = Route<Guid>("familyId");
        var planId = Route<Guid>("planId");
        var authenticatedUser = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticatedUser, ct);
        await _familyMembershipAuthorizer.RequireActiveMembershipAsync(
            familyId,
            user.Id,
            FamilyRolePermissions.CanViewFamily,
            "You do not have permission to view votes for this family plan.",
            ct);

        var planExists = await _dbContext.FamilyPlans
            .AsNoTracking()
            .AnyAsync(plan => plan.Id == planId && plan.FamilyId == familyId, ct);

        if (!planExists)
        {
            throw new NotFoundAppException("Family plan was not found.");
        }

        var votes = await _dbContext.PlanVotes
            .AsNoTracking()
            .Include(vote => vote.User)
            .Where(vote => vote.PlanId == planId)
            .OrderBy(vote => vote.CreatedAt)
            .ToListAsync(ct);

        var voteResponses = votes
            .Select(PlanVoteResponseProjection.FromVote)
            .ToArray();

        var summary = new PlanVoteSummaryResponse(
            planId,
            votes.Count,
            votes.Count(vote => vote.VoteValue == VoteValue.Support),
            votes.Count(vote => vote.VoteValue == VoteValue.Neutral),
            votes.Count(vote => vote.VoteValue == VoteValue.NotNow),
            votes.Sum(vote => vote.PriorityPoints),
            voteResponses);

        await Send.OkAsync(new Response(summary), ct);
    }
}
