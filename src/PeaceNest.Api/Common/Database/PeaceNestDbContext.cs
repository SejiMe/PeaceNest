using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Database.Entities;
using System.Reflection;

namespace PeaceNest.Api.Common.Database;

public sealed class PeaceNestDbContext : DbContext
{
    private readonly TimeProvider _timeProvider;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("citext");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.ApplyPeaceNestConventions();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ChangeTracker.ApplyPeaceNestPersistenceConventions(_timeProvider.GetUtcNow());
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ChangeTracker.ApplyPeaceNestPersistenceConventions(_timeProvider.GetUtcNow());
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}
