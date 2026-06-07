using NpgsqlTypes;

namespace PeaceNest.Api.Common.Database.Entities;

public enum FamilyMemberStatus
{
    [PgName("active")]
    Active = 0,

    [PgName("removed")]
    Removed = 1
}
