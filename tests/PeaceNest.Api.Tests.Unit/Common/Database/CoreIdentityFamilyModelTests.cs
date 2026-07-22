using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Tests.Unit.Common.Database;

public sealed class CoreIdentityFamilyModelTests
{
    [Fact]
    public void Model_MapsCoreIdentityAndFamilyTablesToLowercaseNames()
    {
        using var dbContext = CreateDbContext();

        Assert.Equal("users", EntityType<User>(dbContext).GetTableName());
        Assert.Equal("families", EntityType<Family>(dbContext).GetTableName());
        Assert.Equal("family_members", EntityType<FamilyMember>(dbContext).GetTableName());
        Assert.Equal("family_invitations", EntityType<FamilyInvitation>(dbContext).GetTableName());
        Assert.Equal("family_recovery_codes", EntityType<FamilyRecoveryCode>(dbContext).GetTableName());
    }

    [Fact]
    public void Model_UsesCitextForCaseInsensitiveEmailColumns()
    {
        using var dbContext = CreateDbContext();

        Assert.Equal("citext", Property<User>(dbContext, nameof(User.Email)).GetColumnType());
        Assert.Equal("citext", Property<FamilyInvitation>(dbContext, nameof(FamilyInvitation.InvitedEmail)).GetColumnType());
    }

    [Fact]
    public void Model_MapsRolesAndStatusesToPostgresEnumColumns()
    {
        using var dbContext = CreateDbContext();

        Assert.Equal("family_member_role", Property<FamilyMember>(dbContext, nameof(FamilyMember.Role)).GetColumnType());
        Assert.Equal("family_member_status", Property<FamilyMember>(dbContext, nameof(FamilyMember.Status)).GetColumnType());
        Assert.Equal("family_member_role", Property<FamilyInvitation>(dbContext, nameof(FamilyInvitation.InvitedRole)).GetColumnType());
        Assert.Equal(
            "family_invitation_status",
            Property<FamilyInvitation>(dbContext, nameof(FamilyInvitation.Status)).GetColumnType());
    }

    [Fact]
    public void FamilyMemberRole_IncludesAllPermissionRolesFromDayOne()
    {
        var roles = Enum.GetValues<FamilyMemberRole>();

        Assert.Contains(FamilyMemberRole.Owner, roles);
        Assert.Contains(FamilyMemberRole.ParentAdmin, roles);
        Assert.Contains(FamilyMemberRole.AdultMember, roles);
        Assert.Contains(FamilyMemberRole.ChildMember, roles);
        Assert.Contains(FamilyMemberRole.Viewer, roles);
        Assert.Equal(5, roles.Length);
    }

    [Fact]
    public void Model_ConfiguresIdentityAndInvitationUniqueness()
    {
        using var dbContext = CreateDbContext();

        var userEntity = EntityType<User>(dbContext);
        var invitationEntity = EntityType<FamilyInvitation>(dbContext);
        var memberEntity = EntityType<FamilyMember>(dbContext);

        AssertUniqueIndex(userEntity, nameof(User.SupabaseUserId), "ux_users_supabase_user_id");
        AssertUniqueIndex(userEntity, nameof(User.Email), "ux_users_active_email", "deleted_at IS NULL");
        AssertUniqueIndex(invitationEntity, nameof(FamilyInvitation.TokenHash), "ux_family_invitations_token_hash");
        AssertUniqueIndex(
            invitationEntity,
            nameof(FamilyInvitation.InvitationCodeHash),
            "ux_family_invitations_invitation_code_hash",
            "invitation_code_hash IS NOT NULL");

        var memberIndex = memberEntity.GetIndexes().Single(index =>
            HasProperties(index, nameof(FamilyMember.FamilyId), nameof(FamilyMember.UserId)));

        Assert.True(memberIndex.IsUnique);
        Assert.Equal("ux_family_members_family_id_user_id", memberIndex.GetDatabaseName());
    }

    [Fact]
    public void Model_AppliesSoftDeleteFiltersToUserAndFamily()
    {
        using var dbContext = CreateDbContext();

        Assert.NotEmpty(EntityType<User>(dbContext).GetDeclaredQueryFilters());
        Assert.NotEmpty(EntityType<Family>(dbContext).GetDeclaredQueryFilters());
        Assert.Empty(EntityType<FamilyMember>(dbContext).GetDeclaredQueryFilters());
        Assert.Empty(EntityType<FamilyInvitation>(dbContext).GetDeclaredQueryFilters());
    }

    [Fact]
    public void Model_IndexesFamilyScopedInvitationLookupWithoutStoringRawToken()
    {
        using var dbContext = CreateDbContext();

        var invitationEntity = EntityType<FamilyInvitation>(dbContext);
        var pendingEmailIndex = invitationEntity.GetIndexes().Single(index =>
            HasProperties(index, nameof(FamilyInvitation.FamilyId), nameof(FamilyInvitation.InvitedEmail)));

        Assert.Equal("ix_family_invitations_pending_email", pendingEmailIndex.GetDatabaseName());
        Assert.Equal("status = 'pending'::family_invitation_status", pendingEmailIndex.GetFilter());
        Assert.Null(invitationEntity.FindProperty("Token"));
        Assert.NotNull(invitationEntity.FindProperty(nameof(FamilyInvitation.TokenHash)));
    }

    [Fact]
    public void Model_StoresOnlyHashedRecoveryCodesWithOneActiveCodePerFamily()
    {
        using var dbContext = CreateDbContext();

        var recoveryEntity = EntityType<FamilyRecoveryCode>(dbContext);
        Assert.Equal(
            "family_recovery_code_status",
            Property<FamilyRecoveryCode>(dbContext, nameof(FamilyRecoveryCode.Status)).GetColumnType());
        AssertUniqueIndex(
            recoveryEntity,
            nameof(FamilyRecoveryCode.CodeHash),
            "ux_family_recovery_codes_code_hash");
        AssertUniqueIndex(
            recoveryEntity,
            nameof(FamilyRecoveryCode.FamilyId),
            "ux_family_recovery_codes_active_family_id",
            "status = 'active'");
        Assert.Null(recoveryEntity.FindProperty("Code"));
        Assert.True(Property<FamilyRecoveryCode>(dbContext, nameof(FamilyRecoveryCode.Version)).IsConcurrencyToken);
    }

    private static PeaceNestDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PeaceNestDbContext>()
            .UseNpgsql(
                "Host=localhost;Database=peacenest_model_tests;Username=postgres;Password=postgres",
                PeaceNestDatabaseExtensions.ConfigurePeaceNestNpgsqlOptions)
            .Options;

        return new PeaceNestDbContext(options, TimeProvider.System);
    }

    private static IEntityType EntityType<TEntity>(DbContext dbContext) =>
        dbContext.Model.FindEntityType(typeof(TEntity))!;

    private static IProperty Property<TEntity>(DbContext dbContext, string name) =>
        EntityType<TEntity>(dbContext).FindProperty(name)!;

    private static void AssertUniqueIndex(
        IEntityType entityType,
        string propertyName,
        string expectedDatabaseName,
        string? expectedFilter = null)
    {
        var index = entityType.GetIndexes().Single(index => HasProperties(index, propertyName));

        Assert.True(index.IsUnique);
        Assert.Equal(expectedDatabaseName, index.GetDatabaseName());
        Assert.Equal(expectedFilter, index.GetFilter());
    }

    private static bool HasProperties(IReadOnlyIndex index, params string[] propertyNames)
    {
        var indexPropertyNames = index.Properties.Select(property => property.Name).ToArray();
        return indexPropertyNames.SequenceEqual(propertyNames);
    }
}
