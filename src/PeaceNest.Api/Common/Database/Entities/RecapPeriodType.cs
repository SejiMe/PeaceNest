using NpgsqlTypes;

namespace PeaceNest.Api.Common.Database.Entities;

public enum RecapPeriodType
{
    [PgName("monthly")]
    Monthly = 0
}
