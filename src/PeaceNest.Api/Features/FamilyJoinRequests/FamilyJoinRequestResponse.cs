using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.FamilyJoinRequests;

public sealed record FamilyJoinRequestResponse(
    Guid Id,
    Guid FamilyId,
    string FamilyName,
    string RequesterDisplayName,
    string MaskedRequesterEmail,
    string? RequesterAvatarUrl,
    FamilyJoinRequestStatus Status,
    FamilyMemberRole? ApprovedRole,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? ReviewedAt);
