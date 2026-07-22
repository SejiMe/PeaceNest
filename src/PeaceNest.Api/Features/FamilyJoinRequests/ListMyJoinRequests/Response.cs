namespace PeaceNest.Api.Features.FamilyJoinRequests.ListMyJoinRequests;

public sealed record Response(IReadOnlyCollection<FamilyJoinRequestResponse> JoinRequests);
