using NpgsqlTypes;

namespace PeaceNest.Api.Common.Database.Entities;

public enum PlanStatus
{
    [PgName("active")]
    Active = 0,

    [PgName("completed")]
    Completed = 1,

    [PgName("archived")]
    Archived = 2
}
