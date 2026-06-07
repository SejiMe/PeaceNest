using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.Invitations.CreateInvitation;

public sealed record Request(
    string InvitedEmail,
    FamilyMemberRole InvitedRole);
