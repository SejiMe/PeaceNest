using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.Voting;

public sealed record PlanVoteResponse(
    Guid Id,
    Guid PlanId,
    Guid UserId,
    string UserDisplayName,
    VoteValue VoteValue,
    int PriorityPoints,
    string? Note,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
