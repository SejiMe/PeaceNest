using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.Voting;

public static class PlanVoteResponseProjection
{
    public static PlanVoteResponse FromVote(PlanVote vote) =>
        new(
            vote.Id,
            vote.PlanId,
            vote.UserId,
            vote.User.DisplayName,
            vote.VoteValue,
            vote.PriorityPoints,
            vote.Note,
            vote.CreatedAt,
            vote.UpdatedAt);
}
