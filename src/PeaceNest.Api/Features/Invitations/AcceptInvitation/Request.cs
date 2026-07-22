namespace PeaceNest.Api.Features.Invitations.AcceptInvitation;

public sealed record Request(
    string? InvitationToken = null,
    string? InvitationCode = null);
