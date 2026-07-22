using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Database.Entities;
using System.Reflection;

namespace PeaceNest.Api.Common.Database;

public sealed class PeaceNestDbContext : DbContext
{
    private readonly TimeProvider _timeProvider;
    private readonly HashSet<object> _permanentDeletes = new(ReferenceEqualityComparer.Instance);

    public PeaceNestDbContext(
        DbContextOptions<PeaceNestDbContext> options,
        TimeProvider timeProvider)
        : base(options)
    {
        _timeProvider = timeProvider;
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Family> Families => Set<Family>();

    public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();

    public DbSet<FamilyInvitation> FamilyInvitations => Set<FamilyInvitation>();

    public DbSet<FamilyJoinCode> FamilyJoinCodes => Set<FamilyJoinCode>();

    public DbSet<FamilyJoinRequest> FamilyJoinRequests => Set<FamilyJoinRequest>();

    public DbSet<FamilyRecoveryCode> FamilyRecoveryCodes => Set<FamilyRecoveryCode>();

    public DbSet<PlanCategory> PlanCategories => Set<PlanCategory>();

    public DbSet<FamilyPlan> FamilyPlans => Set<FamilyPlan>();

    public DbSet<WantNeedDetails> WantNeedDetails => Set<WantNeedDetails>();

    public DbSet<MilestoneDetails> MilestoneDetails => Set<MilestoneDetails>();

    public DbSet<GoalStep> GoalSteps => Set<GoalStep>();

    public DbSet<PlanParticipant> PlanParticipants => Set<PlanParticipant>();

    public DbSet<PlanVote> PlanVotes => Set<PlanVote>();

    public DbSet<Comment> Comments => Set<Comment>();

    public DbSet<Reaction> Reactions => Set<Reaction>();

    public DbSet<Memory> Memories => Set<Memory>();

    public DbSet<Recap> Recaps => Set<Recap>();

    public DbSet<RecapItem> RecapItems => Set<RecapItem>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("citext");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.ApplyPeaceNestConventions();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ChangeTracker.ApplyPeaceNestPersistenceConventions(_timeProvider.GetUtcNow(), _permanentDeletes);
        var result = base.SaveChanges(acceptAllChangesOnSuccess);
        _permanentDeletes.Clear();
        return result;
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ChangeTracker.ApplyPeaceNestPersistenceConventions(_timeProvider.GetUtcNow(), _permanentDeletes);
        return SaveAndClearPermanentDeletesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    internal void RemovePermanentlyRange(IEnumerable<object> entities)
    {
        var materialized = entities.ToArray();
        foreach (var entity in materialized)
        {
            _permanentDeletes.Add(entity);
        }

        RemoveRange(materialized);
    }

    private async Task<int> SaveAndClearPermanentDeletesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken)
    {
        var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        _permanentDeletes.Clear();
        return result;
    }
}
