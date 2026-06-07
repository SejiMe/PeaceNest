using NpgsqlTypes;

namespace PeaceNest.Api.Common.Database.Entities;

public enum VoteValue
{
    [PgName("support")]
    Support = 0,

    [PgName("neutral")]
    Neutral = 1,

    [PgName("not_now")]
    NotNow = 2
}
