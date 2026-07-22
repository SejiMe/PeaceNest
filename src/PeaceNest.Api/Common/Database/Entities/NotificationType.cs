using NpgsqlTypes;

namespace PeaceNest.Api.Common.Database.Entities;

public enum NotificationType
{
    [PgName("family_plan_created")]
    FamilyPlanCreated = 0,

    [PgName("plan_updated")]
    PlanUpdated = 1,

    [PgName("comment_added")]
    CommentAdded = 2,

    [PgName("vote_cast")]
    VoteCast = 3,

    [PgName("milestone_completed")]
    MilestoneCompleted = 4,

    [PgName("monthly_recap_ready")]
    MonthlyRecapReady = 5,

    [PgName("family_join_request_created")]
    FamilyJoinRequestCreated = 6,

    [PgName("family_join_request_approved")]
    FamilyJoinRequestApproved = 7,

    [PgName("family_join_request_rejected")]
    FamilyJoinRequestRejected = 8
}
