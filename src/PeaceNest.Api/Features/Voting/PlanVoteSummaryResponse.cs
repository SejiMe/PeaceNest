namespace PeaceNest.Api.Features.Voting;

public sealed record PlanVoteSummaryResponse(
    Guid PlanId,
    int TotalVotes,
    int SupportCount,
    int NeutralCount,
    int NotNowCount,
    int TotalPriorityPoints,
    IReadOnlyCollection<PlanVoteResponse> Votes);
