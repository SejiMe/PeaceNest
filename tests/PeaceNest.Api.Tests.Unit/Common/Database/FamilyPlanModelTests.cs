using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Tests.Unit.Common.Database;

public sealed class FamilyPlanModelTests
{
    [Fact]
    public void Model_MapsUnifiedPlanBackboneAndDetailsTables()
    {
        using var dbContext = CreateDbContext();

        Assert.Equal("family_plans", EntityType<FamilyPlan>(dbContext).GetTableName());
        Assert.Equal("want_need_details", EntityType<WantNeedDetails>(dbContext).GetTableName());
        Assert.Equal("milestone_details", EntityType<MilestoneDetails>(dbContext).GetTableName());
        Assert.Equal("goal_steps", EntityType<GoalStep>(dbContext).GetTableName());
        Assert.Equal("plan_votes", EntityType<PlanVote>(dbContext).GetTableName());
        Assert.Equal("comments", EntityType<Comment>(dbContext).GetTableName());
        Assert.Equal("notifications", EntityType<Notification>(dbContext).GetTableName());
        Assert.Equal("recaps", EntityType<Recap>(dbContext).GetTableName());
    }

    [Fact]
    public void Model_UsesPostgresEnumsForPlanAndActivityLanguage()
    {
        using var dbContext = CreateDbContext();

        Assert.Equal("plan_type", Property<FamilyPlan>(dbContext, nameof(FamilyPlan.PlanType)).GetColumnType());
        Assert.Equal("plan_status", Property<FamilyPlan>(dbContext, nameof(FamilyPlan.Status)).GetColumnType());
        Assert.Equal("want_need_kind", Property<WantNeedDetails>(dbContext, nameof(WantNeedDetails.Kind)).GetColumnType());
        Assert.Equal("score_level", Property<WantNeedDetails>(dbContext, nameof(WantNeedDetails.UrgencyLevel)).GetColumnType());
        Assert.Equal("vote_value", Property<PlanVote>(dbContext, nameof(PlanVote.VoteValue)).GetColumnType());
        Assert.Equal("notification_type", Property<Notification>(dbContext, nameof(Notification.Type)).GetColumnType());
        Assert.Equal("activity_type", Property<ActivityLog>(dbContext, nameof(ActivityLog.ActivityType)).GetColumnType());
        Assert.Equal("recap_period_type", Property<Recap>(dbContext, nameof(Recap.PeriodType)).GetColumnType());
    }

    [Fact]
    public void Model_AppliesSoftDeleteFiltersToUserFacingRecords()
    {
        using var dbContext = CreateDbContext();

        Assert.NotEmpty(EntityType<FamilyPlan>(dbContext).GetDeclaredQueryFilters());
        Assert.NotEmpty(EntityType<PlanCategory>(dbContext).GetDeclaredQueryFilters());
        Assert.NotEmpty(EntityType<GoalStep>(dbContext).GetDeclaredQueryFilters());
        Assert.NotEmpty(EntityType<Comment>(dbContext).GetDeclaredQueryFilters());
        Assert.NotEmpty(EntityType<Memory>(dbContext).GetDeclaredQueryFilters());
        Assert.NotEmpty(EntityType<Recap>(dbContext).GetDeclaredQueryFilters());
        Assert.NotEmpty(EntityType<Notification>(dbContext).GetDeclaredQueryFilters());
    }

    [Fact]
    public void Model_ConfiguresFamilyScopedPlanIndexes()
    {
        using var dbContext = CreateDbContext();

        var planEntity = EntityType<FamilyPlan>(dbContext);

        AssertIndex(
            planEntity,
            "ix_family_plans_family_id_plan_type_status",
            nameof(FamilyPlan.FamilyId),
            nameof(FamilyPlan.PlanType),
            nameof(FamilyPlan.Status));

        AssertIndex(
            planEntity,
            "ix_family_plans_active_priority_rank",
            nameof(FamilyPlan.FamilyId),
            nameof(FamilyPlan.PriorityRank));
    }

    [Fact]
    public void Model_EnforcesOneParticipationAndVotePerPlanUser()
    {
        using var dbContext = CreateDbContext();

        AssertUniqueIndex(
            EntityType<PlanParticipant>(dbContext),
            "ux_plan_participants_plan_id_user_id",
            nameof(PlanParticipant.PlanId),
            nameof(PlanParticipant.UserId));

        AssertUniqueIndex(
            EntityType<PlanVote>(dbContext),
            "ux_plan_votes_plan_id_user_id",
            nameof(PlanVote.PlanId),
            nameof(PlanVote.UserId));
    }

    [Fact]
    public void Model_KeepsCommentsReplyCapableAndPlanScoped()
    {
        using var dbContext = CreateDbContext();
        var commentEntity = EntityType<Comment>(dbContext);

        Assert.NotNull(commentEntity.FindProperty(nameof(Comment.ParentCommentId)));
        AssertIndex(commentEntity, "ix_comments_active_plan_id_created_at", nameof(Comment.PlanId), nameof(Comment.CreatedAt));
        AssertIndex(commentEntity, "ix_comments_parent_comment_id", nameof(Comment.ParentCommentId));
    }

    [Fact]
    public void Model_ConfiguresMonthlyRecapUniquenessAndJsonColumns()
    {
        using var dbContext = CreateDbContext();

        Assert.Equal("jsonb", Property<Recap>(dbContext, nameof(Recap.Stats)).GetColumnType());
        Assert.Equal("jsonb", Property<ActivityLog>(dbContext, nameof(ActivityLog.Metadata)).GetColumnType());

        AssertUniqueIndex(
            EntityType<Recap>(dbContext),
            "ux_recaps_active_family_period",
            nameof(Recap.FamilyId),
            nameof(Recap.PeriodType),
            nameof(Recap.PeriodStart));
    }

    [Fact]
    public void Model_ConfiguresNotificationLookupWithoutPrivateContentIndexes()
    {
        using var dbContext = CreateDbContext();
        var notificationEntity = EntityType<Notification>(dbContext);

        Assert.Equal(300, Property<Notification>(dbContext, nameof(Notification.Body)).GetMaxLength());
        AssertIndex(
            notificationEntity,
            "ix_notifications_active_recipient_read_created",
            nameof(Notification.RecipientUserId),
            nameof(Notification.ReadAt),
            nameof(Notification.CreatedAt));
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

    private static void AssertUniqueIndex(IEntityType entityType, string expectedDatabaseName, params string[] propertyNames)
    {
        var index = AssertIndex(entityType, expectedDatabaseName, propertyNames);
        Assert.True(index.IsUnique);
    }

    private static IReadOnlyIndex AssertIndex(
        IEntityType entityType,
        string expectedDatabaseName,
        params string[] propertyNames)
    {
        var index = entityType.GetIndexes().Single(index => HasProperties(index, propertyNames));

        Assert.Equal(expectedDatabaseName, index.GetDatabaseName());
        return index;
    }

    private static bool HasProperties(IReadOnlyIndex index, params string[] propertyNames)
    {
        var indexPropertyNames = index.Properties.Select(property => property.Name).ToArray();
        return indexPropertyNames.SequenceEqual(propertyNames);
    }
}
