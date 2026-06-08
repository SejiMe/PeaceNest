using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.Voting.CastPlanVote;

public sealed record Request(
    VoteValue VoteValue,
    int PriorityPoints,
    string? Note);
