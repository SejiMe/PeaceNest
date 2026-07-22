using NpgsqlTypes;

namespace PeaceNest.Api.Common.Database.Entities;

public enum FamilyRecoveryCodeStatus
{
    [PgName("active")]
    Active = 0,

    [PgName("used")]
    Used = 1
}
