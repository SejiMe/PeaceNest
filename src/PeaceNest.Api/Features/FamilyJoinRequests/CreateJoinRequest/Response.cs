namespace PeaceNest.Api.Features.FamilyJoinRequests.CreateJoinRequest;

public sealed record Response(FamilyJoinRequestResponse JoinRequest, bool WasAlreadyPending);
