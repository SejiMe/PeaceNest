using NpgsqlTypes;

namespace PeaceNest.Api.Common.Database.Entities;

public enum ActivityType
{
    [PgName("family_created")]
    FamilyCreated = 0,

    [PgName("family_plan_created")]
    FamilyPlanCreated = 1,

    [PgName("plan_updated")]
    PlanUpdated = 2,

    [PgName("plan_completed")]
    PlanCompleted = 3,

    [PgName("comment_added")]
    CommentAdded = 4,

    [PgName("vote_cast")]
    VoteCast = 5,

    [PgName("monthly_recap_generated")]
    MonthlyRecapGenerated = 6
}
