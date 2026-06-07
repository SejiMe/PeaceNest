namespace PeaceNest.Api.Features.WantsAndNeeds.ListWantsAndNeeds;

public sealed record Response(IReadOnlyCollection<WantOrNeedResponse> WantsAndNeeds);
