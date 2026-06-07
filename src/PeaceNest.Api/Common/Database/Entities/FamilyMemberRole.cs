using NpgsqlTypes;

namespace PeaceNest.Api.Common.Database.Entities;

public enum FamilyMemberRole
{
    [PgName("owner")]
    Owner = 0,

    [PgName("parent_admin")]
    ParentAdmin = 1,

    [PgName("adult_member")]
    AdultMember = 2,

    [PgName("child_member")]
    ChildMember = 3,

    [PgName("viewer")]
    Viewer = 4
}
