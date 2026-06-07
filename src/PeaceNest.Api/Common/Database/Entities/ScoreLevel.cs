using NpgsqlTypes;

namespace PeaceNest.Api.Common.Database.Entities;

public enum ScoreLevel
{
    [PgName("low")]
    Low = 0,

    [PgName("medium")]
    Medium = 1,

    [PgName("high")]
    High = 2
}
