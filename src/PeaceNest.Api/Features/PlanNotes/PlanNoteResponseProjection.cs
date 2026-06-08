using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.PlanNotes;

public static class PlanNoteResponseProjection
{
    public static PlanNoteResponse FromComment(Comment comment) =>
        new(
            comment.Id,
            comment.PlanId,
            comment.AuthorUserId,
            comment.AuthorUser.DisplayName,
            comment.Body,
            comment.CreatedAt,
            comment.UpdatedAt);
}
