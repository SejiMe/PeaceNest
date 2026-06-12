namespace PeaceNest.Api.Features.Recaps;

public sealed record MonthlyRecapStatsResponse(
    int TotalPlans,
    int ActivePlans,
    int NewPlans,
    int CompletedPlans,
    int CompletedMilestones,
    int DelayedPlans,
    int NotesAdded,
    int VotesCast);
