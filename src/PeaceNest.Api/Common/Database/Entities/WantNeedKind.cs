using NpgsqlTypes;

namespace PeaceNest.Api.Common.Database.Entities;

public enum WantNeedKind
{
    [PgName("need")]
    Need = 0,

    [PgName("want")]
    Want = 1
}
