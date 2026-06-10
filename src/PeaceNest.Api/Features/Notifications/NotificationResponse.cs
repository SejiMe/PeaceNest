using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.Notifications;

public sealed record NotificationResponse(
    Guid Id,
    Guid FamilyId,
    Guid RecipientUserId,
    Guid? ActorUserId,
    string? ActorDisplayName,
    NotificationType Type,
    string Title,
    string? Body,
    Guid? RelatedPlanId,
    Guid? RelatedCommentId,
    Guid? RelatedRecapId,
    DateTimeOffset? ReadAt,
    DateTimeOffset CreatedAt);
