namespace PeaceNest.Api.Features.FamilyMilestones.ListMilestones;

public sealed record Response(IReadOnlyCollection<MilestoneResponse> Milestones);
