using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.Invitations.CreateInvitation;

public sealed record Response(
    Guid Id,
    Guid FamilyId,
    string InvitedEmail,
    FamilyMemberRole InvitedRole,
    FamilyInvitationStatus Status,
    DateTimeOffset ExpiresAt,
    string InvitationToken,
    string InvitationCode);
