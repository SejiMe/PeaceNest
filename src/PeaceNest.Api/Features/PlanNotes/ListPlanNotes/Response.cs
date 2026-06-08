namespace PeaceNest.Api.Features.PlanNotes.ListPlanNotes;

public sealed record Response(IReadOnlyCollection<PlanNoteResponse> Notes);
