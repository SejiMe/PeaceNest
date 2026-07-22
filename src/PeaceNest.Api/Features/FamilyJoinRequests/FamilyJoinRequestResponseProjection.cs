using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.FamilyJoinRequests;

public static class FamilyJoinRequestResponseProjection
{
    public static FamilyJoinRequestResponse FromRequest(
        FamilyJoinRequest request,
        DateTimeOffset now) =>
        new(
            request.Id,
            request.FamilyId,
            request.Family.Name,
            request.RequesterUser.DisplayName,
            MaskEmail(request.RequesterUser.Email),
            request.RequesterUser.AvatarUrl,
            request.Status == FamilyJoinRequestStatus.Pending && request.ExpiresAt <= now
                ? FamilyJoinRequestStatus.Expired
                : request.Status,
            request.ApprovedRole,
            request.CreatedAt,
            request.ExpiresAt,
            request.ReviewedAt);

    public static string MaskEmail(string email)
    {
        var separator = email.IndexOf('@');
        if (separator <= 0 || separator == email.Length - 1)
        {
            return "***";
        }

        var local = email[..separator];
        var visible = local[..1];
        return $"{visible}***{email[separator..]}";
    }
}
