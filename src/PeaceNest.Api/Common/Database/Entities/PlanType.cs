using NpgsqlTypes;

namespace PeaceNest.Api.Common.Database.Entities;

public enum PlanType
{
    [PgName("want_need")]
    WantNeed = 0,

    [PgName("milestone")]
    Milestone = 1
}
