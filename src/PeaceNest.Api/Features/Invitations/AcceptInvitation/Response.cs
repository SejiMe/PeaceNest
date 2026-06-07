using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.Invitations.AcceptInvitation;

public sealed record Response(
    Guid InvitationId,
    Guid FamilyId,
    string FamilyName,
    FamilyMemberRole Role,
    FamilyMemberStatus MembershipStatus,
    DateTimeOffset AcceptedAt);
