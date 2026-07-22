using NpgsqlTypes;

namespace PeaceNest.Api.Common.Database.Entities;

public enum FamilyJoinCodeStatus
{
    [PgName("active")]
    Active = 0,

    [PgName("revoked")]
    Revoked = 1,

    [PgName("expired")]
    Expired = 2,

    [PgName("capacity_reached")]
    CapacityReached = 3
}
