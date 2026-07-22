using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.Notifications;

public static class NotificationResponseProjection
{
    public static NotificationResponse FromNotification(Notification notification) =>
        new(
            notification.Id,
            notification.FamilyId,
            notification.RecipientUserId,
            notification.ActorUserId,
            notification.ActorUser?.DisplayName,
            notification.Type,
            notification.Title,
            notification.Body,
            notification.RelatedPlanId,
            notification.RelatedCommentId,
            notification.RelatedRecapId,
            notification.RelatedJoinRequestId,
            notification.ReadAt,
            notification.CreatedAt);
}
