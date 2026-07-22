namespace PeaceNest.Api.Features.FamilyJoinRequests.ListFamilyJoinRequests;

public sealed record Response(IReadOnlyCollection<FamilyJoinRequestResponse> JoinRequests);
