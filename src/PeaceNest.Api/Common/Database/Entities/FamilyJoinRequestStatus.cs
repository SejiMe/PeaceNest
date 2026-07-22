using NpgsqlTypes;

namespace PeaceNest.Api.Common.Database.Entities;

public enum FamilyJoinRequestStatus
{
    [PgName("pending")]
    Pending = 0,

    [PgName("approved")]
    Approved = 1,

    [PgName("rejected")]
    Rejected = 2,

    [PgName("withdrawn")]
    Withdrawn = 3,

    [PgName("expired")]
    Expired = 4,

    [PgName("cancelled")]
    Cancelled = 5
}
