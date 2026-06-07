using NpgsqlTypes;

namespace PeaceNest.Api.Common.Database.Entities;

public enum FamilyInvitationStatus
{
    [PgName("pending")]
    Pending = 0,

    [PgName("accepted")]
    Accepted = 1,

    [PgName("expired")]
    Expired = 2,

    [PgName("revoked")]
    Revoked = 3
}
